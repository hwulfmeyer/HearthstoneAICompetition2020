
using System.Collections.Generic;
using System.Diagnostics;
using SabberStoneBasicAI.PartialObservation;

namespace SabberStoneBasicAI.AIAgents.PredatorMCTS
{
	/// <summary>
	/// The base class for performing the MCTS simulation.
	/// </summary>
	class MCTSSimulator : AbstractMCTSSimulator
	{
		/// <summary>
		/// The id of the player the MCTS simulates for. 
		/// </summary>
		protected int _playerId;

		/// <summary>
		/// The scoring function of the player the MCTS simulates for.
		/// </summary>
		protected Score.Score _scoring;
		
		/// <summary>
		/// TODO: API
		/// </summary>
		protected double _deltaTime;
		

		///// <summary>
		///// TODO: API
		///// </summary>
		///// <param name="playerId">the id of the player</param>
		///// <param name="scoring">the scoring function of the player</param>
		///// <param name="oppScoring">the scoring function of the opponent</param>
		///// <param name="map">the bi-gram map</param>
		//public MCTSSimulator(int playerId, IScore scoring, BigramMap map)
		//	: this(PredictionParameters.DEFAULT, playerId, scoring, map) { }

		///// <summary>
		///// TODO: API
		///// </summary>
		///// <param name="predictionParameters"></param>
		///// <param name="playerId"></param>
		///// <param name="scoring"></param>
		///// <param name="oppScoring"></param>
		///// <param name="map"></param>
		//public MCTSSimulator(PredictionParameters predictionParameters, int playerId, IScore scoring, BigramMap map)
		//	: this(MCTSParameters.DEFAULT, predictionParameters, playerId, scoring, map) { }

		/// <summary>
		/// TODO: API
		/// </summary>
		/// <param name="mctsParameters"></param>
		/// <param name="predictionParameters"></param>
		/// <param name="playerId"></param>
		/// <param name="scoring"></param>
		/// <param name="oppScoring"></param>
		/// <param name="map"></param>
		//public MCTSSimulator(MCTSParameters mctsParameters, PredictionParameters predictionParameters,
		//	int playerId, IScore scoring, BigramMap map)
		public MCTSSimulator(int playerId, Score.Score scoring, MCTSParameters mctsParameters)
			: base (mctsParameters)
		{
			_playerId = playerId;
			_scoring = scoring;

			_deltaTime = (_mctsParameters.SimulationTime - 2 * _mctsParameters.AggregationTime);
		}
		
		public override MCTSNode simulate(POGame game)
		{
			POGame gameCopy = game.getCopy();

			// initials root node
			var initLeafs = new List<MCTSNode>();
			var root = new MCTSNode(_playerId, new List<MCTSNode.ScoreExt> { new  MCTSNode.ScoreExt(1.0, _scoring) }, gameCopy, null, null);

			// simulate 
			MCTSNode bestNode = simulate(_deltaTime, root, ref initLeafs);
			
			return bestNode;
		}
	}
}
