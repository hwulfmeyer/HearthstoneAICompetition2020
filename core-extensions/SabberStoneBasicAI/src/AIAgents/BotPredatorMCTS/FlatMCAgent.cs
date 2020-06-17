
using System.Collections.Generic;
using SabberStoneCore.Tasks.PlayerTasks;
using SabberStoneBasicAI.PartialObservation;

namespace SabberStoneBasicAI.AIAgents.PredatorMCTS
{
	class FlatMCAgent : AbstractAgentExt
	{
		public FlatMCAgent() { }


		protected override List<PlayerTask> getSolutions(POGame poGame, int playerID, Score.Score scoring)
		{
			var solutionNode = FlatMCOptionNode.GetSolutions(poGame, playerID, scoring, 10);
			var solutions = new List<PlayerTask>();
			solutionNode.PlayerTasks(ref solutions);
			return solutions;
		}
	}
}
