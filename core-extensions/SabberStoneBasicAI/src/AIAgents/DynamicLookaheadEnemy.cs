﻿using SabberStoneBasicAI.PartialObservation;
using SabberStoneCore.Enums;
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
						POGame gameAltered = createOpponentHand(state.Value);
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

			List<Card> deckCards = game.CurrentPlayer.DeckCards;
			HandZone handZone = new HandZone(game.CurrentPlayer);
			while (cardsToAdd > 0)
			{
				Card card = deckCards.RandomElement(new Random());
				game.addCardToZone(handZone, card, game.CurrentPlayer);
				cardsToAdd--;

			}

			return game;
		}


		private static int Score(POGame state, int playerId)
		{
			Controller p = state.CurrentPlayer.PlayerId == playerId ? state.CurrentPlayer : state.CurrentOpponent;
			return new CustomScore { Controller = p }.Rate();
		}

		public override void FinalizeAgent()
		{
		}

		public override void FinalizeGame()
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
