
namespace SabberStoneBasicAI.AIAgents.PredatorMCTS
{
	/// <summary>
	/// Container class to encapsulate all MCTS specific parameters. 
	/// </summary>
	class MCTSParameters
    {
		/// <summary>
		/// The upper time bound for simulation.
		/// </summary>
		public double SimulationTime
		{ get; set; }

		/// <summary>
		/// The time for operations between simulations. 
		/// </summary>
		public double AggregationTime
		{ get; set; }

		/// <summary>
		/// Returns the maximum depth the roll-out is calculated for. 
		/// </summary>
		public int RolloutDepth
		{ get; set; }

		/// <summary>
		/// Returns the constant to weight the score against the visit count. 
		/// </summary>
		public double UCTConstant
		{ get; set; }

		/// <summary>
		/// Creates a default instance of the MCTS parameters.
		/// </summary>
		public static MCTSParameters DEFAULT
		{
			get
			{
				return new MCTSParameters
				{
					SimulationTime = 15000,
					AggregationTime = 500,
					RolloutDepth = 3,
					UCTConstant = 9000
				};
			}
		}

		public override string ToString()
		{
			//return $"Iter: {Iterations} SimDepth: {RolloutDepth} UCT: {UCTConstant}";
			return $"Simulation Time: {SimulationTime} SimDepth: {RolloutDepth} UCT: {UCTConstant}";
		}
	}	
}
