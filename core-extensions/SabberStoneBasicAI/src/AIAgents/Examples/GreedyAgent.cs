using System.Linq;
using SabberStoneCore.Enums;
using SabberStoneBasicAI.Score;
using SabberStoneCore.Tasks.PlayerTasks;
using SabberStoneBasicAI.PartialObservation;
using System.Collections.Generic;
using SabberStoneCore.Model.Entities;
using SabberStoneCore.Model;
using System.IO;
using CsvHelper;
using System.Globalization;

//Developed by Oskar Kirmis and Florian Koch and submitted to the 2018 Hearthstone AI Competition's Premade Deck Playing Track
namespace SabberStoneBasicAI.AIAgents
{
	// Plain old Greedy Bot
	class GreedyAgent : AbstractAgent
	{
		List<List<float>> GameStateEncodes = new List<List<float>>();

		public override void InitializeAgent() { }
		public override void InitializeGame() { }
		public override void FinalizeGame(Game game, Controller myPlayer)
		{
			int GameResult = myPlayer.PlayState == PlayState.WON ? 1 : myPlayer.PlayState == PlayState.TIED ? 0 : -1;
			int GameResultHp = myPlayer.PlayState == PlayState.WON ? myPlayer.Hero.Health : myPlayer.PlayState == PlayState.TIED ? 0 : -myPlayer.Opponent.Hero.Health;
			foreach (List<float> enc in GameStateEncodes)
			{
				enc.Add(GameResult);
				enc.Add(GameResultHp);
				enc.Add(GameResultHp * (enc[0] + 1) / 11.0f);
				//Console.WriteLine(String.Join(",", enc.Select(x => x.ToString()).ToArray()));
			}

			bool success = false;
			while (!success)
			{
				try
				{
					using (FileStream fileStream = new FileStream("F:\\file_training_greedy" + myPlayer.HeroClass.ToString() + ".csv", FileMode.Append, FileAccess.Write, FileShare.None))
					using (var writer = new StreamWriter(fileStream))
					{
						using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
						{
							foreach (List<float> record in GameStateEncodes)
							{
								foreach (float field in record)
								{
									csv.WriteField(field);
								}

								csv.NextRecord();
							}
							success = true;
						}
					}
				}
				catch
				{

				}
			}
		}
		public override void FinalizeAgent() { }


		public override PlayerTask GetMove(POGame game)
		{
			GameStateEncodes.Add(GameStateEncoding.GetEncoding(game, game.CurrentPlayer.PlayerId));
			var player = game.CurrentPlayer;

			// Implement a simple Mulligan Rule
			if (player.MulliganState == Mulligan.INPUT)
			{
				List<int> mulligan = new AggroScore().MulliganRule().Invoke(player.Choice.Choices.Select(p => game.getGame().IdEntityDic[p]).ToList());
				return ChooseTask.Mulligan(player, mulligan);
			}

			// Get all simulation results for simulations that didn't fail
			IEnumerable<KeyValuePair<PlayerTask, POGame>> validOpts = game.Simulate(player.Options()).Where(x => x.Value != null);

			// If all simulations failed, play end turn option (always exists), else best according to score function
			return validOpts.Any() ?
				validOpts.OrderBy(x => Score(x.Value, player.PlayerId)).Last().Key :
				player.Options().First(x => x.PlayerTaskType == PlayerTaskType.END_TURN);
		}

		// Calculate different scores based on our hero's class
		/*private static int Score(POGame state, int playerId)
		{
			var p = state.CurrentPlayer.PlayerId == playerId ? state.CurrentPlayer : state.CurrentOpponent;
			switch (state.CurrentPlayer.HeroClass)
			{
				case CardClass.WARRIOR: return new AggroScore { Controller = p }.Rate();
				case CardClass.MAGE: return new ControlScore { Controller = p }.Rate();
				default: return new RampScore { Controller = p }.Rate();
			}
		}*/

		private static float Score(POGame state, int playerId)
		{
			Controller p = state.CurrentPlayer.PlayerId == playerId ? state.CurrentPlayer : state.CurrentOpponent;
			return new GameStateEncoding { Controller = p }.Rate();
		}

		class GameStateEncoding : Score.Score
		{
			public int HeroClassId => (int)Controller.HeroClass;
			public int HeroArmor => Controller.Hero.Armor;
			public int HeroBaseMana => Controller.BaseMana;
			new public int MinionTotHealth => BoardZone.Sum(p => p.Health + p.Armor);
			new public int MinionTotHealthTaunt => BoardZone.Where(p => p.HasTaunt).Sum(p => p.Health + p.Armor);
			public int MinionTotHealthPoisonous => BoardZone.Where(p => p.Poisonous).Sum(p => p.Health + p.Armor);
			public int MinionWindfuryTotAtk => BoardZone.Where(p => p.HasWindfury).Sum(p => p.AttackDamage);
			public int MinionTotHealthDeathrattle => BoardZone.Where(p => p.HasDeathrattle).Sum(p => p.Health + p.Armor);
			public int MinionTotHealthDivineShield => BoardZone.Where(p => p.HasDivineShield).Sum(p => p.Health + p.Armor);
			public int MinionFrozenTotAtk => BoardZone.Where(p => p.IsFrozen).Sum(p => p.AttackDamage);
			public int MinionTotHealthStealth => BoardZone.Where(p => p.HasStealth).Sum(p => p.Health + p.Armor);
			public int MinionTotHealthImmune => BoardZone.Where(p => p.IsImmune).Sum(p => p.Health + p.Armor);

