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
using System.Security.Authentication.ExtendedProtection;

namespace SabberStoneBasicAI.AIAgents.MCTSHans
{
	class AgentHans : AbstractAgent
	{
		private Stopwatch SW = new Stopwatch();
		private int maxMilliseconds = 5000;
		private int maxSimulationDepth = 3;
		private double EPS = 0.5;
		private int totalVisitedNum = 0;
		private int numMoves = 0;

		public override PlayerTask GetMove(POGame poGame)
		{
			SW.Restart();
			Controller player = poGame.CurrentPlayer;
			// Implement a simple Mulligan Rule
			if (player.MulliganState == Mulligan.INPUT)
			{
				List<int> mulligan = new CustomScore().MulliganRule().Invoke(player.Choice.Choices.Select(p => poGame.getGame().IdEntityDic[p]).ToList());
				return ChooseTask.Mulligan(player, mulligan);
			}

			return GetMoveMCTS(poGame);
		}


		public PlayerTask GetMoveMCTS(POGame poGame)
		{
			Node root = new Node();
			while(SW.ElapsedMilliseconds < maxMilliseconds)
			{
				POGame mctsGame = poGame.getCopy();
				Node node = Selection(root, ref mctsGame);
				if(node.parent == null || (node.task != null && node.task.PlayerTaskType != PlayerTaskType.END_TURN))
				{
					node = Expansion(node, ref mctsGame);
					Simulation(node, ref mctsGame);
					Backpropagation(node, CustomScore.Score(mctsGame, poGame.CurrentPlayer.PlayerId));
				}
				else if(node.visitedNum == 0) Backpropagation(node, CustomScore.Score(mctsGame, poGame.CurrentPlayer.PlayerId));

			}
			Console.WriteLine(root.visitedNum);
			totalVisitedNum += root.visitedNum;
			numMoves++;
			return Policy(root, "greedy").task;
		}

		private void Backpropagation(Node node, float value)
		{
			node.visitedNum++;
			node.valuesSum += value;
			if (node.parent != null)
			{
				Backpropagation(node.parent, value);
			}
		}

		public void Simulation(Node node, ref POGame poGame)
		{
			if (node.task.PlayerTaskType == PlayerTaskType.END_TURN) return;
			int depth = maxSimulationDepth;
			while (depth > 0)
			{
				IEnumerable<KeyValuePair<PlayerTask, POGame>> subactions = poGame.Simulate(poGame.CurrentPlayer.Options()).Where(x => x.Value != null);
				KeyValuePair<PlayerTask, POGame> action = subactions.RandomElement(new Random());
				poGame = action.Value;
				depth--;
				if (action.Key.PlayerTaskType == PlayerTaskType.END_TURN) return;
			}			
		}


		public Node Expansion(Node node, ref POGame poGame)
		{
			//create all child nodes and select one
			IEnumerable<KeyValuePair<PlayerTask, POGame>> subactions = poGame.Simulate(poGame.CurrentPlayer.Options()).Where(x => x.Value != null);

			foreach(KeyValuePair<PlayerTask, POGame> subact in subactions)
			{
				Node newNode = new Node(subact.Key, node);
				
			}
			KeyValuePair<PlayerTask, POGame> action = subactions.RandomElement(new Random());
			poGame = action.Value;
			return node.children.First(child => child.task == action.Key);

		}

		public Node Selection(Node node, ref POGame poGame)
		{
			if (node.children.Count == 0 || (node.task != null && node.task.PlayerTaskType == PlayerTaskType.END_TURN))
			{
				return node;
			}
			else
			{
				Node selectedNode = Policy(node, "egreedy");
				poGame = poGame.Simulate(new List<PlayerTask>() { selectedNode.task }).First().Value;
				return Selection(selectedNode, ref poGame);
			}
		}

		public Node Policy(Node node, string policy ="greedy")
		{
			switch (policy)
			{
				case "softmax":

				case "egreedy":
					if (new Random().NextDouble() >= EPS) return node.children.OrderBy(child => child.visitedNum!=0?child.valuesSum / child.visitedNum:0).Last();
					else return node.children.RandomElement(new Random());

				case "greedy":
					return node.children.OrderBy(child => child.valuesSum / child.visitedNum).Last();

				default:
					//random
					return node.children.RandomElement(new Random());
			}
		}



		public override void FinalizeAgent()
		{
		}

		public override void FinalizeGame()
		{
			Console.WriteLine("Avg. Simulations: " + totalVisitedNum/numMoves);
		}

		public override void InitializeAgent()
		{
		}

		public override void InitializeGame()
		{
		}
	}


	class Node
	{

		public Node parent { get; set; }

		public PlayerTask task { get; set; }

		public List<Node> children;

		public float valuesSum { get; set; }
		public int visitedNum { get; set; }

		public Node()
		{
			parent = null;
			task = null;
			children = new List<Node>();
		}

		public Node(PlayerTask task, Node parent)
		{
			this.parent = parent;
			this.task = task;
			children = new List<Node>();
			parent.children.Add(this);
		}

	}


	class CustomScore : Score.Score
	{
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

			score += scaling[0] * HeroHp;
			score -= scaling[1] * OpHeroHp;

			score += scaling[2] * BoardZone.Count;
			score -= scaling[3] * OpBoardZone.Count;

			foreach (Minion boardZoneEntry in BoardZone)
			{
				score += scaling[4] * boardZoneEntry.Health;
				score += scaling[5] * boardZoneEntry.AttackDamage;
			}

			foreach (Minion boardZoneEntry in OpBoardZone)
			{
				score -= scaling[6] * boardZoneEntry.Health;
				score -= scaling[7] * boardZoneEntry.AttackDamage;
			}

			return (int)Math.Round(score);
		}

		public override Func<List<IPlayable>, List<int>> MulliganRule()
		{
			return p => p.Where(t => t.Cost > 3).Select(t => t.Id).ToList();
		}
	}


}
