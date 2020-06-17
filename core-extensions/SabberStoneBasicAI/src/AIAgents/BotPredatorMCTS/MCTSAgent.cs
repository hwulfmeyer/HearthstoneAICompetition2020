using SabberStoneBasicAI.Score;

namespace SabberStoneBasicAI.AIAgents.PredatorMCTS
{
	class MCTSAgent : AbstractMCTSAgent
	{
		public MCTSAgent(Score.Score scoring, MCTSParameters mctsParameters){ }

		protected override AbstractMCTSSimulator initSimulator(int playerID, Score.Score scoring)
		{
			return new MCTSSimulator(playerID, scoring, _mctsParameters)
			{
				Watch = Watch
			};
		}
	}
}
