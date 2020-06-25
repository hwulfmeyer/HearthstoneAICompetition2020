using CsvHelper;
using SabberStoneBasicAI.AIAgents.AlvaroAgent;
using SabberStoneBasicAI.PartialObservation;
using SabberStoneCore.Enums;
using SabberStoneCore.Model;
using SabberStoneCore.Model.Entities;
using SabberStoneCore.Model.Zones;
using SabberStoneCore.Tasks.PlayerTasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Timers;

namespace SabberStoneBasicAI.AIAgents.MCTSHans
{

	class AgentHansUCB1 : AgentHans
	{
		public override void setOptions()
		{
			UCB1_C = 8000.0;
			TREE_POLICY = "ucb1";
			AGENT_NAME = "AgentHansUCB1";
		}
	}


	class AgentHansEGREEDY7 : AgentHans
	{
		public override void setOptions()
		{
			EPS = 0.7;
			TREE_POLICY = "egreedy";
			AGENT_NAME = "AgentHansEGREEDY7";
		}
	}

	class AgentHansEGREEDY5 : AgentHans
	{
		public override void setOptions()
		{
			EPS = 0.5;
			TREE_POLICY = "egreedy";
			AGENT_NAME = "AgentHansEGREEDY5";
		}
	}

	class AgentHansEGREEDY3: AgentHans
	{
		public override void setOptions()
		{
			EPS = 0.3;
			TREE_POLICY = "egreedy";
			AGENT_NAME = "AgentHansEGREEDY3";
		}
	}


	class AgentHansRANDOM : AgentHans
	{
		public override void setOptions()
		{
			TREE_POLICY = "random";
			AGENT_NAME = "AgentHansRANDOM";
		}
	}


	class AgentHansDECAY1 : AgentHans
	{
		public override void setOptions()
		{
			TREE_POLICY = "decayegreedy";
			AGENT_NAME = "AgentHansDECAY1";
		}

		public override void decay()
		{
			EPS = Math.Exp(-sw.ElapsedMilliseconds / MAX_MILLISECONDS);
		}
	}

	class AgentHansDECAY2 : AgentHans
	{
		public override void setOptions()
		{
			TREE_POLICY = "decayegreedy";
			AGENT_NAME = "AgentHansDECAY2";
		}

		public override void decay()
		{
			EPS = Math.Exp(-sw.ElapsedMilliseconds / (MAX_MILLISECONDS/2));
		}
	}

	class AgentHansDECAY3 : AgentHans
	{
		public override void setOptions()
		{
			TREE_POLICY = "decayegreedy";
			AGENT_NAME = "AgentHansDECAY3";
		}

		public override void decay()
		{
			EPS = Math.Exp(-sw.ElapsedMilliseconds / (MAX_MILLISECONDS / 3));
		}
	}

	class AgentHansDECAY4 : AgentHans
	{
		public override void setOptions()
		{
			TREE_POLICY = "decayegreedy";
			AGENT_NAME = "AgentHansDECAY4";
		}

		public override void decay()
		{
			EPS = Math.Exp(-sw.ElapsedMilliseconds / (MAX_MILLISECONDS / 4));
		}
	}


	abstract class AgentHans : AbstractAgent
	{
		private bool timeIsOver = false;
		private int totalVisitedNum = 0;
		private int numMoves = 0;
		private Random rnd = new Random();
		private static Timer timer;

		public Stopwatch sw = new Stopwatch();
		public double EPS = 0.5;
		public float MAX_MILLISECONDS = 1500;
		public int MAX_SIMULATION_DEPTH = 5;
		public double UCB1_C;
		public string TREE_POLICY;
		public string AGENT_NAME = "default";

		List<List<float>> GameStateEncodes;
		private Dictionary<string, List<double>> CardClassScoreWeights;

		public void OnTimedEvent(object source, ElapsedEventArgs e)
		{
			timeIsOver = true;
		}

