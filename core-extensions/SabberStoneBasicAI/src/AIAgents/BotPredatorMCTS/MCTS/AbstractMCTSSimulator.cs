using SabberStoneBasicAI.PartialObservation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SabberStoneBasicAI.AIAgents.PredatorMCTS
{
    abstract class AbstractMCTSSimulator
    {
		/// <summary>
		/// The random generator.
		/// </summary>
		protected static Random rnd = new Random();

		/// <summary>
		/// The parameters which drive the MCTS simulation.
		/// </summary>
		protected MCTSParameters _mctsParameters;

		/// <summary>
		/// TODO: API
		/// </summary>
		public Stopwatch Watch
		{
			private get; set;
		}

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="mctsParameters">the parameters controlling the simulator</param>
		public AbstractMCTSSimulator(MCTSParameters mctsParameters)
		{
			_mctsParameters = mctsParameters;
		}

		/// <summary>
		/// TODO: API
		/// </summary>
		/// <param name="game"></param>
		/// <returns></returns>
		public abstract MCTSNode simulate(POGame game);

		/// <summary>
		/// Performs the MCTS simulation for to given root node and returns the best child.
		/// </summary>
		/// <param name="simulationTime"></param>
		/// <param name="root">the root node the MCTS is performed on</param>
		/// <param name="leafNodes">all leaf nodes of the MCTS simulation</param>
		/// <returns>the root's best child node</returns>
		protected MCTSNode simulate(double simulationTime, MCTSNode root, ref List<MCTSNode> leafNodes)
		{
			// simulate
			while (Watch.Elapsed.TotalMilliseconds <= simulationTime)
			{
				// SELECTION using UCT as Tree-Policy
				MCTSNode selectedNode = select(root);

				MCTSNode leafNode = selectedNode;
				if (!leafNode.IsEndTurn)
				{
					// EXPANDATION
					expand(selectedNode);
					// SIMULATION using greedy as Default Policy
					leafNode = rollOut( selectedNode);
				}

				// BACKPROPAGATE
				backpropagate(leafNode, leafNode.Score);

				// XXX: highly unsafe, leaf node is not baked up with UCT 
				//// collect all end turn nodes aka leaf nodes
				//if (leafNode.IsEndTurn && !leafNodes.Contains(leafNode))
				//{
				//	leafNodes.Add(leafNode);
				//}
				if (selectedNode.IsEndTurn && !leafNodes.Contains(selectedNode))
				{
					leafNodes.Add(selectedNode);
				}
			}

			if (root.Children == null)
			{
				expand(root);
			}

			if (root.Children.Count < 1)
			{
				return root;
			}

			return root.Children
				.OrderByDescending(c => c.TotalScore)
				.First();
		}

		/// <summary>
		/// The selection step of the MCTS. Returns the leaf node with the currently best uct score. 
		/// </summary>
		/// <param name="root">the root node the MCTS is performed on</param>
		/// <returns>the leaf node with the best uct score</returns>
		protected MCTSNode select(MCTSNode root)
		{
			MCTSNode selectedNode = root;
			while (!selectedNode.IsLeaf)
			{
				selectedNode = selectedNode
					.Children.OrderByDescending(c => calculateUCT(c)).First();

			}
			return selectedNode;
		}

		/// <summary>
		/// The expanditation step of the MCTS which adds all children to the selected leaf node.
		/// </summary>
		/// <param name="selectedNode">the selected leaf node</param>
		protected void expand(MCTSNode selectedNode)
		{
			// TODO: add something like a expansion-threshold
			selectedNode.Children = selectedNode
				.Tasks.Select(t => new MCTSNode(selectedNode.Game, t, selectedNode)).ToList();
		}

		/// <summary>
		/// The rollout step of the MCTS.
		/// </summary>
		/// <param name="expandedNode">the expanded node the simulation is performed on</param>
		/// <returns>the leaf node of the simulation</returns>
		protected MCTSNode rollOut(MCTSNode expandedNode)
		{
			// select child for simulation
			MCTSNode child;
			if (expandedNode.Children.Count < 1)
			{
				return expandedNode;
			}

			MCTSNode simulationChild = child = expandedNode
				.Children.OrderByDescending(c => c.Score).First();

			// simulate game
			int i = 0;
			while (i++ < _mctsParameters.RolloutDepth && (!simulationChild.IsEndTurn && simulationChild.IsRunning))
			{
				expand(simulationChild);
					// greedy
				if(simulationChild.Children.Any()) simulationChild = simulationChild.Children.OrderByDescending(c => c.Score).First();
			}

			if (child.Children?.Any() ?? false)
			{
				child.Children = null;
			}

			return simulationChild;
		}

		/// <summary>
		/// The backpropagation step of MCTS.
		/// </summary>
		/// <param name="leafNode">the leaf node the rollout has calculated</param>
		protected void backpropagate(MCTSNode leafNode, double score)
		{
			MCTSNode parent = leafNode;
			while (parent != null)
			{
				parent.VisitCount++;
				//parent.TotalScore += score;
				// only highest score should be backpropagated 
				if (parent.TotalScore < score)
				{
					parent.TotalScore = score;
				}

				parent = parent.Parent;
			}
		}

		/// <summary>
		/// Calculates the uct score for a given node.
		/// </summary>
		/// <param name="node">the node the uct is calculated for</param>
		/// <returns>the uct score </returns>
		private double calculateUCT(MCTSNode node)
		{
			// value form the heuristic
			double heuristic = node.TotalScore; ;
			// the exploration parameter — theoretically equal to √2
			double constant = _mctsParameters.UCTConstant; // Math.sqrt(2);
														   // the actual ucb value
			double uct = constant * Math.Sqrt(2 *
				(Math.Log(node.Parent.VisitCount) / node.VisitCount));
			return heuristic + uct;
		}
	}
}
