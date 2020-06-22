using SabberStoneBasicAI.PartialObservation;
using SabberStoneCore.Enums;
using SabberStoneCore.Exceptions;
using SabberStoneCore.Model;
using SabberStoneCore.Model.Entities;
using SabberStoneCore.Model.Zones;
using SabberStoneCore.Tasks.PlayerTasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SabberStoneBasicAI.AIAgents.MyLookaheadEnemy
{
	class CustomScore : Score.Score
	{
		readonly double[] scaling = new double[] {
				21.5,
				33.6,
				41.1,
				19.4,
				54,
				60.5,
				88.5,
				84.7
		};

		public override int Rate()
		{
			if (OpHeroHp < 1)
				return Int32.MaxValue;

			if (HeroHp < 1)
				return Int32.MinValue;

			double score = 0.0;

			score += scaling[0] * HeroHp;
			score -= scaling[1] * OpHeroHp;

			score += scaling[2] * BoardZone.Count;
			score -= scaling[3] * OpBoardZone.Count;

			foreach (Minion boardZoneEntry in BoardZone)
			{
				score += scaling[4] * boardZoneEntry.Health;
				score += scaling[5] * boardZoneEntry.AttackDamage;
			}

			foreach (Minion boardZoneEntry in OpBoardZone)
			{
				score -= scaling[6] * boardZoneEntry.Health;
				score -= scaling[7] * boardZoneEntry.AttackDamage;
			}

			return (int)Math.Round(score);
		}

		public override Func<List<IPlayable>, List<int>> MulliganRule()
		{
			return p => p.Where(t => t.Cost > 3).Select(t => t.Id).ToList();
		}
	}


	class DynamicLookaheadEnemy : AbstractAgent
	{
		public override PlayerTask GetMove(POGame game)
		{
			Controller player = game.CurrentPlayer;
			// Implement a simple Mulligan Rule
			if (player.MulliganState == Mulligan.INPUT)
			{
				List<int> mulligan = new CustomScore().MulliganRule().Invoke(player.Choice.Choices.Select(p => game.getGame().IdEntityDic[p]).ToList());
				return ChooseTask.Mulligan(player, mulligan);
			}


			IEnumerable<KeyValuePair<PlayerTask, POGame>> validOpts = game.Simulate(player.Options()).Where(x => x.Value != null);
			int optcount = validOpts.Count();
			int maxDepth = optcount >= 5 ? (optcount >= 25 ? 2 : 3) : 3;


			if (validOpts.Any())
			{
				PlayerTask task = validOpts.Select(x => score(x, player.PlayerId, maxDepth)).OrderBy(x => x.Value).Last().Key;
				return task;
			}
			else
			{
				return player.Options().First(x => x.PlayerTaskType == PlayerTaskType.END_TURN);
			}

			KeyValuePair<PlayerTask, int> score(KeyValuePair<PlayerTask, POGame> state, int player_id, int max_depth = 3)
			{
				Controller currentPlayer = state.Value.CurrentPlayer;
				if (max_depth > 0)
				{
					if (currentPlayer.PlayerId == player_id)
					{
						IEnumerable<KeyValuePair<PlayerTask, POGame>> subactions = state.Value.Simulate(currentPlayer.Options()).Where(x => x.Value != null);
						List<int> scores = new List<int>();
						foreach (KeyValuePair<PlayerTask, POGame> subaction in subactions)
							scores.Add(score(subaction, player_id, max_depth - 1).Value);
						return new KeyValuePair<PlayerTask, int>(state.Key, scores.Max());
					}
					else
					{
						//simulate opponent game
						POGame gameAltered = createOpponentHand(state.Value.getCopy());
						List<PlayerTask> options = gameAltered.CurrentPlayer.Options();
						IEnumerable<KeyValuePair<PlayerTask, POGame>> subactions = gameAltered.Simulate(options).Where(x => x.Value != null);
						List<int> scores = new List<int>();

						foreach (KeyValuePair<PlayerTask, POGame> subaction in subactions)
							scores.Add(score(subaction, player_id, max_depth - 1).Value);
						return new KeyValuePair<PlayerTask, int>(state.Key, scores.Min());
					}

				}
				else
				{
					return new KeyValuePair<PlayerTask, int>(state.Key, Score(state.Value, player_id));
				}
			}
		}

		private POGame createOpponentHand(POGame game)
		{
			int cardsToAdd = game.CurrentPlayer.HandZone.Count;

			List<Card> deckCards = game.CurrentPlayer.Standard.Where(x => remainInDeck(x, game.CurrentPlayer)).ToList();
			POGame copy = game.getCopy();
			copy.CurrentPlayer.HandZone = new HandZone(copy.CurrentPlayer);
			while (cardsToAdd > 0 && deckCards.Count > 0)
			{
				Card card = deckCards.RandomElement(new Random());
				copy.addCardToHandZone(card, copy.CurrentPlayer);
				cardsToAdd--;
				deckCards = deckCards.Where(x => remainInDeck(x, game.CurrentPlayer)).ToList();
			}
			return copy;
		}

		private bool remainInDeck(Card card, Controller player)
		{
			int occurences = player.HandZone.Count(c => c.Card == card) + player.GraveyardZone.Count(c => c.Card == card) + player.BoardZone.Count(c => c.Card == card);
			return card.Class == player.HeroClass && card.Cost <= player.RemainingMana && occurences < card.MaxAllowedInDeck;
		}

		private List<IPlayable> createZone(Controller opponent, List<Card> predictedCards, IZone zone, ref SetasideZone setasideZone)
		{
			var deck = new List<IPlayable>();
			foreach (Card card in predictedCards)
			{
				var tags = new Dictionary<GameTag, int>();
				tags[GameTag.ENTITY_ID] = opponent.Game.NextId;
				tags[GameTag.CONTROLLER] = opponent.PlayerId;
				tags[GameTag.ZONE] = (int)zone.Type;
				IPlayable playable = null;
				switch (card.Type)
				{
					case CardType.MINION:
						playable = new Minion(opponent, card, tags);
						break;

					case CardType.SPELL:
						playable = new Spell(opponent, card, tags);
						break;

					case CardType.WEAPON:
						playable = new Weapon(opponent, card, tags);
						break;

					case CardType.HERO:
						tags[GameTag.ZONE] = (int)SabberStoneCore.Enums.Zone.PLAY;
						tags[GameTag.CARDTYPE] = card[GameTag.CARDTYPE];
						playable = new Hero(opponent, card, tags);
						break;

					case CardType.HERO_POWER:
						tags[GameTag.COST] = card[GameTag.COST];
						tags[GameTag.ZONE] = (int)SabberStoneCore.Enums.Zone.PLAY;
						tags[GameTag.CARDTYPE] = card[GameTag.CARDTYPE];
						playable = new HeroPower(opponent, card, tags);
						break;

					default:
						throw new EntityException($"Couldn't create entity, because of an unknown cardType {card.Type}.");
				}

				opponent.Game.IdEntityDic.Add(playable.Id, playable);

				// add entity to the appropriate zone if it was given
				zone?.Add(playable);

				if (playable.ChooseOne)
				{
					playable.ChooseOnePlayables[0] = Entity.FromCard(opponent,
							Cards.FromId(playable.Card.Id + "a"),
							new Dictionary<GameTag, int>
							{
								[GameTag.CREATOR] = playable.Id,
								[GameTag.PARENT_CARD] = playable.Id
							},
							setasideZone);
					playable.ChooseOnePlayables[1] = Entity.FromCard(opponent,
							Cards.FromId(playable.Card.Id + "b"),
							new Dictionary<GameTag, int>
							{
								[GameTag.CREATOR] = playable.Id,
								[GameTag.PARENT_CARD] = playable.Id
							},
							setasideZone);
				}
				deck.Add(playable);
			}
			return deck;
		}


		private static int Score(POGame state, int playerId)
		{
			Controller p = state.CurrentPlayer.PlayerId == playerId ? state.CurrentPlayer : state.CurrentOpponent;
			return new CustomScore { Controller = p }.Rate();
		}

		public override void FinalizeAgent()
		{
		}

		public override void FinalizeGame(Game game, Controller controllers)
		{
		}

		public override void InitializeAgent()
		{
		}

		public override void InitializeGame()
		{
		}
	}
}

