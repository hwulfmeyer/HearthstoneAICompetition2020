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
	class DynamicLookaheadv3 : DynamicLookahead
	{
		public override void InitializeAgent()
		{
			base.InitializeAgent();
			write = false;
			test = false;
			name = "DLAv3";
			GameStateEncodes = new List<List<float>>();
			CardClassScoreWeights = new Dictionary<string, List<double>>();

			List<double> weights;
			weights = new List<double>() { 0.5359265974980221, 0.5163114233409954, 0.5485197719199845, 0.36123494163762654, -0.21254228460373295, 0.5522279027745877, 0.4465726654204785, 0.03168684515790007, 0.0903130984503714, 0.05893754159218277, -0.05247681717627896, -0.2762670765145196, 2.220446049250313e-16, -0.4694207080901187, -0.5568169453139014, -0.329092032650898, -0.622984809108873, 0.011381852158929881, -0.3897472450688495, -0.1736767723473775, 1.2356281332081898, -0.2057357115189894, -0.20461946974514122, -0.23562925347201877, 0.21011411382216533, -0.09555381413527705 };
			CardClassScoreWeights.Add("MAGE", weights);

			weights = new List<double>() { 1.321582909813508, 0.6600234757994029, 0.07225337308226192, 0.3561532123238372, -0.07002814761078717, 0.4344872454067603, 0.30904167100833185, -0.7668000749550619, 0.1958134754406109, -0.07125321071290065, 0.06617582988919406, -0.3438359959554716, -0.306619507390962, -1.1905019494484472, -0.626364769117593, -0.09048360350577073, -0.39328710690895535, -0.1421581309605943, -0.30728058792352486, -0.11973773788550268, 0.5971881063215135, -0.06337710274844065, -0.27731942594032427, -0.03777892829291099, 0.1628543031470663, 0.33132057484638794 };
			CardClassScoreWeights.Add("PALADIN", weights);

			weights = new List<double>() { 1.3876203341920617, 0.6855418703095606, 0.18742743185611296, 0.4906887561268414, -0.12333869465983191, 0.5159124416313647, 0.3926983492936258, 0.21796856129618436, 0.08085182303808995, 0.04484716131612132, 0.06305752445728835, -0.3279216689060969, -0.5415917183306147, -1.2893943429311692, -0.6575468268088345, -0.09945372614583294, -0.3741190285624961, -0.12587310944074598, -0.4123026856717087, -0.09327712393800808, -0.1126003422719337, -0.1078676393090936, -0.1783328015735798, -0.04399168593150808, 0.028893190158871117, 0.08138469777772873 };
			CardClassScoreWeights.Add("ROGUE", weights);

			weights = new List<double>() { 1.3163664743138574, 0.6651327294035738, 0.2463972255663899, 0.42981388952680394, -0.0933569491800495, 0.4351591004240116, 0.31840999368715955, -0.2573649316235434, 0.045889229358232066, 0.054270312055949614, 0.0650814253359627, -0.30796023550670243, -0.5837882373003719, -1.1936220559824027, -0.6431085559333698, -0.2534837210670258, -0.42955286490077516, -0.08744398565453504, -0.3349378848297718, -0.15917769808206944, 1.3454048744237146, -0.12043684008394678, -0.2090173070323826, -0.07235243255529863, 0.036022610903660095, -0.06855198536149777 };
			CardClassScoreWeights.Add("SHAMAN", weights);

			weights = new List<double>() { -0.36929655126930644, 0.6195265114650441, 0.37644732714902773, -0.3473474085909965, -0.27905516617519643, 0.5617859900728492, 0.6540797790317703, -0.6111236230733184, 0.9659898475574875, -0.025531931306625996, -0.10297637553664565, -0.29973176057855555, -1.7446150056431826, 0.8041461939090816, -0.7913046356317388, -1.2432428027740718, -0.8970806499900621, -0.05963308546005544, -0.320900059509135, 0.12312507414272451, -1.0651183230303507, 0.11421795612534447, -0.34690467646615253, -0.06142127398665819, 0.3580480567140461, -0.34033885014579923 };
			CardClassScoreWeights.Add("WARLOCK", weights);

			weights = new List<double>() { 1.0447011856371882, 0.6587007992995593, 0.16798267001151668, 0.3549776866337994, -0.06532101980092385, 0.4853443314615905, 0.3350045531749963, -0.23918898874607145, 0.2623023388691243, -0.006507861393080114, -0.034344842254078224, -0.2885405172610697, -0.5546237875880109, -0.8391187569387485, -0.6350423070848802, -0.08230853664135244, -0.35751329154159234, -0.13485235546171478, -0.44878052254615786, -0.20490992855935167, -0.15862403698242533, 0.21193396019532146, -0.09697862354296279, -0.0328743806291497, 0.20523344139941854, 0.24676682334538855 };
			CardClassScoreWeights.Add("WARRIOR", weights);
		}
	}

	class DynamicLookaheadv2 : DynamicLookahead
	{
		public override void InitializeAgent()
		{
			base.InitializeAgent();
			write = false;
			test = false;
			name = "DLAv2";
			GameStateEncodes = new List<List<float>>();
			CardClassScoreWeights = new Dictionary<string, List<double>>();

			List<double> weights;
			weights = new List<double>() { 1.938747258483477, 0.818240001476494, 1.381586607839055, 0.2526790972982106, -0.13122544891217455, 0.5171498586440116, 0.6718084241473198, 1.527569550651253, 0.7371746481457607, 0.19267568915876684, -0.058069376174614655, -0.23795491752372047, -0.4966140671510174, -2.0461036981404126, -0.4834859319895265, -0.11891460660513306, -0.9399243561244603, -0.154408573109368, -0.41204929691020564, -0.02890839443098675, -2.524398511157367, 0.3271311310916452, -0.44426177931995636, 0.13400038360669292, 0.1127043028273832, -0.17034012642976573 };
			CardClassScoreWeights.Add("MAGE", weights);

			weights = new List<double>() { 4.379025236615299, 0.9594149923900825, -0.34575840415683756, -0.10564913313685792, -0.09716465647655594, 0.46881024950914435, 0.6927722089436678, -1.9253868236114973, 1.0687800258506883, -0.16339375298349315, -0.2929414167615923, 0.018081633444984174, -0.3165979942906423, -4.463832600942665, -0.6139280555780036, 0.17534863421886318, -1.4324168856893733, 0.03132877550307023, -0.5143910086242638, -0.15223773973101173, -7.645234045070544, 0.5051136774125496, -0.31135155066596243, 0.053594204954991696, 0.10067769525483766, 0.552300496047932 };
			CardClassScoreWeights.Add("PALADIN", weights);

			weights = new List<double>() { 4.471722417034926, 1.0082990509892957, 0.5925602960770466, 0.7027545989764048, -0.01245883357340686, 0.3242852141472355, 0.6954122075277187, -0.8900966243228271, 0.47283741831279635, -0.0439418558891088, 0.24692150020588474, -0.11485403574240424, -0.6083965720446799, -4.261644630014515, -0.529740187802663, 0.4353390042339865, -1.6929310970208262, 0.012162241126420281, -0.44829714989656727, -0.021610908320330947, 0.3469552695476015, 0.6155910598883376, -0.3296255555934871, 0.31053729962836063, 0.48543889369434917, -0.3037862463926443 };
			CardClassScoreWeights.Add("ROGUE", weights);

			weights = new List<double>() { 4.374720962335739, 1.0215904747643367, -0.3150188886481086, -0.06238242363857954, -0.06759220919860373, 0.5656590233403843, 0.3885512221800834, -0.17147831203631791, -0.3781746944755436, 0.0034751150323643542, 0.28508309496300777, -0.07325098411514964, -0.19882269153444776, -4.194720857391452, -0.519375057295756, -0.23973255672480906, -1.4125582272155413, 0.010208177164815535, -0.49644178971877934, -0.21577822311450603, -0.27919371639786444, -0.21945439292187974, -0.311899520077985, 0.2442347322744321, 0.3214521462218269, 0.43565274379344204 };
			CardClassScoreWeights.Add("SHAMAN", weights);

			weights = new List<double>() { -1.072192661157513, 0.48298261315140345, -1.3594242462917916, 2.2596705419393848, -0.30360423342872417, 0.21406633228280458, 1.0211975782368317, 3.54749564050917, -0.07007404222163256, 0.024789604882904676, -0.5313245057925058, -0.17493303956967668, -3.7166450600831467, 2.37119206488871, -0.9264883742679608, -1.571498381872472, -1.5154581646252392, 0.024024360028297675, -0.40652186000033413, -0.02200559938182131, 1.176836406102666e-14, -1.0737549848094954, -0.4642755352193355, -0.13364746407394681, 0.2634316514630303, -1.0875941482588776 };
			CardClassScoreWeights.Add("WARLOCK", weights);

			weights = new List<double>() { 4.005229679577584, 0.9601773887212031, -0.01691428580547783, 0.7357998724008524, 0.12226592531098472, 0.22858566660395913, 0.5250712181907465, 0.5333028408195718, 0.7579364095871625, 0.1415790067415535, 0.1790327881121236, 0.06619431453638194, -1.2715308932608584, -3.8380718069182675, -0.5363176260976187, 0.028662543891683337, -1.8904231652309114, 0.08274439103048192, -0.6240644839738418, -0.04579668263310886, -14.410304908146836, 0.20670668702698958, -0.13184222329572082, -0.1325371317911512, 0.2853191674767238, 0.8432421147379726 };
			CardClassScoreWeights.Add("WARRIOR", weights);
		}
	}

	class DynamicLookaheadv1 : DynamicLookahead
	{
		public override void InitializeAgent()
		{
			base.InitializeAgent();
			write = false;
			test = false;
			name = "DLAv1";
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
	}


	abstract class DynamicLookahead : AbstractAgent
	{
		public static Timer timer;
		public bool timeIsOver = false;
		public List<List<float>> GameStateEncodes;
		public Dictionary<string, List<double>> CardClassScoreWeights;
		public bool write = false;
		public bool test = false;
		public string name = "";


		public void OnTimedEvent(object source, ElapsedEventArgs e)
		{
			timeIsOver = true;
		}


		public override void FinalizeAgent()
		{
		}

		public override void FinalizeGame(Game game, Controller myPlayer)
		{
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
						using (FileStream fileStream = new FileStream(folder + name + "_" + myPlayer.HeroClass.ToString() + ".csv", FileMode.Append, FileAccess.Write, FileShare.None))
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

		public override PlayerTask GetMove(POGame game)
		{
			timeIsOver = false;
			timer.Start();
			GameStateEncodes.Add(GameStateEncoding.GetEncoding(game, game.CurrentPlayer.PlayerId));
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

			KeyValuePair<PlayerTask, float> score(KeyValuePair<PlayerTask, POGame> state, int player_id, int max_depth = 3)
			{
				float max_score = Int32.MinValue;
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
				return new KeyValuePair<PlayerTask, float>(state.Key, max_score);
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
