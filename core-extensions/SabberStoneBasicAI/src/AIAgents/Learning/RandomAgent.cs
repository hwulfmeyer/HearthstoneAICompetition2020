using System;
using System.Collections.Generic;
using SabberStoneCore.Tasks.PlayerTasks;
using SabberStoneBasicAI.PartialObservation;
using SabberStoneCore.Enums;
using System.Linq;
using SabberStoneCore.Model.Entities;
using SabberStoneCore.Model;
using CsvHelper;
using System.IO;
using System.Globalization;

namespace SabberStoneBasicAI.AIAgents.Learning
{

	class RandomAgent : AbstractAgent
	{
		private Random Rnd = new Random();
		List<List<float>> GameStateEncodes = new List<List<float>>();

		public override void InitializeAgent()
		{
			Rnd = new Random();
			GameStateEncodes = new List<List<float>>();
		}

		public override void FinalizeAgent()
		{
			//Nothing to do here
		}

		public override void FinalizeGame(Game game, Controller myPlayer)
		{
			bool write = false;
			bool test = false;
			int GameResult = myPlayer.PlayState == PlayState.WON ? 1 : myPlayer.PlayState == PlayState.TIED ? 0 : -1;
			int GameResultHp = myPlayer.PlayState == PlayState.WON ? myPlayer.Hero.Health : myPlayer.PlayState == PlayState.TIED ? 0 : -myPlayer.Opponent.Hero.Health;
			string folder = "F:\\data\\" + (test ? "test" : "train") + "\\";
			if (write)
			{
				for (int i = 0; i < GameStateEncodes.Count; i++)
				{
					double power = 1.0;
					if (GameStateEncodes[i][0] <= 3.0f) power = 1.6; //early
					if (GameStateEncodes[i][0] <= 7.0f && GameStateEncodes[i][0] >= 4.0f) power = 2; //mid
					if (GameStateEncodes[i][0] >= 8.0f) power = 2.4; //late

					GameStateEncodes[i].Add((float)(1000.0 * GameResultHp * Math.Pow((i + 1.0f) / GameStateEncodes.Count, power)));
					/*GameStateEncodes[i].Add((float)(GameResultHp * Math.Pow((i + 1.0f) / GameStateEncodes.Count, 1)));	 //-18
					GameStateEncodes[i].Add((float)(GameResultHp * Math.Pow((i + 1.0f) / GameStateEncodes.Count, 1.2))); //-17
					GameStateEncodes[i].Add((float)(GameResultHp * Math.Pow((i + 1.0f) / GameStateEncodes.Count, 1.4))); //-16
					GameStateEncodes[i].Add((float)(GameResultHp * Math.Pow((i + 1.0f) / GameStateEncodes.Count, 1.6))); //-15 //early
					GameStateEncodes[i].Add((float)(GameResultHp * Math.Pow((i + 1.0f) / GameStateEncodes.Count, 1.8))); //-14 
					GameStateEncodes[i].Add((float)(GameResultHp * Math.Pow((i + 1.0f) / GameStateEncodes.Count, 2)));   //-13 //mid
					GameStateEncodes[i].Add((float)(GameResultHp * Math.Pow((i + 1.0f) / GameStateEncodes.Count, 2.2))); //-12
					GameStateEncodes[i].Add((float)(GameResultHp * Math.Pow((i + 1.0f) / GameStateEncodes.Count, 2.4))); //-11 //late
					GameStateEncodes[i].Add((float)(GameResultHp * Math.Pow((i + 1.0f) / GameStateEncodes.Count, 2.6))); //-10
					GameStateEncodes[i].Add((float)(GameResultHp * Math.Pow((i + 1.0f) / GameStateEncodes.Count, 2.8))); //-9
					GameStateEncodes[i].Add((float)(GameResultHp * Math.Pow((i + 1.0f) / GameStateEncodes.Count, 3)));	 //-8
					GameStateEncodes[i].Add((float)(GameResultHp * Math.Pow((i + 1.0f) / GameStateEncodes.Count, 3.2))); //-7
					GameStateEncodes[i].Add((float)(GameResultHp * Math.Pow((i + 1.0f) / GameStateEncodes.Count, 3.4))); //-6
					GameStateEncodes[i].Add((float)(GameResultHp * Math.Pow((i + 1.0f) / GameStateEncodes.Count, 3.6))); //-5
					GameStateEncodes[i].Add((float)(GameResultHp * Math.Pow((i + 1.0f) / GameStateEncodes.Count, 3.8))); //-4
					GameStateEncodes[i].Add((float)(GameResultHp * Math.Pow((i + 1.0f) / GameStateEncodes.Count, 4)));   //-3
					GameStateEncodes[i].Add((float)(GameResultHp * Math.Pow((i + 1.0f) / GameStateEncodes.Count, 4.2))); //-2
					GameStateEncodes[i].Add((float)(GameResultHp * Math.Pow((i + 1.0f) / GameStateEncodes.Count, 4.4))); //-1*/
				}

				List<List<float>> GameStateEncodesEarly = GameStateEncodes.Where(x => x[0] <= 3.0f).ToList();
				List<List<float>> GameStateEncodesMid = GameStateEncodes.Where(x => x[0] <= 7.0f && x[0] >= 4.0f).ToList();
				List<List<float>> GameStateEncodesLate = GameStateEncodes.Where(x => x[0] >= 8.0f).ToList();

				foreach (List<float> record in GameStateEncodesEarly)
				{
					record.RemoveAt(0);
				}

				foreach (List<float> record in GameStateEncodesMid)
				{
					record.RemoveAt(0);
				}

				foreach (List<float> record in GameStateEncodesLate)
				{
					record.RemoveAt(0);
				}

				bool success = false;
				while (!success)
				{
					try
					{
						using (FileStream fileStream = new FileStream(folder + "random__early_" + myPlayer.HeroClass.ToString() + ".csv", FileMode.Append, FileAccess.Write, FileShare.None))
						using (var writer = new StreamWriter(fileStream))
						{
							using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
							{
								foreach (List<float> record in GameStateEncodesEarly)
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
				success = false;
				while (!success)
				{
					try
					{
						using (FileStream fileStream = new FileStream(folder + "random__mid_" + myPlayer.HeroClass.ToString() + ".csv", FileMode.Append, FileAccess.Write, FileShare.None))
						using (var writer = new StreamWriter(fileStream))
						{
							using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
							{
								foreach (List<float> record in GameStateEncodesMid)
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
				success = false;
				while (!success)
				{
					try
					{
						using (FileStream fileStream = new FileStream(folder + "random__late_" + myPlayer.HeroClass.ToString() + ".csv", FileMode.Append, FileAccess.Write, FileShare.None))
						using (var writer = new StreamWriter(fileStream))
						{
							using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
							{
								foreach (List<float> record in GameStateEncodesLate)
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

			GameStateEncodes = new List<List<float>>();
		}

		public override PlayerTask GetMove(POGame poGame)
		{
			GameStateEncodes.Add(GameStateEncoding.GetEncoding(poGame, poGame.CurrentPlayer.PlayerId));
			var player = poGame.CurrentPlayer;

			// During Mulligan: select Random cards
			if (player.MulliganState == Mulligan.INPUT)
			{
				List<int> mulligan = RandomMulliganRule().Invoke(player.Choice.Choices.Select(p => poGame.getGame().IdEntityDic[p]).ToList());
				return ChooseTask.Mulligan(player, mulligan);
			}

			// During Gameplay: select a random action
			List<PlayerTask> options = poGame.CurrentPlayer.Options();
			PlayerTask task = options[Rnd.Next(options.Count)];

			return task;
		}

		public override void InitializeGame()
		{
			//Nothing to do here
		}

		public Func<List<IPlayable>, List<int>> RandomMulliganRule()
		{
			return p => p.Where(t => Rnd.Next(1, 3) > 1).Select(t => t.Id).ToList();
		}
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
			
			Encoding.Add(MinionTotHealthPoisonous);
			Encoding.Add(MinionWindfuryTotAtk);
			Encoding.Add(MinionTotHealthDeathrattle);
			Encoding.Add(MinionTotHealthDivineShield);
			Encoding.Add(MinionFrozenTotAtk);
			Encoding.Add(MinionTotHealthStealth);
			//Encoding.Add(MinionTotHealthImmune);

			//Encoding.Add(OpHeroClassId);
			//Encoding.Add(OpHeroBaseMana);
			Encoding.Add(OpHeroHp);
			Encoding.Add(OpHeroArmor);
			Encoding.Add(OpBoardZone.Count);
			Encoding.Add(OpMinionTotHealth);
			Encoding.Add(OpMinionTotAtk);
			Encoding.Add(OpMinionTotHealthTaunt);
			
			Encoding.Add(OpMinionTotHealthPoisonous);
			Encoding.Add(OpMinionWindfuryTotAtk);
			Encoding.Add(OpMinionTotHealthDeathrattle);
			Encoding.Add(OpMinionTotHealthDivineShield);
			Encoding.Add(OpMinionFrozenTotAtk);
			Encoding.Add(OpMinionTotHealthStealth);
			//Encoding.Add(OpMinionTotHealthImmune);

			return Encoding;
		}

		public override Func<List<IPlayable>, List<int>> MulliganRule()
		{
			return p => p.Where(t => t.Cost > 3).Select(t => t.Id).ToList();
		}
	}
}