			public int OpHeroClassId => (int)Controller.Opponent.HeroClass;
			public int OpHeroArmor => Controller.Opponent.Hero.Armor;
			public int OpHeroBaseMana => Controller.Opponent.BaseMana;
			new public int OpMinionTotHealth => OpBoardZone.Sum(p => p.Health + p.Armor);
			new public int OpMinionTotHealthTaunt => OpBoardZone.Where(p => p.HasTaunt).Sum(p => p.Health + p.Armor);
			public int OpMinionTotHealthPoisonous => OpBoardZone.Where(p => p.Poisonous).Sum(p => p.Health + p.Armor);
			public int OpMinionWindfuryTotAtk => OpBoardZone.Where(p => p.HasWindfury).Sum(p => p.AttackDamage);
			public int OpMinionTotHealthDeathrattle => OpBoardZone.Where(p => p.HasDeathrattle).Sum(p => p.Health + p.Armor);
			public int OpMinionTotHealthDivineShield => OpBoardZone.Where(p => p.HasDivineShield).Sum(p => p.Health + p.Armor);
			public int OpMinionFrozenTotAtk => OpBoardZone.Where(p => p.IsFrozen).Sum(p => p.AttackDamage);
			public int OpMinionTotHealthStealth => OpBoardZone.Where(p => p.HasStealth).Sum(p => p.Health + p.Armor);
			public int OpMinionTotHealthImmune => OpBoardZone.Where(p => p.IsImmune).Sum(p => p.Health + p.Armor);

			double[] weights = new double[] {
				-2.20995732e-01,
				6.01566994e-01,
				1.59892065e-01,
				-1.98476635e-01,
				2.66882461e-01,
				4.76948350e-01,
				5.99475889e-02,

				5.94089401e-01,
				-4.86523459e-01,
				-2.83088299e-01,
				2.38955527e-01,
				-3.56540531e-01,
				-5.07434111e-01,
				-9.68924530e-02 };

			List<float> Encoding = new List<float>();

			public static List<float> GetEncoding(POGame state, int playerId)
			{
				Controller p = state.CurrentPlayer.PlayerId == playerId ? state.CurrentPlayer : state.CurrentOpponent;
				return new GameStateEncoding { Controller = p }.CreateEncoding();
			}

			public List<float> CreateEncoding()
			{
				//Encoding.Add(HeroClassId);
				Encoding.Add(HeroBaseMana);
				Encoding.Add(HeroHp);
				Encoding.Add(HeroArmor);
				Encoding.Add(BoardZone.Count);
				Encoding.Add(MinionTotHealth);
				Encoding.Add(MinionTotAtk);
				Encoding.Add(MinionTotHealthTaunt);
				/*
				Encoding.Add(MinionTotHealthPoisonous);
				Encoding.Add(MinionWindfuryTotAtk);
				Encoding.Add(MinionTotHealthDeathrattle);
				Encoding.Add(MinionTotHealthDivineShield);
				Encoding.Add(MinionFrozenTotAtk);
				Encoding.Add(MinionTotHealthStealth);
				Encoding.Add(MinionTotHealthImmune);*/

				//Encoding.Add(OpHeroClassId);
				Encoding.Add(OpHeroBaseMana);
				Encoding.Add(OpHeroHp);
				Encoding.Add(OpHeroArmor);
				Encoding.Add(OpBoardZone.Count);
				Encoding.Add(OpMinionTotHealth);
				Encoding.Add(OpMinionTotAtk);
				Encoding.Add(OpMinionTotHealthTaunt);
				/*
				Encoding.Add(OpMinionTotHealthPoisonous);
				Encoding.Add(OpMinionWindfuryTotAtk);
				Encoding.Add(OpMinionTotHealthDeathrattle);
				Encoding.Add(OpMinionTotHealthDivineShield);
				Encoding.Add(OpMinionFrozenTotAtk);
				Encoding.Add(OpMinionTotHealthStealth);
				Encoding.Add(OpMinionTotHealthImmune);*/

				return Encoding;
			}

			new public float Rate()
			{
				CreateEncoding();
				float score = 0;
				for (int i = 0; i < Encoding.Count; i++)
				{
					score += Encoding[i] * (float)weights[i];
				}
				return score;
			}
		}
	}
}