/*
System.Collections.Generic.KeyNotFoundException: The given key was not present in the dictionary.
   at SabberStoneCore.Model.Entities.EntityList.get_Item(Int32 id) in C:\Users\Hans\Documents\HearthstoneAICompetition2020\SabberStoneCore\src\Model\Entities\EntityList.cs:line 28
   at SabberStoneCore.Model.Game.MainCleanUp() in C:\Users\Hans\Documents\HearthstoneAICompetition2020\SabberStoneCore\src\Model\Game.cs:line 933
   at SabberStoneCore.Model.GameEventManager.NextStepEvent(Game game, Step step) in C:\Users\Hans\Documents\HearthstoneAICompetition2020\SabberStoneCore\src\Model\GameEventManager.cs:line 117
   at SabberStoneCore.Model.Game.set_NextStep(Step value) in C:\Users\Hans\Documents\HearthstoneAICompetition2020\SabberStoneCore\src\Model\Game.cs:line 1378
   at SabberStoneCore.Model.Game.MainEnd() in C:\Users\Hans\Documents\HearthstoneAICompetition2020\SabberStoneCore\src\Model\Game.cs:line 916
   at SabberStoneCore.Tasks.PlayerTasks.EndTurnTask.Process() in C:\Users\Hans\Documents\HearthstoneAICompetition2020\SabberStoneCore\src\Tasks\PlayerTasks\EndTurnTask.cs:line 35
   at SabberStoneCore.Model.Game.Process(PlayerTask gameTask) in C:\Users\Hans\Documents\HearthstoneAICompetition2020\SabberStoneCore\src\Model\Game.cs:line 501
   at SabberStoneBasicAI.PartialObservation.POGame.Simulate(List`1 tasksToSimulate) in C:\Users\Hans\Documents\HearthstoneAICompetition2020\core-extensions\SabberStoneBasicAI\src\PartialObservation\POGame.cs:line 156The given key was not present in the dictionary.
   at SabberStoneBasicAI.PartialObservation.POGame.Simulate(List`1 tasksToSimulate) in C:\Users\Hans\Documents\HearthstoneAICompetition2020\core-extensions\SabberStoneBasicAI\src\PartialObservation\POGame.cs:line 164
   at SabberStoneBasicAI.AIAgents.MyLookaheadEnemy.DynamicLookaheadEnemy.<>c__DisplayClass0_0.<GetMove>g__score|2(KeyValuePair`2 state, Int32 player_id, Int32 max_depth) in C:\Users\Hans\Documents\HearthstoneAICompetition2020\core-extensions\SabberStoneBasicAI\src\AIAgents\DynamicLookaheadEnemy.cs:line 111
   at SabberStoneBasicAI.AIAgents.MyLookaheadEnemy.DynamicLookaheadEnemy.<>c__DisplayClass0_0.<GetMove>g__score|2(KeyValuePair`2 state, Int32 player_id, Int32 max_depth) in C:\Users\Hans\Documents\HearthstoneAICompetition2020\core-extensions\SabberStoneBasicAI\src\AIAgents\DynamicLookaheadEnemy.cs:line 115
   at SabberStoneBasicAI.AIAgents.MyLookaheadEnemy.DynamicLookaheadEnemy.<>c__DisplayClass0_0.<GetMove>b__4(KeyValuePair`2 x) in C:\Users\Hans\Documents\HearthstoneAICompetition2020\core-extensions\SabberStoneBasicAI\src\AIAgents\DynamicLookaheadEnemy.cs:line 85
   at System.Linq.Enumerable.WhereSelectEnumerableIterator`2.MoveNext()
   at System.Linq.OrderedEnumerable`1.TryGetLast(Boolean& found)
   at System.Linq.Enumerable.TryGetLast[TSource](IEnumerable`1 source, Boolean& found)
   at System.Linq.Enumerable.Last[TSource](IEnumerable`1 source)
   at SabberStoneBasicAI.AIAgents.MyLookaheadEnemy.DynamicLookaheadEnemy.GetMove(POGame game) in C:\Users\Hans\Documents\HearthstoneAICompetition2020\core-extensions\SabberStoneBasicAI\src\AIAgents\DynamicLookaheadEnemy.cs:line 85
   at SabberStoneBasicAI.PartialObservation.POGameHandler.PlayGame(Boolean addToGameStats, Boolean debug) in C:\Users\Hans\Documents\HearthstoneAICompetition2020\core-extensions\SabberStoneBasicAI\src\PartialObservation\POGameHandler.cs:line 97
*/
