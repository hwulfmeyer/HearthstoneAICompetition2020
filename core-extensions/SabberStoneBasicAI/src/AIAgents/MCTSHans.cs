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
		public float MAX_MILLISECONDS = 3000;
		public int MAX_SIMULATION_DEPTH = 5;
		public double UCB1_C;
		public string TREE_POLICY;
		public string AGENT_NAME = "default";

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
					Backpropagation(node, CustomScore.Score(simGame, poGame.CurrentPlayer.PlayerId));
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

		public override void FinalizeGame()
		{
			Console.WriteLine(AGENT_NAME + " Avg. Simulations: " + totalVisitedNum/numMoves);
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

		public abstract void setOptions();

		public override void InitializeGame()
		{
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


	class CustomScore : Score.Score
	{
		public int HeroArmor => Controller.Hero.Armor;

		public int OpHeroArmor => Controller.Opponent.Hero.Armor;

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

		public static int Score(POGame state, int playerId)
		{
			Controller p = state.CurrentPlayer.PlayerId == playerId ? state.CurrentPlayer : state.CurrentOpponent;
			return new CustomScore { Controller = p }.Rate();
		}

		public override int Rate()
		{
			if (OpHeroHp < 1)
				return Int32.MaxValue;

			if (HeroHp < 1)
				return Int32.MinValue;

			double score = 0.0;

			score += scaling[0] * (HeroHp + HeroArmor);
			score -= scaling[1] * (OpHeroHp + OpHeroArmor);

			score += scaling[2] * BoardZone.Count;
			score -= scaling[3] * OpBoardZone.Count;

			score += scaling[4] * MinionTotHealth;
			score += scaling[5] * MinionTotAtk;

			score -= scaling[6] * OpMinionTotHealth;
			score -= scaling[7] * OpMinionTotAtk;
			//MinionTotHealthTaunt OpMinionTotHealthTaunt

			return (int)Math.Round(score);
		}

		public override Func<List<IPlayable>, List<int>> MulliganRule()
		{
			return p => p.Where(t => t.Cost > 3).Select(t => t.Id).ToList();
		}
	}


}
