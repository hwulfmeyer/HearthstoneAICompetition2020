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

namespace SabberStoneBasicAI.AIAgents
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
			int GameResult = myPlayer.PlayState == PlayState.WON ? 1 : myPlayer.PlayState == PlayState.TIED ? 0 : -1;
			int GameResultHp = myPlayer.PlayState == PlayState.WON ? myPlayer.Hero.Health : myPlayer.PlayState == PlayState.TIED ? 0 : -myPlayer.Opponent.Hero.Health;

			for (int i = 0; i < GameStateEncodes.Count; i++)
			{
				GameStateEncodes[i].Add(GameResult);
				GameStateEncodes[i].Add(GameResultHp);
				GameStateEncodes[i].Add(GameResultHp * (GameStateEncodes[i][0] + 1) / 11.0f);
				GameStateEncodes[i].Add(GameResultHp * (i+1.0f) / GameStateEncodes.Count);
			}

			bool success = false;
			while(!success) {
				try
				{
					using (FileStream fileStream = new FileStream("F:\\file_training_random" + myPlayer.HeroClass.ToString() + ".csv", FileMode.Append, FileAccess.Write, FileShare.None))
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
			return options[Rnd.Next(options.Count)];
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
			Encoding.Add(MinionTotHealthImmune);

			//Encoding.Add(OpHeroClassId);
			Encoding.Add(OpHeroBaseMana);
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
			Encoding.Add(OpMinionTotHealthImmune);


			return Encoding;
		}

		public override Func<List<IPlayable>, List<int>> MulliganRule()
		{
			return p => p.Where(t => t.Cost > 3).Select(t => t.Id).ToList();
		}
	}
}
