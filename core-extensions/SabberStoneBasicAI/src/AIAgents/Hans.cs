using System;
using System.Collections.Generic;
using System.Linq;
using SabberStoneBasicAI.PartialObservation;
using SabberStoneCore.Tasks.PlayerTasks;
using SabberStoneCore.Model.Entities;
using SabberStoneCore.Enums;
using System.Diagnostics;
using SabberStoneCore.Model;
using SabberStoneCore.Exceptions;

namespace SabberStoneBasicAI.AIAgents.AgentHans
{
	class Hans : AbstractAgent
	{
		public override void FinalizeAgent()
		{
		}

		public override void FinalizeGame()
		{
		}

		public override PlayerTask GetMove(POGame game)
		{
			Controller player = game.CurrentPlayer;
			// Implement a simple Mulligan Rule
			if (player.MulliganState == Mulligan.INPUT)
			{
				List<int> mulligan = new CustomScore().MulliganRule().Invoke(player.Choice.Choices.Select(p => game.getGame().IdEntityDic[p]).ToList());
				return ChooseTask.Mulligan(player, mulligan);
			}


			//a round is a change of players
			int maxRounds = 3;
			int maxMovesPerRound = 20; //this is simply a safeguard against stackoverflow exceptions for endlessly repeating moves
			int maxSeconds = 6;
			Stopwatch sw = new Stopwatch();
			sw.Start();
			IEnumerable<KeyValuePair<PlayerTask, POGame>> validOpts = game.Simulate(player.Options()).Where(x => x.Value != null);

			if (validOpts.Count() > 1) {
				PlayerTask task = validOpts.Select(x => searchTree(x, player.PlayerId, player.PlayerId, 0, 0)).OrderBy(x => x.Value).Last().Key;
				//Console.WriteLine(task);
				//Console.WriteLine(sw.ElapsedMilliseconds);
				return task;
			}
			else return player.Options().First(x => x.PlayerTaskType == PlayerTaskType.END_TURN);

			KeyValuePair<PlayerTask, int> searchTree(KeyValuePair<PlayerTask, POGame> state, int player_id, int last_player_id, int rounds, int movesRound)
			{
				Controller currentPlayer = state.Value.CurrentPlayer;
				int max_score = 0;
				if (rounds < maxRounds && sw.ElapsedMilliseconds < maxSeconds * 1000 && movesRound < maxMovesPerRound)
				{
					if (currentPlayer.PlayerId != last_player_id)
					{
						++rounds;
						movesRound = 0;
					}
					else ++movesRound;

					if (currentPlayer.PlayerId == player_id)
					{
						max_score = Int32.MinValue;
						IEnumerable<KeyValuePair<PlayerTask, POGame>> subactions = state.Value.Simulate(currentPlayer.Options()).Where(x => x.Value != null);
						subactions = subactions.OrderBy(x => Score(x.Value, player_id));
						if (subactions.Any())
						{
							foreach (KeyValuePair<PlayerTask, POGame> subaction in subactions)
								max_score = Math.Max(max_score, searchTree(subaction, player_id, currentPlayer.PlayerId, rounds, movesRound).Value);
						}
						else
						{
							max_score = Score(state.Value, player_id);
						}
					}
					else
					{					
						//simulate opponent
						max_score = Int32.MaxValue;
						//remove all cards from the handzone
						while (!currentPlayer.HandZone.IsEmpty) currentPlayer.HandZone.Remove(currentPlayer.HandZone.Random);
						/*
						//the initial deck of this player
						List<Card> allDeckCards = currentPlayer.DeckCards;
						List < IPlayable > playedCards = new List<IPlayable>();
						playedCards.AddRange(currentPlayer.GraveyardZone.GetAll());
						playedCards.AddRange(currentPlayer.BoardZone.GetAll());
						//remove already played cards
						foreach (IPlayable playable in playedCards)
						{
							allDeckCards.Remove(playable.Card);
						}

						foreach(Card card in allDeckCards)
						{
							//choose random cards
							IPlayable playableCard = Entity.FromCard(currentPlayer.Controller, card);
							currentPlayer.HandZone.Add(playableCard);
							if (currentPlayer.HandZone.IsFull) break;
						}*/
						IEnumerable<KeyValuePair<PlayerTask, POGame>> subactions = state.Value.Simulate(currentPlayer.Options()).Where(x => x.Value != null);
						subactions = subactions.OrderBy(x => Score(x.Value, player_id)).Reverse();
						if (subactions.Any())
						{
							foreach (KeyValuePair<PlayerTask, POGame> subaction in subactions)
								max_score = Math.Min(max_score, searchTree(subaction, player_id, currentPlayer.PlayerId, rounds, movesRound).Value);
						}
						else
						{
							max_score = Score(state.Value, player_id);
						}
					}
				}
				else
				{
					max_score = Score(state.Value, player_id);
				}
				return new KeyValuePair<PlayerTask, int>(state.Key, max_score);
			}
		}

		private static int Score(POGame state, int playerId)
		{
			Controller p = state.CurrentPlayer.PlayerId == playerId ? state.CurrentPlayer : state.CurrentOpponent;
			return new CustomScore { Controller = p }.Rate();
		}

		public override void InitializeAgent()
		{
		}

		public override void InitializeGame()
		{
		}
	}

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
}