		public override PlayerTask GetMove(POGame poGame)
		{
			timeIsOver = false;
			timer.Start();
			sw.Restart();
			Controller player = poGame.CurrentPlayer;
			// Implement a simple Mulligan Rule
			if (player.MulliganState == Mulligan.INPUT)
			{
				List<int> mulligan = new CustomScore().MulliganRule().Invoke(player.Choice.Choices.Select(p => poGame.getGame().IdEntityDic[p]).ToList());
				return ChooseTask.Mulligan(player, mulligan);
			}

			return GetMoveMCTS(poGame);
		}


		public virtual void decay()
		{
			EPS = Math.Exp(-sw.ElapsedMilliseconds / MAX_MILLISECONDS);
		}


		public PlayerTask GetMoveMCTS(POGame poGame)
		{
			MCTSNode root = new MCTSNode(poGame);
			int numSimulation = 0;
			//stop if confidence in the best node is high enough
			while(!timeIsOver)
			{
				if (TREE_POLICY == "decayegreedy") decay();
				MCTSNode node = Selection(root);
				MCTSNode expandedNode = node;
				int numRepeat = 1;
				if(node.parent == null || (node.task != null && node.task.PlayerTaskType != PlayerTaskType.END_TURN))
				{
					expandedNode = Expansion(ref node);
				}
				if(node.task != null && node.task.PlayerTaskType == PlayerTaskType.END_TURN)
				{
					numRepeat = poGame.CurrentOpponent.DeckZone.Count;
				}

				for (int i = 0; i < numRepeat; i++)
				{
					POGame simGame = Simulation(expandedNode, poGame.CurrentPlayer.PlayerId);
					Backpropagation(node, Score(simGame, poGame.CurrentPlayer.PlayerId));
				}
				numSimulation++;
			}
			totalVisitedNum += root.visitsNum;
			numMoves++;

			root.children.Sort((a, b) => a.value.CompareTo(b.value));
			PlayerTask task = Policy(root, "greedy").task;
			root = null;
			return task;
		}

		public void Backpropagation(MCTSNode node, float value)
		{
			node.visitsNum++;
			node.valuesSum += value;
			node.value = node.valuesSum / node.visitsNum;
			if (node.children.Count > 0)
			{
				foreach (MCTSNode child in node.children)
				{
					child.ucb1 = UCB1(child);
				}
				if(TREE_POLICY == "ucb1") node.children.Sort((a, b) => a.ucb1.CompareTo(b.ucb1));
				else node.children.Sort((a, b) => a.value.CompareTo(b.value));
			}
			if (node.parent != null)
			{
				Backpropagation(node.parent, value);
			}
		}

		public double UCB1(MCTSNode node)
		{
			return node.parent!=null?node.value + UCB1_C * Math.Sqrt(2 * Math.Log(node.parent.visitsNum) / (node.visitsNum + 1)):0;
		}

		public POGame Simulation(MCTSNode node, int myPlayerId)
		{
			int depth = MAX_SIMULATION_DEPTH;
			POGame simGame = node.poGame;
			if(node.poGame.CurrentPlayer.Id != myPlayerId) simGame = createOpponentHand(simGame.getCopy());
			while (depth > 0)
			{
				IEnumerable<KeyValuePair<PlayerTask, POGame>> subactions = simGame.Simulate(simGame.CurrentPlayer.Options()).Where(x => x.Value != null);
				if (!subactions.Any())
				{
					break;
				}

				KeyValuePair<PlayerTask, POGame> action = subactions.RandomElement(rnd);
				simGame = action.Value;
				depth--;
				if (action.Key.PlayerTaskType == PlayerTaskType.END_TURN) break;
			}
			return simGame;
		}

		public MCTSNode Expansion(ref MCTSNode node)
		{
			//create all child nodes and select one
			IEnumerable<KeyValuePair<PlayerTask, POGame>> subactions = node.poGame.Simulate(node.poGame.CurrentPlayer.Options()).Where(x => x.Value != null);

			foreach(KeyValuePair<PlayerTask, POGame> subact in subactions)
			{
				MCTSNode newNode = new MCTSNode(node, subact.Key, subact.Value);
				node.children.Add(newNode);
				
			}
			return Policy(node, "random");
		}

