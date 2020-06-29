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

		public override void InitializeAgent()
		{
			base.InitializeAgent();
			write = true;
			test = false;
			GameStateEncodes = new List<List<float>>();
			GameStateEncodesEnd = new List<List<float>>();
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
			EPS = Math.Exp(-TimeSpan.FromTicks(sw.Elapsed.Ticks).TotalMilliseconds / MAX_MILLISECONDS);
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
			EPS = Math.Exp(-TimeSpan.FromTicks(sw.Elapsed.Ticks).TotalMilliseconds / (MAX_MILLISECONDS/2));
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
			EPS = Math.Exp(-TimeSpan.FromTicks(sw.Elapsed.Ticks).TotalMilliseconds / (MAX_MILLISECONDS / 3));
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
			EPS = Math.Exp(-TimeSpan.FromTicks(sw.Elapsed.Ticks).TotalMilliseconds / (MAX_MILLISECONDS / 4));
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
		public float MAX_MILLISECONDS = 500;
		public int MAX_SIMULATION_DEPTH = 5;
		public double UCB1_C;
		public string TREE_POLICY;
		public string AGENT_NAME = "default";

		public List<List<float>> GameStateEncodesEnd;
		public List<List<float>> GameStateEncodes;
		public Dictionary<string, List<double>> CardClassScoreWeights;
		public bool write = false;
		public bool test = false;

		public void OnTimedEvent(object source, ElapsedEventArgs e)
		{
			timeIsOver = true;
		}

		public override PlayerTask GetMove(POGame poGame)
		{
			timeIsOver = false;
			timer.Start();
			sw.Restart();
			GameStateEncodes.Add(GameStateEncoding.GetEncoding(poGame, poGame.CurrentPlayer.PlayerId));
			Controller player = poGame.CurrentPlayer;
			// Implement a simple Mulligan Rule
			if (player.MulliganState == Mulligan.INPUT)
			{
				List<int> mulligan = new GameStateEncoding().MulliganRule().Invoke(player.Choice.Choices.Select(p => poGame.getGame().IdEntityDic[p]).ToList());
				return ChooseTask.Mulligan(player, mulligan);
			}
			PlayerTask task = GetMoveMCTS(poGame);
			if(task.PlayerTaskType != PlayerTaskType.END_TURN)
			{
				GameStateEncodesEnd.Add(GameStateEncoding.GetEncoding(poGame, poGame.CurrentPlayer.PlayerId));
			}
			return task;
		}


		public virtual void decay()
		{
			EPS = Math.Exp(-TimeSpan.FromTicks(sw.Elapsed.Ticks).TotalMilliseconds / MAX_MILLISECONDS);
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
		}

		public override void InitializeGame() { }
		public override void FinalizeGame(Game game, Controller myPlayer)
		{
			Console.WriteLine(AGENT_NAME + " Avg. Simulations: " + totalVisitedNum / numMoves);
			if (write)
			{
				int GameResult = myPlayer.PlayState == PlayState.WON ? 1 : myPlayer.PlayState == PlayState.TIED ? 0 : -1;
				int GameResultHp = myPlayer.PlayState == PlayState.WON ? myPlayer.Hero.Health : myPlayer.PlayState == PlayState.TIED ? 0 : -myPlayer.Opponent.Hero.Health;

				for (int i = 0; i < GameStateEncodes.Count; i++)
				{
					GameStateEncodes[i].Add(GameResult);
					GameStateEncodes[i].Add(GameResultHp);
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
