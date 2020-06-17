using System.Collections.Generic;
using System.Linq;
using SabberStoneCore.Tasks.PlayerTasks;
using SabberStoneBasicAI.PartialObservation;

namespace SabberStoneBasicAI.AIAgents.PredatorMCTS
{
	class ExhaustiveSeachAgent : AbstractAgentExt
	{
		private int _maxDepth;

		private int _maxWidth;

		public ExhaustiveSeachAgent(int maxDepth, int maxWidth, Score.Score scoring)
		{
			_maxDepth = maxDepth;
			_maxWidth = maxWidth;
		}

		protected override List<PlayerTask> getSolutions(POGame poGame, int playerID, Score.Score scoring)
		{
			List<POOptionNode> solutionNodes = POOptionNode.GetSolutions(poGame, playerID, scoring, _maxDepth, _maxWidth);
			var solutions = new List<PlayerTask>();
			solutionNodes.OrderByDescending(p => p.Score).First().PlayerTasks(ref solutions);
			return solutions;
		}
	}
}
