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
	class GreedyAgentv3 : AbstractAgent
	{
		List<List<float>> GameStateEncodes;
		private Dictionary<string, List<double>> CardClassScoreWeights;

		public override void InitializeAgent(){
			GameStateEncodes = new List<List<float>>();
			CardClassScoreWeights = new Dictionary<string, List<double>>();

			List<double> weights;
			weights = new List<double>() {0.3396823227350791, 0.4755695757161211, 0.6315254752527363, 0.4734719464388754, -0.13342819577228687, 0.4880292751372225, 0.4546600122453255, -0.09655686815963457, 0.22444652242475907,
0.19958732533079662, -0.17036348526647713, -0.15105786100949445, -0.8577670378368388, -0.28672597345950523, -0.5220220209814568, 0.007159377000021262, -0.38789345571895467, -0.1283321978508957, -0.4730444625436555,
-0.10962017160626067, 0.648797000536877, -0.6197835909878951, -0.09484805666235381, -0.15806698813254624, 0.03653132581369362, -0.3760426624619679};
			CardClassScoreWeights.Add("MAGE", weights);

			weights = new List<double>() { 2.6657967031741223, 0.6172282932925817, 0.5267734627229559, 0.3845890126246053, -0.05651094886014054, 0.4235874880590937, 0.40590328154826616, 0.13045532836374601, 0.3227626928830109, 0.0013320505860923888, -0.026905224180923823, -0.12896842177994336, -0.17339781765457415, -2.537035435940092, -0.6444615887697005, -0.2150859695275532, -0.30035560311314363, -0.08332442555726452, -0.5248060718900047, -0.16630457818423394, 0.5487155310531595, -0.21270732650522323, -0.25718575866040094, 0.005845334069150769, 0.02459113277955093, 0.14781638952600942 };
			CardClassScoreWeights.Add("PALADIN", weights);

			weights = new List<double>() {2.108254812618058, 0.6256819258143028, -0.21972786839670158, 0.11695877393257469, 0.13971959985135304, 0.4647307286829961, 0.3451117139527672, -0.4515953027724709, 0.27874213066988307,
0.0612268289119284, 0.2444840560427164, -0.009243188007072415, -0.6285073497816541, -2.0742859221806595, -0.6376497639879554, 0.03982635363840739, -0.450726429902333, -0.19786737194618953, -0.5446075216627354, -0.04207827000472267, 2.0914957006005803, -0.5866838669347538, -0.1609333012132106, -0.20116804674688515, 0.3902093088311108, 0.08711608204928868};
			CardClassScoreWeights.Add("ROGUE", weights);

			weights = new List<double>() { 2.1009877294840034, 0.5871437352243997, 0.12991498581449737, 0.45608217731438716, -0.030739199730632275, 0.45068848120459415, 0.3276483798250836, -1.4679626602161622, 0.4618489919070022, 0.11366472294529824, 0.12179101469814593, -0.23180098517182537, -0.39853818287066883, -2.1079112766917323, -0.6217759542112327, 0.18233187125024847, -0.7997254282096468, 0.01284499905586331, -0.4731496751070926, -0.08733492413557076, -2.808511602548035, -0.24739222263126154, -0.24111417711725536, -0.3275898154533169, 0.19010422386652226, 0.02403775889384799 };
			CardClassScoreWeights.Add("SHAMAN", weights);

			weights = new List<double>() {-1.4287179253776663, 0.4326959098996448, 0.1047511018602149, 0.17594329650466567, -0.06921212031170057, 0.49609267849694666, 0.793744933165348, -1.0842463494240322, -0.919799035058143,
-0.1516633391465757, -0.4104235208663316, 0.34515065165163444, -0.12414920567443831, 1.3402820139310152, -0.7017375269392632, -1.2838838813284599, -0.9572873647942951, -0.01581083452335453, -0.38196481235139096, 0.21999190978150235, 1.788962342289077, -0.4985101151314515, -0.26627739179661614, -0.5524113721880164, 0.22232892366034218, -0.6321553673250434};
			CardClassScoreWeights.Add("WARLOCK", weights);

			weights = new List<double>() { 1.8050278085507863, 0.5993997150677683, -0.05743831873331757, 0.4842651998954194, -0.036644133696711156, 0.5639806682411649, 0.36182956007060973, 2.084307096138654, -0.0063968642256147104, -0.022303631616641447, 0.17157618893986473, -0.19397182835333096, -0.9149225669162552, -1.9234300753289446, -0.6347653103164637, -0.14779504336526836, -0.40072919206987917, -0.11960767888127061, -0.5897291624755794, -0.05857769488665897, 0.9529889748884189, -0.5882732419814911, -0.2647173934518952, 0.03945145315509562, 0.3909444018952406, -0.041700524148903635 };
			CardClassScoreWeights.Add("WARRIOR", weights);
		}

		public override void InitializeGame() {}
		public override void FinalizeGame(Game game, Controller myPlayer)
		{
			bool write = false;
			bool test = false;

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
