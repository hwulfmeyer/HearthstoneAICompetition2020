
namespace SabberStoneBasicAI.AIAgents.PredatorMCTS
{
	/// <summary>
	/// Container class to encapsulate all prediction specific parameters.
	/// </summary>
    public class PredictionParameters
    {
		/// <summary>
		/// 
		/// </summary>
		public string File
		{ get; set; }

		/// <summary>
		/// Returns the decay factor which decrease the probability of a card over time.
		/// </summary>
		public double DecayFactor
		{ get; set; }

		/// <summary>
		/// Returns the number of cards which are picked from a prediction.
		/// </summary>
		public int CardCount
		{ get; set; }

		/// <summary>
		/// Returns the number of decks used for prediction. 
		/// </summary>
		public int DeckCount
		{ get; set; }

		/// <summary>
		/// TODO: API
		/// </summary>
		public int StepWidth
		{ get; set; }

		/// <summary>
		/// Returns the number of sets consisting of hand and deck cards which are
		/// created using different permutations of predicted cards.
		/// </summary>
		public int SetCount
		{ get; set; }

		/// <summary>
		/// Returns the number of leafs, aka end turn nodes, on which the prediction driven MCTS will run.
		/// For each node, two MCTS simulation will be performed sequentially: one for the opponent; one for the player. 
		/// </summary>
		public int LeafCount
		{ get; set; }

		/// <summary>
		/// 
		/// </summary>
		public int SimulationDepth
		{ get; set; }

		/// <summary>
		/// 
		/// </summary>
		public int OverallLeafCount
		{ get; set; }

		/// <summary>
		/// Creates a default instance of the prediction parameters.
		/// </summary>
		public static PredictionParameters DEFAULT
		{
			get
			{
				return new PredictionParameters
				{
					// no decay
					DecayFactor = 1,
					CardCount = 10,
					DeckCount = 1,
					StepWidth = 2,
					SetCount = 2,
					LeafCount = 5,
					SimulationDepth = 1,
					OverallLeafCount = 5,
				};
			}
		}

		public override string ToString()
		{
			return $"CardCount: {CardCount} StepWidth: {StepWidth} SetCount: {SetCount} LeafPercentage: {LeafCount}";
		}
	}
}
