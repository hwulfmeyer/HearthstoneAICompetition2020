using System;
using System.Collections.Generic;
using System.Linq;
using SabberStoneBasicAI.PartialObservation;
using SabberStoneCore.Tasks.PlayerTasks;
using SabberStoneCore.Model.Entities;
using SabberStoneCore.Enums;
using System.Timers;

namespace SabberStoneBasicAI.AIAgents.Lookahead
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


	class DynamicLookahead : AbstractAgent
	{
		private static Timer timer;
		private bool timeIsOver = false;

		private void OnTimedEvent(object source, ElapsedEventArgs e)
		{
			timeIsOver = true;
		}


		public override void FinalizeAgent()
		{
		}

		public override void FinalizeGame(SabberStoneCore.Model.Game game, Controller controllers)
		{
		}

		public override PlayerTask GetMove(POGame game)
		{
			timeIsOver = false;
			timer.Start();
			Controller player = game.CurrentPlayer;
			// Implement a simple Mulligan Rule
			if (player.MulliganState == Mulligan.INPUT)
			{
				List<int> mulligan = new CustomScore().MulliganRule().Invoke(player.Choice.Choices.Select(p => game.getGame().IdEntityDic[p]).ToList());
				return ChooseTask.Mulligan(player, mulligan);
			}


			IEnumerable<KeyValuePair<PlayerTask, POGame>> validOpts = game.Simulate(player.Options()).Where(x => x.Value != null);
			int optcount = validOpts.Count();
			int maxDepth = optcount >= 5 ? (optcount >= 25 ? 1 : 2) : 3;

			if (validOpts.Any()) return validOpts.Select(x => score(x, player.PlayerId, maxDepth)).OrderBy(x => x.Value).Last().Key;
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

		private static int Score(POGame state, int playerId)
		{
			Controller p = state.CurrentPlayer.PlayerId == playerId ? state.CurrentPlayer : state.CurrentOpponent;
			return new CustomScore { Controller = p }.Rate();
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
	}
}
