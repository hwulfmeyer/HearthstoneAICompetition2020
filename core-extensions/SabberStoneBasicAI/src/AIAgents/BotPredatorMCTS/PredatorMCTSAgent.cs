
using System;
using System.Collections.Generic;
using System.Text;
using SabberStoneCore.Enums;
using SabberStoneCore.Model;

namespace SabberStoneBasicAI.AIAgents.PredatorMCTS
{
    class PredatorMCTSAgent : AbstractMCTSAgent
	{
		private PredictionParameters _predictionParameters;

		private BigramMap _map;
		private List<Card> deck;
		private CardClass hero;
		public string AgentAuthor = "PredatorMCTS";

		public PredatorMCTSAgent()
		{
			base._scoring = new WeightedScore(); ;
			base._mctsParameters = new MCTSParameters
			{
				SimulationTime = 15000,
				AggregationTime = 100,
				RolloutDepth = 5,
				UCTConstant = 9000
			};
			_predictionParameters = new PredictionParameters
			{
				File = Environment.CurrentDirectory + @"\src\Bigramms\bigramm_1-2017-12-2016.json.gz",
				DecayFactor = 1,
				CardCount = 10,
				StepWidth = 2,
				DeckCount = 1,
				SetCount = 3,
				LeafCount = 5,
				SimulationDepth = 1,
				OverallLeafCount = 5
			};
			deck = ControlWarlock;
			hero = CardClass.WARLOCK;

			_map = BigramMapReader.ParseFile(_predictionParameters.File); 
		}

		public static List<Card> ControlWarlock => new List<Card>(){
				//Dark Pact
				Cards.FromId ("LOOT_017"),
				Cards.FromId ("LOOT_017"),
				// Kobold Librarian
				Cards.FromId ("LOOT_014"),
				Cards.FromId ("LOOT_014"),
				// Defile
				Cards.FromId ("ICC_041"),
				Cards.FromId ("ICC_041"),
				// Stonehill Defender
				Cards.FromId ("UNG_072"),
				Cards.FromId ("UNG_072"),
				// Lesser Amethyst Spellstone
				Cards.FromId ("LOOT_043"),
				Cards.FromId ("LOOT_043"),
				// Hellfire
				Cards.FromId ("CS2_062"),
				Cards.FromId ("CS2_062"),
				// Possessed Lackey
				Cards.FromId ("LOOT_306"),
				Cards.FromId ("LOOT_306"),
				// Rin, the First Disciple
				Cards.FromId ("LOOT_415"),
				// Twisting Nether
				Cards.FromId ("EX1_312"),
				Cards.FromId ("EX1_312"),
				// Voidlord
				Cards.FromId ("LOOT_368"),
				Cards.FromId ("LOOT_368"),
				// Bloodreaver Gul'dan
				Cards.FromId ("ICC_831"),
				// Mistress of Mixtures
				Cards.FromId ("CFM_120"),
				Cards.FromId ("CFM_120"),
				// Doomsayer
				Cards.FromId ("NEW1_021"),
				Cards.FromId ("NEW1_021"),
				// N'Zoth, the Corruptor
				Cards.FromId ("OG_133"),
				// Siphon Soul
				Cards.FromId ("EX1_309"),
				// Skulking Geist
				Cards.FromId ("ICC_701"),
				//Mortal Coil
				Cards.FromId ("EX1_302"),
				// Gnomeferatu
				Cards.FromId ("ICC_407"),
				Cards.FromId ("ICC_407")
			};

		protected override AbstractMCTSSimulator initSimulator(int playerID, Score.Score scoring)
		{
			return new MCTSSimulatorExt(playerID, scoring,_mctsParameters,
				_predictionParameters, _map)
			{
				Watch = Watch
			};
		}
	}
}
