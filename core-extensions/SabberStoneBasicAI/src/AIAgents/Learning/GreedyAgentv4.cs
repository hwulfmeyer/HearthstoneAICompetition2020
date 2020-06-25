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
using System.Diagnostics;
using System;

//Developed by Oskar Kirmis and Florian Koch and submitted to the 2018 Hearthstone AI Competition's Premade Deck Playing Track
namespace SabberStoneBasicAI.AIAgents.Learning
{
	// Plain old Greedy Bot
	class GreedyAgentv4 : AbstractAgent
	{
		List<List<float>> GameStateEncodes;
		private Dictionary<string, List<double>> CardClassScoreWeights;

		public override void InitializeAgent(){
			GameStateEncodes = new List<List<float>>();
			CardClassScoreWeights = new Dictionary<string, List<double>>();

			List<double> weights;
			weights = new List<double>() { 0.598065746396966, 0.49095310686114435, 0.3014604069938352, 0.1948565464958161, -0.10392010620254383, 0.5201749099338618, 0.4881575646144944, 0.4689600423745222, 0.18450743523737959, 0.15026938017089655, -0.15236235061126638, -0.10527641026037836, -0.7336376836750389, -0.47554866432262777, -0.5010813514551001, -0.47639152082625824, -0.42433016775625815, -0.1523054989372939, -0.41977889444392014, -0.07630755348343218, -1.1508867818065318, -0.06613899385147948, -0.1891620007638619, -0.2442811716828002, 0.06921634230312976, 0.01747350860795673 };
			CardClassScoreWeights.Add("MAGE", weights);

			weights = new List<double>() {2.214560814525201, 0.6586410596734427, 0.07903208212628259, 0.26387957410824064, -0.07096259392939917, 0.45906562150874614, 0.48561827572670424, 1.1350098817197796, 0.4506378939300482,
-0.13632450460136417, -1.214224910015313e-06, -0.0760387559900064, -0.285643568899386, -2.0575346870121884, -0.6540059405548384, 0.06695801381268666, -0.4826313007138901, -0.09676606086387882, -0.40804070381990937,
-0.1513298497628543, -1.741171898096198, -0.11679087341676189, -0.29522790613350386, 0.0058595971514614625, 0.11712473276853029, 0.1813773686656724};
			CardClassScoreWeights.Add("PALADIN", weights);

			weights = new List<double>() { 2.6848021449412713, 0.6763005340400551, 0.03305613249711951, 0.2898885973337898, 0.037393326051679145, 0.42408641051929796, 0.42712049418229775, -0.7640694334484565, 0.27685070129999134, 0.09591510217657598, 0.043795414501844196, -0.15178319456944883, -0.692472624311828, -2.3718758012728287, -0.6161692395593276, 0.051468521056583935, -0.41972866372341033, -0.11794155216370236, -0.5017475834304307, -0.08792149424847748, -0.4967729105098038, -0.15723652875561062, -0.10722439962386879, 0.04103041154267651, 0.21987681066868056, 0.11990531577601958 };
			CardClassScoreWeights.Add("ROGUE", weights);

			weights = new List<double>() { 2.152800856415321, 0.6434702894819269, 0.30774662098372674, 0.3557844601633494, -0.0842973158691835, 0.46819294918546245, 0.4143871727020375, 1.8134406911509975, 0.22157164057010248, 0.051912936227122154, 0.006539218090375489, -0.1091642984806764, -0.44618428556349665, -2.040171217039921, -0.6389967804209116, -0.17229082958266168, -0.6268856605461437, -0.07354025090276316, -0.3920253620886482, -0.06696512058429432, -0.4530677219920705, -0.3349403630831643, -0.23414007840412313, -0.0648295518967722, 0.09222289824232013, -0.016659865398295667 };
			CardClassScoreWeights.Add("SHAMAN", weights);

			weights = new List<double>() { -1.4146962456625105, 0.49754713327551436, -0.7142446587966887, 0.9780677674920117, -0.3690643477912796, 0.3556993498651843, 0.723801048647224, 1.663618164269731, -0.9840058212827905, -0.02656429817739804, -0.2523920890743777, -0.14772160381588986, -0.6108106848328775, 1.598498943119686, -0.6358365846293869, -0.6139832434215626, -1.0442617135355634, 0.0004373492367708634, -0.3727791919385305, -0.12118645354892706, 3.4558981250135483, -0.6319398973495314, -0.12419799219887522, -0.28129969457999865, 0.3242279776442094, -0.4713800150455945 };
			CardClassScoreWeights.Add("WARLOCK", weights);

			weights = new List<double>() { 1.8727523344112422, 0.6549309215329471, -0.026562407277574876, 0.48672688367504097, -0.009024126328616882, 0.47219449157409854, 0.3647234822583707, -0.054485660039332154, 0.6295080612165574, -0.020829371647803456, 0.024213315482672484, -0.17615027006505526, -0.7875428749996669, -1.8257630674869059, -0.6467541003055373, 0.06505655861193148, -0.4965323468273937, -0.13372492046647697, -0.427165467637058, 0.057033640352959755, -1.121634493082168, -0.48281279316647224, -0.22776360097227555, -0.07251660852514885, 0.24034290413079448, 0.2218666964537394 };
			CardClassScoreWeights.Add("WARRIOR", weights);


		}
		public override void InitializeGame() {}
		public override void FinalizeGame(Game game, Controller myPlayer)
		{
			bool write = false;
			bool test = true;

			if (write)
			{
				int GameResult = myPlayer.PlayState == PlayState.WON ? 1 : myPlayer.PlayState == PlayState.TIED ? 0 : -1;
				int GameResultHp = myPlayer.PlayState == PlayState.WON ? myPlayer.Hero.Health : myPlayer.PlayState == PlayState.TIED ? 0 : -myPlayer.Opponent.Hero.Health;

				for (int i = 0; i < GameStateEncodes.Count; i++)
				{
					GameStateEncodes[i].Add(GameResult);
					GameStateEncodes[i].Add(GameResultHp);
					GameStateEncodes[i].Add(GameResultHp * (GameStateEncodes[i][0] + 1) / 11.0f);
					GameStateEncodes[i].Add(GameResultHp * (i + 1.0f) / GameStateEncodes.Count);
				}

				bool success = false;	
				string folder = "F:\\data_" + (test ? "test" : "train") + "\\";
				while (!success)
				{
					try
					{
						using (FileStream fileStream = new FileStream(folder + "greedyv3_" + myPlayer.HeroClass.ToString() + ".csv", FileMode.Append, FileAccess.Write, FileShare.None))
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
		}
		public override void FinalizeAgent() {}


		public override PlayerTask GetMove(POGame game)
		{
			float EPS = 0.0f;
			GameStateEncodes.Add(GameStateEncoding.GetEncoding(game, game.CurrentPlayer.PlayerId));
			Controller player = game.CurrentPlayer;

			// Implement a simple Mulligan Rule
			if (player.MulliganState == Mulligan.INPUT)
			{
				List<int> mulligan = new AggroScore().MulliganRule().Invoke(player.Choice.Choices.Select(p => game.getGame().IdEntityDic[p]).ToList());
				return ChooseTask.Mulligan(player, mulligan);
			}

			// Get all simulation results for simulations that didn't fail
			IEnumerable<KeyValuePair<PlayerTask, POGame>> validOpts = game.Simulate(player.Options()).Where(x => x.Value != null);

			// If all simulations failed, play end turn option (always exists), else best according to score function
			if (validOpts.Any())
			{
				Random rnd = new Random();
				if (rnd.NextDouble() >= EPS) return validOpts.OrderBy(x => Score(x.Value, player.PlayerId)).Last().Key;
				else return validOpts.ElementAt(rnd.Next(validOpts.Count())).Key;
			}
			else
			{
				return player.Options().First(x => x.PlayerTaskType == PlayerTaskType.END_TURN);
			}
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

		private float Score(POGame state, int playerId)
		{
			Controller p = state.CurrentPlayer.PlayerId == playerId ? state.CurrentPlayer : state.CurrentOpponent;
			return new GameStateEncoding { Controller = p }.Rate(CardClassScoreWeights);
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
				//Encoding.Add(OpMinionTotHealthImmune);
				return Encoding;
			}

			public float Rate(Dictionary<string, List<double>> scoreWeights)
			{
				CreateEncoding();
				float score = 0;

				List<double> weights = scoreWeights.GetValueOrDefault(Controller.HeroClass.ToString());

				for (int i = 0; i < Encoding.Count; i++)
				{
					score += Encoding[i] * (float)weights[i];
				}
				return score;
			}
		}
	}
}
