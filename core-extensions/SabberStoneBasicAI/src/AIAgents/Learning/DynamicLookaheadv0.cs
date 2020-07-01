using System;
using System.Collections.Generic;
using System.Linq;
using SabberStoneBasicAI.PartialObservation;
using SabberStoneCore.Tasks.PlayerTasks;
using SabberStoneCore.Model.Entities;
using SabberStoneCore.Enums;
using System.Timers;
using System.IO;
using CsvHelper;
using System.Globalization;
using SabberStoneCore.Model;
using System.Transactions;

namespace SabberStoneBasicAI.AIAgents.Learning
{
	class DynamicLookaheadOld : AbstractAgent
	{
		public static Timer timer;
		public bool timeIsOver = false;
		private long totalMoves;
		private long totalScore;
		private long maxScore;

		public void OnTimedEvent(object source, ElapsedEventArgs e)
		{
			timeIsOver = true;
		}


		public override void FinalizeAgent()
		{
		}

		public override void FinalizeGame(Game game, Controller myPlayer)
		{
			Console.WriteLine(" Avg. Score: " + totalScore / totalMoves);
			Console.WriteLine(" Max. Score: " + maxScore);
		}

		public override PlayerTask GetMove(POGame game)
		{
			timeIsOver = false;
			timer.Start();
			Controller player = game.CurrentPlayer;
			// Implement a simple Mulligan Rule
			if (player.MulliganState == Mulligan.INPUT)
			{
				List<int> mulligan = new MyScore().MulliganRule().Invoke(player.Choice.Choices.Select(p => game.getGame().IdEntityDic[p]).ToList());
				return ChooseTask.Mulligan(player, mulligan);
			}


			IEnumerable<KeyValuePair<PlayerTask, POGame>> validOpts = game.Simulate(player.Options()).Where(x => x.Value != null);
			int optcount = validOpts.Count();
			int maxDepth = optcount >= 5 ? (optcount >= 25 ? 1 : 2) : 3;
			if (validOpts.Any())
			{
				KeyValuePair<PlayerTask, int> winnerTask = validOpts.Select(x => score(x, player.PlayerId, maxDepth)).OrderBy(x => x.Value).Last();
				totalMoves++;
				maxScore = maxScore > winnerTask.Value ? maxScore : winnerTask.Value;
				totalScore += winnerTask.Value;
				return winnerTask.Key;
			}
			else return player.Options().First(x => x.PlayerTaskType == PlayerTaskType.END_TURN);

			KeyValuePair<PlayerTask, int> score(KeyValuePair<PlayerTask, POGame> state, int player_id, int max_depth = 3)
			{
				int max_score = Int32.MinValue;
				if (max_depth > 0 && state.Value.CurrentPlayer.PlayerId == player_id && !timeIsOver)
				{
					IEnumerable<KeyValuePair<PlayerTask, POGame>> subactions = state.Value.Simulate(state.Value.CurrentPlayer.Options()).Where(x => x.Value != null);

					foreach (KeyValuePair<PlayerTask, POGame> subaction in subactions)
						max_score = Math.Max(max_score, score(subaction, player_id, max_depth - 1).Value);
				}
				else
				{
					max_score = Math.Max(max_score, Score(state.Value, player_id));
				}
				return new KeyValuePair<PlayerTask, int>(state.Key, max_score);
			}
		}

		public override void InitializeAgent()
		{
			timer = new System.Timers.Timer
			{
				Interval = 28000
			};
			timer.Elapsed += OnTimedEvent;
			timer.AutoReset = true;
			timer.Enabled = true;
		}


		public override void InitializeGame()
		{
		}

		private int Score(POGame state, int playerId)
		{
			Controller p = state.CurrentPlayer.PlayerId == playerId ? state.CurrentPlayer : state.CurrentOpponent;
			return new MyScore { Controller = p }.Rate();
		}

		public class MyScore : Score.Score
		{
			public override int Rate()
			{
				if (OpHeroHp < 1)
					return Int32.MaxValue;

				if (HeroHp < 1)
					return Int32.MinValue;

				int result = 0;

				if (OpMinionTotHealthTaunt > 0)
					result += OpMinionTotHealthTaunt * -100;

				if (OpBoardZone.Count == 0 && BoardZone.Count > 0)
					result += 1000;

				if (OpMinionTotHealthTaunt > 0)
					result += MinionTotHealthTaunt * -500;

				result += (BoardZone.Count - OpBoardZone.Count) * 10;

				result += MinionTotAtk * 10;

				result += (HeroHp - OpHeroHp) * 10;

				result += (MinionTotHealth - OpMinionTotHealth) * 10;

				result += (MinionTotAtk - OpMinionTotAtk) * 10;

				result += (MinionTotHealthTaunt - OpMinionTotHealthTaunt) * 25;

				result += (BoardZone.Count - OpBoardZone.Count) * 5;

				//result += Controller.RemainingMana * 5;

				return result;
			}

			public override Func<List<IPlayable>, List<int>> MulliganRule()
			{
				return p => p.Where(t => t.Cost > 3).Select(t => t.Id).ToList();
			}
		}
	}
}
