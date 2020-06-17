using System.Collections.Generic;
using SabberStoneCore.Tasks.PlayerTasks;
using SabberStoneBasicAI.Score;
using SabberStoneBasicAI.PartialObservation;

namespace SabberStoneBasicAI.AIAgents.PredatorMCTS
{
	abstract class AbstractMCTSAgent : AbstractAgentExt
	{
		protected MCTSParameters _mctsParameters;

		private AbstractMCTSSimulator _simulator;
		
		//public AbstractMCTSAgent(IScore scoring)
		//	: this(MCTSParameters.DEFAULT, PredictionParameters.DEFAULT, scoring) { }

		//public AbstractMCTSAgent(PredictionParameters predictionParameters, IScore scoring)
		//	: this (MCTSParameters.DEFAULT, predictionParameters, scoring)	{ }

		//public AbstractMCTSAgent(MCTSParameters mctsParameters, PredictionParameters predictionParameters, IScore scoring)
		//	: base(scoring)
					public AbstractMCTSAgent()
		{
			//_predictionParameters = predictionParameters;

			//if (_predictionParameters != null)
			//{
			//	_map = BigramMapReader.ParseFile(_predictionParameters.File); ;
			//}
		}

		protected override List<PlayerTask> getSolutions(POGame game, int playerID, Score.Score scoring)
		{
			// lazily instantiate  
			if (_simulator == null)
			{
				//_simulator = new MCTSSimulator(_mctsParameters, _predictionParameters,
				//	playerID, scoring, _map)
				//{
				//	Watch = Watch
				//};
				_simulator = initSimulator(playerID, scoring);
			}

			if (Watch.Elapsed.TotalMilliseconds <= (_mctsParameters.SimulationTime - _mctsParameters.AggregationTime))
			{
				return _simulator.simulate(game).GetSolution();
			}
			else
			{
				// safety net, when everything goes wrong
				return new List<PlayerTask> { EndTurnTask.Any(game.CurrentPlayer) };
			}
		}

		protected abstract AbstractMCTSSimulator initSimulator(int playerID, Score.Score scoring);
		
	}
}
