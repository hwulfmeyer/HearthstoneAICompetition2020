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

	class DynamicLookaheadv1 : DynamicLookahead
	{
		public override void InitializeAgent()
		{
			base.InitializeAgent();
			name = "DLAv1";
			CardClassScoreWeights = new Dictionary<string, List<double>>();

			List<double> MAGE_early = new List<double>() { 113129.48260575096, 58763.17910485841, -221749.27995923607, 51914.19308005275, 113427.70379706146, 69710.3329236221, 791773.4862970789, -419332.15802029456, 32924.822024549045, -226515.02071901807, 99441.6849958362, -242334.39152073502, -264.03345116750677, -97007.43575151596, -63084.944462861924, -34986.35067498923, -190527.89222049955, 53833.699517827714, -1.3387762010097504e-09, 330980.4376136386, 118258.29104678778, -24514.9166934137, 126930.31763154756, 117987.47797302538 };
			CardClassScoreWeights.Add("MAGE_early", MAGE_early);

			List<double> MAGE_mid = new List<double>() { 275140.8652095063, 24267.81782180062, -341596.86706656124, -53728.6925883408, 383700.2833734017, 99695.52256546014, 697766.3918087691, -1114389.192543297, -64519.280027693916, -124009.3365819217, -181714.10871889777, 106384.88644999688, -199525.63416458754, -50773.18375718573, 96103.13009893344, 74037.2480715992, -430858.2179168105, -108074.29433608685, 799577.6670094477, -357242.55990056245, 163739.75976125058, -56200.41650263811, 274460.4594110615, -188973.2782936318 };
			CardClassScoreWeights.Add("MAGE_mid", MAGE_mid);

			List<double> MAGE_late = new List<double>() { 292078.04620794044, -87802.69938491273, -23815.64075959538, -69990.52716847947, 343996.6726166235, 149798.9067524485, -29704.894354199856, 197870.79529557016, 75404.3292167568, 260399.53264981767, -180059.33158779802, 263615.5687620292, -314288.53064651386, -610452.2594007589, 45045.46595803068, 13336.300889799522, -207326.47126776542, -160133.16933056316, -231992.89391508373, -350985.99264102406, -53571.203562231516, -257233.09700766817, 73393.59475786824, 24377.633103668388 };
			CardClassScoreWeights.Add("MAGE_late", MAGE_late);

			List<double> PALADIN_early = new List<double>() { 59016.240412429, -97302.89482564687, -103988.40233480567, -9206.083672386869, 199771.32084366918, 65425.488479046835, 1490120.7486821623, -306507.12750719505, -1583.576269564788, -94287.85388288651, 65209.881914837344, -173564.66712851764, -66756.63948408223, -33696.45222197907, 76888.27383259273, -91297.8222190913, -152782.8076562322, 17191.576009175165, 4.190951585769653e-09, -34202.213902684176, 52043.43370378193, 47731.393513531504, 310374.04758560576, 306233.9038681939 };
			CardClassScoreWeights.Add("PALADIN_early", PALADIN_early);

			List<double> PALADIN_mid = new List<double>() { 241461.19177614775, 83235.4816637325, -384044.72969880665, -769.3835953085854, 446677.74069683754, -66616.27761292258, -181850.63233354778, -1045410.5949131205, 43730.043136512744, -80538.43371300834, -322601.87200385914, -87949.92835633628, -323186.19433302735, -45361.929441796085, 258662.7398826414, -79376.74864350565, -371269.9270662806, 29662.96703945636, -1.367880031466484e-09, 68976.65534602437, 22804.50882101011, 68346.69401744797, 173042.93889413695, 133748.34352318427 };
			CardClassScoreWeights.Add("PALADIN_mid", PALADIN_mid);

			List<double> PALADIN_late = new List<double>() {354435.7514417769, -430351.08606962505, 91378.4100832325, -99650.12479091815, 507116.9971494142, 121046.57873183418, 535272.9150126039, 236431.0093790748, -15633.258734540248, 138288.878390164, -605991.2234811602, 38476.92413952443, -459734.0284930857, -533182.7052636257, 421686.1732231411, -181620.91058892748, -271702.41355677444, -63735.730520482844, -1047557.903077823,
-70423.1665700835, -49193.01779686579, -82800.80970103477, -61786.04498421187, 93368.17025655622};
			CardClassScoreWeights.Add("PALADIN_late", PALADIN_late);

			List<double> ROGUE_early = new List<double>() { 121236.2695185789, 501946.527920281, -176376.34322969738, 59422.3957597562, 175139.09636591634, 83264.48124877036, 756839.5901945044, -65910.62844263209, 94873.10167391655, -284074.37114341505, 40489.42213510421, -210774.26386984624, -52087.59277539664, -29626.565496355004, 146313.9694226684, -174146.15662740258, -180152.2835960845, 78076.14946243937, -1071658.2327492398, -113341.73577436246, 43103.79559542164, -151186.50241515768, 185282.78529012736, 74777.3531593558 };
			CardClassScoreWeights.Add("ROGUE_early", ROGUE_early);

			List<double> ROGUE_mid = new List<double>() { 322743.4943911939, 31433.260536216432, -324686.17224621226, 139427.22873301237, 404944.46224492596, -50676.49765373656, 1243095.6230955052, 30579.52869817111, -111217.88295978155, -108937.4818865558, -502362.1368619735, 22501.17649807078, -352542.7336199184, -77751.42248295725, 172011.0360458699, -123984.48729847975, -446114.7844514092, 10038.274729127344, -1197058.1476837795, 105017.11017853227, 132249.7821092731, 157391.0871722066, 388465.2606488808, -6875.971871848675 };
			CardClassScoreWeights.Add("ROGUE_mid", ROGUE_mid);

			List<double> ROGUE_late = new List<double>() { 439986.1657728752, -270779.2869610148, 307991.6802689001, -97230.8354503989, 409192.6148600083, 81841.7016464377, 824795.3575115561, 141532.79466508934, 68782.6275336557, 111295.05722479691, -495422.08314383304, -41708.245940862216, -477907.713443165, -444197.67533439596, 21818.305641230076, -114323.6137056087, -287189.0470517239, -117503.09589751274, -1074257.555307167, -77158.04620857855, -33962.42644497397, -86130.291612944, -45182.58853593073, 61741.192207545195 };
			CardClassScoreWeights.Add("ROGUE_late", ROGUE_late);

			List<double> SHAMAN_early = new List<double>() { 70919.38725160624, 15404.103415143698, -100626.3472467096, 61095.17584748284, 145097.26973859878, 28052.05016675243, 1835462.2221378558, -93345.77576709026, 28803.14646925117, -96732.69035359845, 76961.01092213711, -107453.79303721972, -91623.2935226137, -41932.09883948406, 130342.46073340467, -68035.24980566622, -183481.7755458436, -42688.68342831485, -2.6775524020195007e-09, 161158.5320519521, 419.44601541952727, -28459.29355250374, 204021.5406052017, 83306.54955696406 };
			CardClassScoreWeights.Add("SHAMAN_early", SHAMAN_early);

			List<double> SHAMAN_mid = new List<double>() { 254017.7287262631, 76585.16701260477, -183584.8302793467, 20604.982282508463, 385588.17286453926, -326.90751466673737, -137568.58218590924, -280558.2937614014, -59690.29921164767, -260284.76963533706, -435189.9349558906, 111091.71354367568, -351033.24841132975, -155655.90577746867, 286450.47073262744, -94066.58105799663, -390824.27013437514, 12886.511437883011, 696696.7673017666, 210416.27105527208, 107981.98534737337, 59603.396959412625, 74716.75664871946, -137396.24393073528 };
			CardClassScoreWeights.Add("SHAMAN_mid", SHAMAN_mid);

			List<double> SHAMAN_late = new List<double>() { 386271.1534948852, -469135.71476652724, 296540.4058623561, -113626.07608365711, 479744.9428361838, 103224.03044375984, 185885.60916753404, 291456.94642593956, -75867.19786160643, 142262.01035642906, -609717.8523253115, 49538.083608111774, -474410.682363235, -589973.0617396737, 541555.156678587, -164965.9295993959, -289678.0532890972, -65127.881941717234, 297916.42668893264, -74703.38355919335, -82550.16517175297, -46918.152674671626, -158907.6129476207, 185327.06123028632 };
			CardClassScoreWeights.Add("SHAMAN_late", SHAMAN_late);

			List<double> WARLOCK_early = new List<double>() { 91177.1437899179, -265438.0023376114, -235441.11044828172, 76692.30312502371, 174263.3938464159, 2528.320334144634, -885084.9286765606, -68198.7400515222, 13789.919085691294, -70749.27493287029, 82832.93916816042, -202391.2307833399, -66105.26738422437, -140021.97686154066, -19280.196587274542, -59869.07214197656, -166237.67320472535, 89474.65960880737, 4.656612873077393e-10, 423470.0194474, 172477.28724853456, -5961.590695629847, -121318.06057021434, 12148.257249370428 };
			CardClassScoreWeights.Add("WARLOCK_early", WARLOCK_early);

			List<double> WARLOCK_mid = new List<double>() { 305162.22140823514, 134961.0417151848, -118493.62971195913, 90533.706287838, 304372.71730648825, -16074.883664537969, 2007529.3423562876, 440437.0970370513, -137315.63211707195, -27213.048744404794, -321010.95419736375, 151292.27674183983, -337126.4486852175, -51656.149028401145, 131701.40933919782, -125494.99485822898, -373048.5886041441, 50696.63148229358, 1.0477378964424133e-09, 553180.127283211, 182477.22396079858, 221309.52702246068, 186011.36487137194, 6480.95420221336 };
			CardClassScoreWeights.Add("WARLOCK_mid", WARLOCK_mid);

			List<double> WARLOCK_late = new List<double>() { 478382.6653673761, -309978.3155918219, 580997.2822350758, -48579.69602073827, 292693.2651112339, 86010.4859613215, -631727.8111791787, 15813.253368370168, -93093.08414325549, 269942.6686120533, -582950.9585582613, -62986.71446022437, -467756.83243250864, -483411.54493183165, 172693.86324452623, -173132.7046247062, -224453.1799703071, -40410.12102939076, -693683.6938612313, 7123.536794955785, -25274.463333700678, 41861.128084518685, -243381.82627799961, 90754.37397172464 };
			CardClassScoreWeights.Add("WARLOCK_late", WARLOCK_late);

			List<double> WARRIOR_early = new List<double>() { 86202.35098964651, -47751.79606015571, -191171.63208411017, 46822.127391534246, 258556.52161341102, 12438.618046753652, 2579305.2462249584, -210232.83491523494, -227370.75573235066, -437252.4680364641, -129926.62561505547, -329136.09738685336, -99530.83683214268, -38837.136296429024, 89128.16791847341, -105922.59457545514, -175361.6510535612, 2366.677878813048, -2.1464074961841106e-09, 130678.8890147685, 13155.366361420138, -147802.39453467645, -6369.211104613424, 261992.18106205075 };
			CardClassScoreWeights.Add("WARRIOR_early", WARRIOR_early);

			List<double> WARRIOR_mid = new List<double>() { 330553.3852847443, -9936.461129723815, -424160.73246160225, 35574.24872094039, 526870.0086048958, -63740.238431825186, 761164.0996198126, -90079.36124183779, -247325.79239296084, -176007.03895455072, -416489.6793101805, -38240.88015719667, -435085.442097291, -22345.093102191626, 261005.29174721305, -88160.29735304236, -476615.25482148316, 80321.01093287428, -303579.3937987246, -156485.44981236357, 207523.9184452709, 179131.6558421832, 283780.8143534634, -93937.18367361209 };
			CardClassScoreWeights.Add("WARRIOR_mid", WARRIOR_mid);

			List<double> WARRIOR_late = new List<double>() { 350139.31493239, -321191.02409123315, 558933.1095721276, -232336.64167599002, 523410.0083011639, 171911.91658312592, -116960.98677282104, 137986.6252266356, -124381.06703913452, 183509.44708446757, -575721.8652999313, 208059.64250564558, -485899.3750991896, -547946.5282610595, 403694.8959527701, -136556.1231244739, -293587.1779950078, -83075.5429996005, -127878.80297425784, -125653.40334960907, -43485.0599693441, -66969.86784052433, -54534.87134677813, 183798.60902063394 };
			CardClassScoreWeights.Add("WARRIOR_late", WARRIOR_late);
		}
	}


	abstract class DynamicLookahead : AbstractAgent
	{
		public static Timer timer;
		public bool timeIsOver = false;
		public Dictionary<string, List<double>> CardClassScoreWeights;
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
		}

		public override PlayerTask GetMove(POGame game)
		{
			timeIsOver = false;
			timer.Start();
			Controller player = game.CurrentPlayer;
			// Implement a simple Mulligan Rule
			if (player.MulliganState == Mulligan.INPUT)
			{
				List<int> mulligan = new GameStateEncoding().MulliganRule().Invoke(player.Choice.Choices.Select(p => game.getGame().IdEntityDic[p]).ToList());
				return ChooseTask.Mulligan(player, mulligan);
			}


			IEnumerable<KeyValuePair<PlayerTask, POGame>> validOpts = game.Simulate(player.Options()).Where(x => x.Value != null);
			int optcount = validOpts.Count();
			int maxDepth = optcount >= 5 ? (optcount >= 25 ? 1 : 2) : 3;

			if (validOpts.Any()) return validOpts.Select(x => score(x, player.PlayerId, maxDepth)).OrderBy(x => x.Value).Last().Key;
			else return player.Options().First(x => x.PlayerTaskType == PlayerTaskType.END_TURN);

			KeyValuePair<PlayerTask, long> score(KeyValuePair<PlayerTask, POGame> state, int player_id, int max_depth = 3)
			{
				long max_score = Int64.MinValue;
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
				return new KeyValuePair<PlayerTask, long>(state.Key, max_score);
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

		private long Score(POGame state, int playerId)
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
				//Encoding.Add(HeroBaseMana);

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

			public long Rate(Dictionary<string, List<double>> scoreWeights)
			{
				if (OpHeroHp < 1)
					return Int64.MaxValue;

				if (HeroHp < 1)
					return Int64.MinValue;

				CreateEncoding();
				double score = 0;

				string stage = "";
				if (HeroBaseMana <= 3.0f) stage = "early";
				if (HeroBaseMana <= 7.0f && HeroBaseMana >= 4.0f) stage = "mid";
				if (HeroBaseMana >= 8.0f) stage = "late";
				stage = "late";
				string cardClass = Controller.HeroClass.ToString();
				List<string> availableCardClasses = new List<string> { "MAGE", "PALADIN", "ROGUE", "SHAMAN", "WARLOCK", "WARRIOR" };

				if (!availableCardClasses.Contains(cardClass))
				{
					Console.WriteLine("CardClass not available!");
					cardClass = availableCardClasses[new Random().Next(availableCardClasses.Count)];
				}

				List<double> weights = scoreWeights.GetValueOrDefault(cardClass + "_" + stage);

				for (int i = 0; i < Encoding.Count; i++)
				{
					score += Encoding[i] * weights[i];
				}
				return (long) score;
			}

			public override Func<List<IPlayable>, List<int>> MulliganRule()
			{
				return p => p.Where(t => t.Cost > 3).Select(t => t.Id).ToList();
			}
		}
	}
}
