using System;
using System.Collections.Generic;
using System.Linq;
using SabberStoneCore.Model.Entities;
using SabberStoneCore.Tasks.PlayerTasks;
using SabberStoneBasicAI.PartialObservation;

namespace SabberStoneBasicAI.AIAgents.PredatorMCTS
{
	class FlatMCOptionNode : POOptionNode
	{
		private static readonly Random Rnd = new Random();

		public FlatMCOptionNode(FlatMCOptionNode parent, POGame game, int playerId, PlayerTask playerTask, Score.Score scoring)
			: base(parent, game, playerId, playerTask, scoring) { }

		public static FlatMCOptionNode GetSolutions(POGame game, int playerId, Score.Score scoring, int iterations)
		{
			var root = new FlatMCOptionNode(null, game, playerId, null, scoring);
			var children = new Dictionary<string, FlatMCOptionNode>();
			root.options(ref children);

			var optionsScoreDir = new Dictionary<FlatMCOptionNode, double>();
			var hashNodeDir = new Dictionary<string, FlatMCOptionNode>();
			foreach (FlatMCOptionNode child in children.Values)
			{
				// the simulation step of the flat monte carlo 
				FlatMCOptionNode mcNode = child;
				double score = 0;
				for (int i = 0; i < iterations; i++)
				{
					// the play-till-end step 
					while (!mcNode.IsEndTurn && mcNode.IsRunning)
					{
						// calculate next node, i.e. the step where the simulated 'move' is performed
						FlatMCOptionNode nextMCNode = null;
						mcNode.randomOption(ref nextMCNode);

						// update score using the node's score value 
						if (nextMCNode.IsWon || nextMCNode.IsEndTurn)
							score = nextMCNode.Score;
						//if (nextMCNode.IsWon)
						//	score++;

						// update node
						mcNode = nextMCNode;
					}

					// TODO: maybe use the hash of the node instead 
					//if (!optionsScoreDir.ContainsKey(mcNode))
					//	optionsScoreDir.Add(mcNode, score);
					// node should be end turn node, so add node and score 
					if (!hashNodeDir.ContainsKey(mcNode.Hash))
					{
						hashNodeDir.Add(mcNode.Hash, mcNode);
						optionsScoreDir.Add(mcNode, score);
					}
					else
					{
						// update scoring value  
						FlatMCOptionNode keyNode = hashNodeDir[mcNode.Hash];
						optionsScoreDir[keyNode] = score;

					}
				}
			}

			// returns the best node by score
			//Console.WriteLine($"* Solutions with Score: {string.Join(",", optionsScoreDir.OrderByDescending(o => o.Value).ToList().Select(o => o.Value).ToList())}");
			return optionsScoreDir.OrderByDescending(o => o.Value).ToList().First().Key;
		}

		private void options(ref Dictionary<string, FlatMCOptionNode> optionNodes)
		{
			//List<PlayerTask> options = _game.ControllerById(_playerId).Options(!_isOpponentTurn);
			Controller controller = (_game.CurrentPlayer.Id == _playerId) ? _game.CurrentPlayer : _game.CurrentOpponent;
			List<PlayerTask> options = controller.Options(!_isOpponentTurn);

			foreach (PlayerTask option in options)
			{
				var optionNode = new FlatMCOptionNode(this, _game, _playerId, option, Scoring);
				if (!optionNodes.ContainsKey(optionNode.Hash))
					optionNodes.Add(optionNode.Hash, optionNode);
			}
		}

		private void randomOption(ref FlatMCOptionNode optionNode)
		{
			//List<PlayerTask> options = _game.ControllerById(_playerId).Options(!_isOpponentTurn);
			Controller controller = (_game.CurrentPlayer.Id == _playerId) ? _game.CurrentPlayer : _game.CurrentOpponent;
			List<PlayerTask> options = controller.Options(!_isOpponentTurn);

			// play random 
			PlayerTask option = options[Rnd.Next(options.Count)];
			optionNode = new FlatMCOptionNode(this, _game, _playerId, option, Scoring);

		}
	}
}
