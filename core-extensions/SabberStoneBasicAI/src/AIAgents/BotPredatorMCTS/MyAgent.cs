using System;
using SabberStoneBasicAI.PartialObservation;
using SabberStoneCore.Tasks.PlayerTasks;


namespace SabberStoneBasicAI.AIAgents.PredatorMCTS
{
	class MyAgent : AbstractAgent
	{
		private Random Rnd = new Random();

		public override void FinalizeAgent()
		{
		}

		public override void FinalizeGame()
		{
		}

		public override PlayerTask GetMove(POGame poGame)
		{
			return poGame.CurrentPlayer.Options()[0];
		}

		public override void InitializeAgent()
		{
			Rnd = new Random();
		}

		public override void InitializeGame()
		{
		}
	}
}