		public MCTSNode Selection(MCTSNode node)
		{
			if (node.children.Count == 0 || (node.task != null && node.task.PlayerTaskType == PlayerTaskType.END_TURN))
			{
				return node;
			}
			else
			{
				MCTSNode selectedNode = Policy(node, TREE_POLICY);
				return Selection(selectedNode);
			}
		}

		public MCTSNode Policy(MCTSNode node, string policy="default")
		{
			switch (policy)
			{
				case "egreedy":
					if (rnd.NextDouble() >= EPS) return Policy(node, "greedy");
					else return Policy(node, "random");

				case "ucb1":
				case "greedy":
					//children are ordered by the backpropagation
					return node.children[node.children.Count - 1];

				case "random":
				default:
					//random
					return node.children[rnd.Next(node.children.Count)];
			}
		}


		private POGame createOpponentHand(POGame game)
		{
			int cardsToAdd = game.CurrentPlayer.HandZone.Count;

			List<Card> deckCards = game.CurrentPlayer.Standard.Where(x => remainInDeck(x, game.CurrentPlayer)).ToList();
			HandZone handZone = new HandZone(game.CurrentPlayer);
			while (cardsToAdd > 0 && deckCards.Count > 0)
			{
				Card card = deckCards.RandomElement(new Random());
				//game.addCardToZone(handZone, card, game.CurrentPlayer);
				cardsToAdd--;
				deckCards = deckCards.Where(x => remainInDeck(x, game.CurrentPlayer)).ToList();
			}
			return game;
		}

		private bool remainInDeck(Card card, Controller player)
		{
			int occurences = player.HandZone.Count(c => c.Card == card) + player.GraveyardZone.Count(c => c.Card == card) + player.BoardZone.Count(c => c.Card == card);
			return card.Class == player.HeroClass && card.Cost <= player.RemainingMana && occurences < card.MaxAllowedInDeck;
		}


		public override void FinalizeAgent()
		{
		}

		public override void InitializeAgent()
		{
			timer = new System.Timers.Timer
			{
				Interval = MAX_MILLISECONDS
			};
			timer.Elapsed += OnTimedEvent;
			timer.AutoReset = true;
			timer.Enabled = true;

			setOptions();

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

		public override void InitializeGame() { }
		public override void FinalizeGame(Game game, Controller myPlayer)
		{
			bool write = false;
			bool test = false;
			Console.WriteLine(AGENT_NAME + " Avg. Simulations: " + totalVisitedNum / numMoves);
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
						using (FileStream fileStream = new FileStream(folder + "MCTSHansV1_" + myPlayer.HeroClass.ToString() + ".csv", FileMode.Append, FileAccess.Write, FileShare.None))
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

		public abstract void setOptions();

		private float Score(POGame state, int playerId)
		{
			Controller p = state.CurrentPlayer.PlayerId == playerId ? state.CurrentPlayer : state.CurrentOpponent;
			return new GameStateEncoding { Controller = p }.Rate(CardClassScoreWeights);
		}
	}


	class MCTSNode
	{

		public MCTSNode parent { get; set; }

		public PlayerTask task { get; set; }

		public POGame poGame { get; set; }

		public List<MCTSNode> children;

		public float valuesSum { get; set; }
		public int visitsNum { get; set; }

		public float value { get; set; }

		public double ucb1 { get; set; }


		public MCTSNode(POGame poGame)
		{
			parent = null;
			task = null;
			children = new List<MCTSNode>();
			this.poGame = poGame;
		}

		public MCTSNode(MCTSNode parent, PlayerTask task, POGame poGame)
		{
			this.parent = parent;
			this.task = task;
			this.poGame = poGame;
			children = new List<MCTSNode>();
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


		public override Func<List<IPlayable>, List<int>> MulliganRule()
		{
			return p => p.Where(t => t.Cost > 3).Select(t => t.Id).ToList();
		}
	}


}
