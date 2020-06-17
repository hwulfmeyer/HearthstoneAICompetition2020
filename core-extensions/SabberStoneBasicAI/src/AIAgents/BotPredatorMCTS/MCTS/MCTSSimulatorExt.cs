using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SabberStoneCore.Enums;
using SabberStoneCore.Exceptions;
using SabberStoneCore.Model;
using SabberStoneCore.Model.Entities;
using SabberStoneCore.Model.Zones;
using SabberStoneCore.Tasks.PlayerTasks;
using SabberStoneBasicAI.PartialObservation;
using SabberStoneBasicAI.Meta;
using SabberStoneBasicAI.Score;

namespace SabberStoneBasicAI.AIAgents.PredatorMCTS
{
	/// <summary>
	/// The extended MCTS simulator class using a prediction for simulating the opponent's moves.
	/// </summary>
    class MCTSSimulatorExt : MCTSSimulator
    {
		/// <summary>
		/// The parameters to control the prediction.  
		/// </summary>
		private PredictionParameters _predictionParameters;

		/// <summary>
		/// The bi-gram map storing the information needed to perform a prediction.
		/// </summary>
		private BigramMap _map;

		/// <summary>
		/// The cards which the opponent has currently on the board. 
		/// </summary>
		private BoardCards _oppBoardCards;

		/// <summary>
		/// The last predicted cards and there probabilities. 
		/// </summary>
		private List<PredictedCards> _lastPredictionCards;

		/// <summary>
		/// The opponent's history of played cards. 
		/// </summary>
		private List<PlayHistoryEntry> _oppHistory;

		public MCTSSimulatorExt(int playerId, Score.Score scoring,
			MCTSParameters mctsParameters, PredictionParameters predictionParameters, BigramMap map)
			: base(playerId, scoring, mctsParameters)
		{
			_predictionParameters = predictionParameters;
			_map = map;

			_oppHistory = null;
			_oppBoardCards = new BoardCards();

			// the delta time for each simulation is:
			// a) depth * 2, because we simulation for every depth the opponent and the player at once 
			// b) + 1, because we always simulation the player first
			_deltaTime = (_mctsParameters.SimulationTime
				- ((_predictionParameters.SimulationDepth * 2) + 1) * _mctsParameters.AggregationTime)
				/ ((_predictionParameters.SimulationDepth * 2) + 1);
		}

		/// <summary>
		/// TODO: API
		/// </summary>
		/// <param name="game"></param>
		/// <returns></returns>
		public override MCTSNode simulate(POGame game)
		{
			POGame gameCopy = game.getCopy();

			// initials root node
			var initLeafs = new List<MCTSNode>();
			var root = new MCTSNode(_playerId, new List<MCTSNode.ScoreExt> { new MCTSNode.ScoreExt(1.0, _scoring) }, gameCopy, null, null);

			// simulate 
			MCTSNode bestNode = simulate(_deltaTime, root, ref initLeafs);
			
			// initials opponent's history
			if (_oppHistory == null)
			{
				List<PlayHistoryEntry> history = gameCopy.CurrentOpponent.PlayHistory;
				PlayHistoryEntry[] copyHistory = new PlayHistoryEntry[history.Count];
				history.CopyTo(copyHistory);
				_oppHistory = copyHistory.ToList();
			}

				var simulationQueue = new Queue<KeyValuePair<POGame, List<MCTSNode>>>();
				simulationQueue.Enqueue(new KeyValuePair<POGame, List<MCTSNode>>(gameCopy, initLeafs));

				int i = 0;
				while (i < _predictionParameters.SimulationDepth
					&& simulationQueue.Count > 0)
				{
					// calculate the lower and upper time bound of the current depth
					double lowerSimulationTimeBound = _deltaTime + i * (2 * _deltaTime);

					KeyValuePair<POGame, List<MCTSNode>> simulation = simulationQueue.Dequeue();
					POGame simulationGame = simulation.Key;
					List<MCTSNode> leafs = simulation.Value;

					leafs = leafs.Where(l => l.Game != null)
						.OrderByDescending(l => l.Score)
						.Take((leafs.Count > _predictionParameters.LeafCount)
							? _predictionParameters.LeafCount : leafs.Count)
						.ToList();
					if (leafs.Count() < 0)
					{
						return bestNode;
					}

					Controller opponent = getOpponent(simulationGame);
					List<Prediction> predicitionMap = getPredictionMap(simulationGame, opponent);
					var oldSimulations = new Dictionary<POGame, List<MCTSNode>>();

					// the simulation time for one leaf
					double timePerLeaf = (2 * _deltaTime) / leafs.Count;

					// get all games from all leaf nodes
					for (int j = 0; j < leafs.Count; j++)
					{
						// calculate the lower time bound of the current leaf
						double lowerLeafTimeBound = lowerSimulationTimeBound + j * timePerLeaf;

						MCTSNode leafNode = leafs[j];
						POGame oppGame = leafNode.Game;
						double leafScore;
						// XXX: game can be null

						leafScore = simulateOpponentWithPrediction(lowerLeafTimeBound, timePerLeaf,
							oppGame, opponent, predicitionMap, ref oldSimulations);
						// back-propagate score
						backpropagate(leafNode, leafScore);
					}

					var newSimulations = new Dictionary<POGame, List<MCTSNode>>();
					oldSimulations.ToList()
						.OrderByDescending(s => s.Value.Sum(l => l.TotalScore))
						.Take((leafs.Count > _predictionParameters.OverallLeafCount) ? _predictionParameters.OverallLeafCount : leafs.Count)
						.ToList()
						.ForEach(l => newSimulations.Add(l.Key, l.Value));

					// add new simulations
					foreach (KeyValuePair<POGame, List<MCTSNode>> sim in oldSimulations)
					{
						simulationQueue.Enqueue(sim);
					}
					i++;
				}
				return root.Children
					.OrderByDescending(c => c.TotalScore)
					.First();

		}

		/// <summary>
		/// TODO: API
		/// </summary>
		/// <param name="oppGame"></param>
		/// <param name="opponent">the controller of the opponent</param>
		/// <param name="predicitionMap">the map with all predictions</param>
		/// <param name="newSimulations"></param>
		/// <returns></returns>
		private double simulateOpponentWithPrediction(double lowerTimeBound, double timePerLeaf, POGame oppGame, Controller opponent,
			List<Prediction> predicitionMap, ref Dictionary<POGame, List<MCTSNode>> newSimulations)
		{
			double predictionScore = 0;
			if (predicitionMap?.Any() ?? false)
			{
				int denominator = predicitionMap.Count;
				var scorings = predicitionMap.GroupBy(p => p.Deck.Scoring)
					.Select(c => new MCTSNode.ScoreExt(((double)c.Count() / denominator), c.Key))
					.OrderByDescending(s => s.Value).ToList();

				// the simulation time for one prediction
				double timePerPrediction = timePerLeaf / predicitionMap.Count;

				// use prediction for each game 
				for (int i = 0; i < predicitionMap.Count; i++)
				{
					// forward game
					POGame forwardGame = oppGame.getCopy();

					Prediction prediction = predicitionMap[i];
					var setasideZone = forwardGame.CurrentPlayer.ControlledZones[Zone.SETASIDE] as SetasideZone;
					setasideZone = new SetasideZone(forwardGame.CurrentPlayer);

					// create deck zone
					List<Card> deckCards = prediction.Deck.Cards;
					var deckZone = forwardGame.CurrentPlayer.ControlledZones[Zone.DECK] as DeckZone;
					deckZone = new DeckZone(forwardGame.CurrentPlayer);
					createZone(forwardGame.CurrentPlayer, deckCards, deckZone, ref setasideZone);
					deckZone.Shuffle();
					forwardGame.CurrentPlayer.DeckZone = deckZone;

					// create hand zone
					List<Card> handCards = prediction.Hand.Cards;
					var handZone = forwardGame.CurrentPlayer.ControlledZones[Zone.HAND] as HandZone;
					handZone = new HandZone(forwardGame.CurrentPlayer);
					createZone(forwardGame.CurrentPlayer, handCards, handZone, ref setasideZone);
					forwardGame.CurrentPlayer.HandZone = handZone;

					var oppLeafNodes = new List<MCTSNode>();
					Score.Score oppStrategy = prediction.Deck.Scoring;


					// upper time bound for simulation the opponent using the current prediction
					double oppSimulationTime = lowerTimeBound + (i + 1) * timePerPrediction / 2;

					// simulate opponent's moves
					while (forwardGame != null
						&& forwardGame.State == State.RUNNING
						&& forwardGame.CurrentPlayer.Id == opponent.Id)
					{
						// simulate
						var oppRoot = new MCTSNode(opponent.Id, scorings, forwardGame, null, null);
						MCTSNode bestOppNode = simulate(oppSimulationTime, oppRoot, ref oppLeafNodes);
						// get solution
						List<PlayerTask> solutions = bestOppNode.GetSolution();
						for (int j = 0; j < solutions.Count && (forwardGame != null); j++)
						{
							PlayerTask oppTask = solutions[j];
							Dictionary<PlayerTask, POGame> dir = forwardGame.Simulate(new List<PlayerTask> { oppTask });
							forwardGame = dir[oppTask];

							if (forwardGame != null && forwardGame.CurrentPlayer.Choice != null)
							{
								break;
							}
						}
					}

					// upper time bound for simulation the player using the forwarded game 
					double simulationTime = oppSimulationTime + timePerPrediction / 2;
					double score = 0;
					var leafs = new List<MCTSNode>();

					// simulate player using forwarded opponent game
					while (forwardGame != null
						&& forwardGame.State == State.RUNNING
						&& forwardGame.CurrentPlayer.Id == _playerId)
					{
						// simulate
						var root = new MCTSNode(_playerId, new List<MCTSNode.ScoreExt> { new MCTSNode.ScoreExt(1.0, _scoring) }, forwardGame, null, null);
						MCTSNode bestNode = simulate(simulationTime, root, ref leafs);
						// get solution
						List<PlayerTask> solutions = bestNode.GetSolution();
						for (int j = 0; j < solutions.Count && (forwardGame != null); j++)
						{
							PlayerTask task = solutions[j];
							Dictionary<PlayerTask, POGame> dir = forwardGame.Simulate(new List<PlayerTask> { task });
							forwardGame = dir[task];

							if (forwardGame != null && forwardGame.CurrentPlayer.Choice != null)
							{
								break;
							}
						}
						// TODO: maybe penalty forwardGame == null
						score = bestNode.TotalScore;
					}
					predictionScore += score;

					if (forwardGame != null)
					{
						newSimulations.Add(forwardGame, leafs);
					}
				}
			}
			return predictionScore;
		}

		/// <summary>
		/// TODO: API
		/// </summary>
		/// <param name="simulationGame">the game the prediction is performed on</param>
		/// <param name="opponent">the controller of the opponent</param>
		/// <returns></returns>
		private List<Prediction> getPredictionMap(POGame simulationGame, Controller opponent)
		{
			PlayedCards playedCards = updateTasks(_predictionParameters.DecayFactor, opponent);
			var predictionMap = new List<Prediction>();
			if (playedCards.CardEntries?.Any() ?? false)
			{
				Dictionary<CardClass, double> predictedDeckClasses = predictDeckClasses(opponent, _oppBoardCards);
				if (_lastPredictionCards == null)
				{
					_lastPredictionCards = predictCards(_predictionParameters.CardCount, simulationGame,
						playedCards, _oppBoardCards, predictedDeckClasses);
				}

				var combinedPredictedCards = new PredictedCards(_lastPredictionCards
						.SelectMany(p => p.CardEntries)
						.ToList());

				List<Deck> predictedDecks = predictDecks(combinedPredictedCards, _oppBoardCards);

				var predictedDecksCopy = new List<Deck>();
				foreach (Deck deck in predictedDecks)
				{
					Deck predictedDeckCopy = copyDeck(deck);
					// remove already drawn cards from deck
					_oppBoardCards.Cards.ForEach(c => {
						if (predictedDeckCopy.Cards.Contains(c))
						{
							predictedDeckCopy.Remove(c);
						}
					});
					predictedDecksCopy.Add(predictedDeckCopy);
				}

				int handZoneCount = opponent.HandZone.Count;

				createPrediction(handZoneCount, combinedPredictedCards, predictedDecksCopy, ref predictionMap);
			}
			return predictionMap;
		}

		/// <summary>
		/// Updates the opponent's history of played cards and returns the last played cards of the opponent. 
		/// This cards will be used for the prediction.
		/// </summary
		/// <param name="decayFactor">the decay factor by which the probability will be decrease</param>
		/// <param name="opponent">the controller of the opponent</param>
		/// <returns>the last played cards</returns>
		private PlayedCards updateTasks(double decayFactor, Controller opponent)
		{
			double initValue = 1;
			// add cards which were in the opponent's board and weren't played
			Minion[] oppBoard = opponent.BoardZone.GetAll();
			oppBoard.Select(b => b.Card)
				.Where(c => !_oppBoardCards.Cards.Contains(c)).ToList()
				.ForEach(c => {
					_oppBoardCards.Add(c, initValue);
				});

			List<PlayHistoryEntry> history = opponent.PlayHistory;
			history.Where(h => !_oppHistory.Contains(h)).ToList()
				.Select(h => h.SourceCard)
				.Where(c => !_oppBoardCards.Cards.Contains(c)).ToList()
				.ForEach(c =>
				{
					_oppBoardCards.Add(c, initValue);
				});

			PlayHistoryEntry[] copyHistory = new PlayHistoryEntry[history.Count];
			history.CopyTo(copyHistory);
			_oppHistory = copyHistory.ToList();

			return _oppBoardCards;
		}

		/// <summary>
		/// Returns a prediction of the opponent's deck classes, i.e. set of classes and there probability,
		/// based on the board cards of the opponent.
		/// </summary>
		/// <param name="opponent">the controller of the opponent</param>
		/// <param name="boardCards">the board cards of the opponent</param>
		/// <returns>a set of classes and there probability</returns>
		private Dictionary<CardClass, double> predictDeckClasses(Controller opponent, BoardCards boardCards)
		{
			int denominator = boardCards.Count;
			// fetch all classes of the opponent's board cards 
			var boardClasses = new List<CardClass>();
			boardClasses.AddRange(boardCards.Cards
				.Select(c => c.Class)
				.ToList());

			// add the class of the opponent's hero 
			CardClass heroClass = opponent.HeroClass;
			if (!boardClasses.Contains(heroClass))
			{
				boardClasses.Add(heroClass);
				// increment denominator because the hero is not part of the opponent's board
				denominator++;
			}

			var deckClasses = new Dictionary<CardClass, double>();
			boardClasses.GroupBy(c => c)
				// calculate card class distribution over all already drawn cards
				.Select(c => new KeyValuePair<CardClass, double>(c.Key, ((double)c.Count() / denominator)))
				.OrderByDescending(c => c.Key)
				.ToList().ForEach(c => deckClasses.Add(c.Key, c.Value));
			return deckClasses;
		}

		/// <summary>
		/// Returns a prediction of the opponent's hand cards, i.e. set of cards and there probability,
		/// based on the bi-gram map and cards which haven been played the opponent.
		/// </summary>
		/// <param name="cardCount">the number of sets which will by predicted</param>
		/// <param name="simulationGame">the game the prediction is performed on</param>
		/// <param name="playedCards">the cards which have been played by the opponent</param>
		/// <param name="boardCards">the board cards of the opponent</param>
		/// <param name="classPrediction">the prediction of the deck classes of the opponent</param>
		/// <returns>the set of cards and there probability</returns>
		private List<PredictedCards> predictCards(int cardCount, POGame simulationGame, PlayedCards playedCards,
			BoardCards boardCards, Dictionary<CardClass, double> classPrediction)
		{
			List<Dictionary<string, double>> occMaps = getOccurenceMaps(playedCards);
			var combinedOccMap = new Dictionary<string, double>();
			foreach (Dictionary<string, double> occMap in occMaps)
			{
				occMap.ToList().ForEach(o => {
					string cardId = o.Key;
					double cardValue = o.Value;
					if (combinedOccMap.ContainsKey(cardId))
					{
						// greedy approach
						if (combinedOccMap[cardId] < cardValue)
						{
							combinedOccMap[cardId] = cardValue;
						}
					}
					else
					{
						combinedOccMap.Add(cardId, cardValue);
					}
				});
			}
			Dictionary<Card, double> playableCards = convertToCardMap(combinedOccMap, classPrediction);

			// get number of cards in relation to the class distribution of already drawn cards
			var tempPredictedCardGroups = new List<PredictedCards>();
			classPrediction.ToList().ForEach(c =>
			{
				// filter cards if they have already been exhausted or not
				var filterdPlayableCards = new PredictedCards(
					playableCards.Where(p => p.Key.Class == c.Key)
					.Where(p =>
					{
						if (p.Key.Rarity == Rarity.LEGENDARY)
						{
							return (boardCards.Cards
								.Count(b => b == p.Key) < 1);
						}
						else
						{
							return (boardCards.Cards
								.Count(b => b == p.Key) < 2);
						}
					}).ToList());

				int share = (int)(c.Value * cardCount);
				var predictedCards = new PredictedCards(filterdPlayableCards
					.CardEntries.Take(share)
					.ToList());
				tempPredictedCardGroups.Add(predictedCards);
			});

			var combinedPredictedCards = new PredictedCards(
				tempPredictedCardGroups.SelectMany(g => g.CardEntries)
					.ToList());
			double sum = combinedPredictedCards.Sum;

			var predictedCardGroups = new List<PredictedCards>();
			foreach (PredictedCards group in tempPredictedCardGroups)
			{
				var newGroup = new PredictedCards();
				group.CardEntries
					// sort list
					.OrderByDescending(c => c.Value)
					.ToList().ForEach(c => newGroup.Add(c.Key, (c.Value / sum)));
				predictedCardGroups.Add(newGroup);
			}
			return predictedCardGroups;
		}


		/// <summary>
		/// Creates a prediction using the specified parameters and adds these to the prediction map. 
		/// </summary>
		/// <param name="handZoneCount">the number of the opponent's hand cards</param>
		/// <param name="predictedCards">the predicted hand cards of the opponent</param>
		/// <param name="predictedDecks">the predicted decks of the opponent</param>
		/// <param name="predictionMap">the map the prediction is added to</param>
		private void createPrediction(int handZoneCount, PredictedCards predictedCards, List<Deck> predictedDecks,
			ref List<Prediction> predictionMap)
		{
			int deckCount = 0;
			for (int d = 0; d < predictedDecks.Count
				&& deckCount < _predictionParameters.DeckCount; d++)
			{
				Deck predictedDeck = predictedDecks[d];
				List<KeyValuePair<Card, double>> cardPool = predictedDeck.CardEntries;

				int predictionsCount = 0;
				//for (int i = 0; i < (cardPool.Count - handZoneCount)
				//	&& predictionMap.Count() < _predictionParameters.SetCount;
				//	i += _predictionParameters.StepWidth)
				for (int i = 0; i < (cardPool.Count - handZoneCount)
					&& predictionsCount < _predictionParameters.SetCount;
					i += _predictionParameters.StepWidth)
				{
					// create hand 
					var hand = new Hand();
					int j = 0;
					while (j < handZoneCount && j < cardPool.Count)
					{
						KeyValuePair<Card, double> card = cardPool[i + j];
						hand.Add(card);
						j++;
					}
					double sum = hand.Sum;
					// create deck
					Deck deck = copyDeck(predictedDeck);
					hand.Cards.ForEach(h =>
					{
						if (deck.Cards.Contains(h))
						{
							deck.Remove(h);
						}
					});
					predictionMap.Add(new Prediction(sum, hand, deck));
					predictionsCount++;
				}
				deckCount++;
			}
		}

		/// <summary>
		/// Returns a map of predicted card ids based on given cards. 
		/// </summary>
		/// <param name="playedCards">the cards the prediction is performed with</param>
		/// <returns>the map of card ids and there probability</returns>
		private List<Dictionary<string, double>> getOccurenceMaps(PlayedCards playedCards)
		{
			var occurMaps = new List<Dictionary<string, double>>();
			foreach (Card card in playedCards.Cards)
			{
				var occurMap = new Dictionary<string, double>();
				var occurList = _map.GetOccurrencesList(card.Id).ToList();
				occurList.ForEach(o =>
				{
					string cardId = o.cardId;
					double numOcc = o.numOcc;
					occurMap.Add(cardId, numOcc);
				});
				occurMaps.Add(occurMap);
			}
			return occurMaps;
		}

		/// <summary>
		/// Returns and converts a map of (card) ids into a map of cards.  
		/// </summary>
		/// <param name="predictionMap">the predicted map</param>
		/// <param name="predictClasses">the predicted classes</param>
		/// <returns>the map of cards and there probability</returns>
		private Dictionary<Card, double> convertToCardMap(Dictionary<string, double> predictionMap,
			Dictionary<CardClass, double> predictClasses)
		{
			var cardDic = new Dictionary<Card, double>();
			// get only cards which exist within sabberstone
			predictionMap.Where(c => getDecks().Values
				.SelectMany(d => d.Cards)
				.ToList()
				.Exists(cd => cd.Id == c.Key))
			// order cards by occurrence 
			.OrderByDescending(c => c.Value)
			// get card by first match		
			.ToList()
			.ForEach(c =>
			{
				Card card = getDecks().Values
					.SelectMany(d => d.Cards)
					.ToList()
					.First(cd => cd.Id == c.Key);
				double value = c.Value;
				if (predictClasses.Select(op => op.Key).Contains(card.Class))
				{
					cardDic.Add(card, value);
				}
			});
			return cardDic;
		}

		/// <summary>
		/// Returns a prediction of the opponent's strategy and deck, i.e. a scoring function and a set of cards, based on
		/// the predicted cards of the opponent. 
		/// </summary>
		/// <param name="predictedCards">the cards the strategy and the deck is predicted with</param>
		/// <param name="boardCards">the board cards of the opponent</param>
		/// <returns>the scoring function and a set of cards</returns>
		private List<Deck> predictDecks(PredictedCards predictedCards, BoardCards boardCards)
		{
			var allCards = predictedCards.Cards
				.Union(boardCards.Cards)
				.ToList();

			return getDecks().Values.OrderByDescending(d =>
			{
				int hit = 0;
				foreach (Card card in d.Cards)
				{
					if (allCards.Contains(card))
					{
						hit++;
					}
				}
				double value = ((double)(hit) / allCards.Count);
				return value;
			}).ToList();
		}

		/// <summary>
		/// Creates a zone for the opponent using the specified cards and zone.
		/// </summary>
		/// <param name="opponent">the controller of the opponent</param>
		/// <param name="predictedCards">the predicted cards the zone is populated with</param>
		/// <param name="zone">the zone who will be created</param>
		/// <param name="setasideZone">the setaside zone</param>
		/// <returns>the playables of the zone based on the cards</returns>
		private List<IPlayable> createZone(Controller opponent, List<Card> predictedCards,
			IZone zone, ref SetasideZone setasideZone)
		{
			var deck = new List<IPlayable>();
			foreach (Card card in predictedCards)
			{
				var tags = new Dictionary<GameTag, int>();
				tags[GameTag.ENTITY_ID] = opponent.Game.NextId;
				tags[GameTag.CONTROLLER] = opponent.PlayerId;
				tags[GameTag.ZONE] = (int)zone.Type;
				IPlayable playable = null;
				switch (card.Type)
				{
					case CardType.MINION:
						playable = new Minion(opponent, card, tags);
						break;

					case CardType.SPELL:
						playable = new Spell(opponent, card, tags);
						break;

					case CardType.WEAPON:
						playable = new Weapon(opponent, card, tags);
						break;

					case CardType.HERO:
						tags[GameTag.ZONE] = (int)SabberStoneCore.Enums.Zone.PLAY;
						tags[GameTag.CARDTYPE] = card[GameTag.CARDTYPE];
						playable = new Hero(opponent, card, tags);
						break;

					case CardType.HERO_POWER:
						tags[GameTag.COST] = card[GameTag.COST];
						tags[GameTag.ZONE] = (int)SabberStoneCore.Enums.Zone.PLAY;
						tags[GameTag.CARDTYPE] = card[GameTag.CARDTYPE];
						playable = new HeroPower(opponent, card, tags);
						break;

					default:
						throw new EntityException($"Couldn't create entity, because of an unknown cardType {card.Type}.");
				}

				opponent.Game.IdEntityDic.Add(playable.Id, playable);

				// add entity to the appropriate zone if it was given
				zone?.Add(playable);

				if (playable.ChooseOne)
				{
					playable.ChooseOnePlayables[0] = Entity.FromCard(opponent,
							Cards.FromId(playable.Card.Id + "a"),
							new Dictionary<GameTag, int>
							{
								[GameTag.CREATOR] = playable.Id,
								[GameTag.PARENT_CARD] = playable.Id
							},
							setasideZone);
					playable.ChooseOnePlayables[1] = Entity.FromCard(opponent,
							Cards.FromId(playable.Card.Id + "b"),
							new Dictionary<GameTag, int>
							{
								[GameTag.CREATOR] = playable.Id,
								[GameTag.PARENT_CARD] = playable.Id
							},
							setasideZone);
				}
				deck.Add(playable);
			}
			return deck;
		}

		/// <summary>
		/// Returns the opponent of the player who is specified by the <code>_playerId</code>.  
		/// </summary>
		/// <param name="game">the game the opponent is specified from</param>
		/// <returns>the opponent of the player</returns>
		private Controller getOpponent(POGame game)
		{
			if (game.CurrentPlayer.Id == _playerId)
			{
				return game.CurrentPlayer.Opponent;
			}
			else
			{
				return game.CurrentOpponent.Opponent;
			}
		}

		/// <summary>
		/// Returns a copy of the card list.
		/// </summary>
		/// <param name="deck">the card list to copy</param>
		/// <returns>the copied card list</returns>
		private List<Card> copyCards(List<Card> deck)
		{
			Card[] deckCopyArray = new Card[deck.Count()];
			deck.CopyTo(deckCopyArray);
			return deckCopyArray.ToList();
		}

		/// <summary>
		/// Returns a copy of the deck.
		/// </summary>
		/// <param name="deck">the deck to copy</param>
		/// <returns>the copied deck</returns>
		private Deck copyDeck(Deck deck)
		{
			List<Card> cards = deck.Cards;
			List<CardClass> classes = deck.Classes;
			Score.Score scoring = deck.Scoring;
			// deep-copy deck cards 
			List<Card> copiedCards = copyCards(cards);
			return new Deck(copiedCards, classes, scoring);
		}

		/// <summary>
		/// Initials a mapping of all decks, i.e. list of cards, within sabberstone to their card classes and a scoring function. 
		/// </summary>
		/// <returns>the deck map</returns>
		private static Dictionary<string, Deck> getDecks()
		{
			var deckMap = new Dictionary<string, Deck>();

			List<Card> aggroPirateWarrior = Decks.AggroPirateWarrior;
			var aggroPirateWarriorClasses = aggroPirateWarrior.GroupBy(c => c.Class)
				.OrderByDescending(c => c.Count())
				.Select(p => p.Key)
				.ToList();
			var aggroPirateWarriorDeck = new Deck(aggroPirateWarrior, aggroPirateWarriorClasses, new AggroScore());
			deckMap.Add("AggroPirateWarrior", aggroPirateWarriorDeck);

			List<Card> midrangeBuffPaladin = Decks.MidrangeBuffPaladin;
			var midrangeBuffPaladinClasses = midrangeBuffPaladin.GroupBy(c => c.Class)
				.OrderByDescending(c => c.Count())
				.Select(p => p.Key)
				.ToList();
			var midrangeBuffPaladinDeck = new Deck(midrangeBuffPaladin, midrangeBuffPaladinClasses, new MidRangeScore());
			deckMap.Add("MidrangeBuffPaladin", midrangeBuffPaladinDeck);

			List<Card> midrangeJadeShaman = Decks.MidrangeJadeShaman;
			var midrangeJadeShamanClasses = midrangeJadeShaman.GroupBy(c => c.Class)
				.OrderByDescending(c => c.Count())
				.Select(p => p.Key)
				.ToList();
			var midrangeJadeShamanDeck = new Deck(midrangeJadeShaman, midrangeJadeShamanClasses, new MidRangeScore());
			deckMap.Add("MidrangeJadeShaman", midrangeJadeShamanDeck);

			List<Card> midrangeSecretHunter = Decks.MidrangeSecretHunter;
			var midrangeSecretHunterClasses = midrangeSecretHunter.GroupBy(c => c.Class)
				.OrderByDescending(c => c.Count())
				.Select(p => p.Key)
				.ToList();
			var midrangeSecretHunterDeck = new Deck(midrangeSecretHunter, midrangeSecretHunterClasses, new MidRangeScore());
			deckMap.Add("MidrangeSecretHunter", midrangeSecretHunterDeck);

			List<Card> miraclePirateRogue = Decks.MiraclePirateRogue;
			var miraclePirateRogueClasses = miraclePirateRogue.GroupBy(c => c.Class)
				.OrderByDescending(c => c.Count())
				.Select(p => p.Key)
				.ToList();
			var miraclePirateRogueDeck = new Deck(miraclePirateRogue, miraclePirateRogueClasses, new AggroScore());
			deckMap.Add("MiraclePirateRogue", miraclePirateRogueDeck);

			List<Card> renoKazakusDragonPriest = Decks.RenoKazakusDragonPriest;
			var renoKazakusDragonPriestClasses = renoKazakusDragonPriest.GroupBy(c => c.Class)
				.OrderByDescending(c => c.Count())
				.Select(p => p.Key)
				.ToList();
			var renoKazakusDragonPriestDeck = new Deck(renoKazakusDragonPriest, renoKazakusDragonPriestClasses, new AggroScore());
			deckMap.Add("RenoKazakusDragonPriest", renoKazakusDragonPriestDeck);

			List<Card> renoKazakusMage = Decks.RenoKazakusMage;
			var renoKazakusMageClasses = renoKazakusMage.GroupBy(c => c.Class)
				.OrderByDescending(c => c.Count())
				.Select(p => p.Key)
				.ToList();
			var renoKazakusMageDeck = new Deck(renoKazakusMage, renoKazakusMageClasses, new ControlScore());
			deckMap.Add("RenoKazakusMage", renoKazakusMageDeck);

			List<Card> zooDiscardWarlock = Decks.ZooDiscardWarlock;
			var zooDiscardWarlockClasses = zooDiscardWarlock.GroupBy(c => c.Class)
				.OrderByDescending(c => c.Count())
				.Select(p => p.Key)
				.ToList();
			var zooDiscardWarlockDeck = new Deck(zooDiscardWarlock, zooDiscardWarlockClasses, new AggroScore());
			deckMap.Add("ZooDiscardWarlock", zooDiscardWarlockDeck);

			List<Card> murlocDruid = Decks.MurlocDruid;
			var murlocDruidClasses = murlocDruid.GroupBy(c => c.Class)
				.OrderByDescending(c => c.Count())
				.Select(p => p.Key)
				.ToList();
			var murlocDruidkDeck = new Deck(murlocDruid, murlocDruidClasses, new AggroScore());
			deckMap.Add("MurlocDruid", murlocDruidkDeck);

			List<Card> cubelock = TopTierDecks.Cubelock();
			var cubelockClasses = cubelock.GroupBy(c => c.Class)
				.OrderByDescending(c => c.Count())
				.Select(p => p.Key)
				.ToList();
			var cubelockDeck = new Deck(cubelock, cubelockClasses, new ControlScore());
			deckMap.Add("Cubelock", cubelockDeck);

			List<Card> murlocPaladin = TopTierDecks.MurlocPaladin();
			var murlocPaladinClasses = murlocPaladin.GroupBy(c => c.Class)
				.OrderByDescending(c => c.Count())
				.Select(p => p.Key)
				.ToList();
			var murlocPaladinDeck = new Deck(murlocPaladin, murlocPaladinClasses, new MidRangeScore());
			deckMap.Add("MurlocPaladin", murlocPaladinDeck);

			List<Card> dudePaladin = TopTierDecks.DudePaladin();
			var dudePaladinClasses = dudePaladin.GroupBy(c => c.Class)
				.OrderByDescending(c => c.Count())
				.Select(p => p.Key)
				.ToList();
			var dudePaladinDeck = new Deck(dudePaladin, dudePaladinClasses, new MidRangeScore());
			deckMap.Add("DudePaladin", dudePaladinDeck);

			List<Card> dragonPriest = TopTierDecks.DragonPriest();
			var dragonPriestClasses = dragonPriest.GroupBy(c => c.Class)
				.OrderByDescending(c => c.Count())
				.Select(p => p.Key)
				.ToList();
			var dragonPriestDeck = new Deck(dragonPriest, dragonPriestClasses, new MidRangeScore());
			deckMap.Add("DragonPriest", dragonPriestDeck);

			List<Card> controlWarlock = TopTierDecks.ControlWarlock();
			var controlWarlockClasses = controlWarlock.GroupBy(c => c.Class)
				.OrderByDescending(c => c.Count())
				.Select(p => p.Key)
				.ToList();
			var controlWarlockDeck = new Deck(controlWarlock, controlWarlockClasses, new ControlScore());
			deckMap.Add("ControlWarlock", controlWarlockDeck);

			return deckMap;
		}

		/// <summary>
		/// Container class for played cards. 
		/// </summary>
		private class PlayedCards : CardSet { }

		/// <summary>
		/// Container class for board cards.
		/// </summary>
		private class BoardCards : PlayedCards { }

		/// <summary>
		/// Container class for predicted cards.
		/// </summary>
		private class PredictedCards : PlayedCards
		{
			/// <summary>
			/// Creates a set of predicted cards.
			/// </summary>
			public PredictedCards()
			{
			}

			/// <summary>
			/// Creates a set of predicted cards using the specified card-value entries.
			/// </summary>
			/// <param name="entries">the specified card-value entries</param>
			public PredictedCards(ICollection<KeyValuePair<Card, double>> entries)
			{
				AddRange(entries);
			}
		}

		/// <summary>
		/// Container class for hand cards.
		/// </summary>
		private class Hand : CardSet { }

		/// <summary>
		/// Container class for decks.
		/// </summary>
		private class Deck : CardSet
		{
			/// <summary>
			/// Returns the scoring function of the deck.
			/// </summary>
			public Score.Score Scoring { get; private set; }

			/// <summary>
			/// The card classes of the deck.
			/// </summary>
			public List<CardClass> Classes { get; private set; }

			/// <summary>
			/// TODO: API
			/// </summary>
			/// <param name="cards">the cards of the deck</param>
			/// <param name="classes">the classes of the deck</param>
			/// <param name="scoring">the scoring function of the deck</param>
			public Deck(List<Card> cards, List<CardClass> classes, Score.Score scoring)
			{
				cards.ForEach(c => Add(c));
				Classes = classes;
				Scoring = scoring;
			}
		}

		/// <summary>
		/// Abstract container class for a collection of card and value pairs. 
		/// </summary>
		private abstract class CardSet
		{
			/// <summary>
			/// The entries, i.e. the pairs of cards and values, of the card set.
			/// </summary>
			public List<KeyValuePair<Card, double>> CardEntries { get; protected set; }

			/// <summary>
			/// The cards of the card set. 
			/// </summary>
			public List<Card> Cards { get { return CardEntries.Select(e => e.Key).ToList(); } }

			/// <summary>
			/// The sum of the values of the entries.
			/// </summary>
			public double Sum { get { return CardEntries.Sum(c => c.Value); } }

			/// <summary>
			/// The number of cards in the set.
			/// </summary>
			public int Count { get { return CardEntries.Count; } }

			/// <summary>
			/// TODO: API
			/// </summary>
			public CardSet()
			{
				CardEntries = new List<KeyValuePair<Card, double>>();
			}

			/// <summary>
			/// Adds a single card to the set.
			/// </summary>
			/// <param name="card">the card to add</param>
			public void Add(Card card)
			{
				Add(card, 0);
			}

			/// <summary>
			/// Adds a card and this value to the set.
			/// </summary>
			/// <param name="card">the card to add</param>
			/// <param name="value">the value of the card</param>
			public void Add(Card card, double value)
			{
				Add(new KeyValuePair<Card, double>(card, value));
			}

			/// <summary>
			/// Adds a pair of card and value to the set.
			/// </summary>
			/// <param name="entry">the entry pair to add</param>
			public void Add(KeyValuePair<Card, double> entry)
			{
				CardEntries.Add(entry);
			}

			/// <summary>
			/// Adds a collection of card and value pairs.
			/// </summary>
			/// <param name="entries">the entries to add</param>
			public void AddRange(ICollection<KeyValuePair<Card, double>> entries)
			{
				CardEntries.AddRange(entries);
			}

			/// <summary>
			/// Removes a single card and it's value from the set.
			/// </summary>
			/// <param name="card">the card to remove</param>
			public void Remove(Card card)
			{
				if (Cards.Contains(card))
				{
					KeyValuePair<Card, double> entry = CardEntries.First(e => e.Key.Equals(card));
					CardEntries.Remove(entry);
				}
			}
		}

		/// <summary>
		/// Container class for prediction objects.
		/// </summary>
		private class Prediction
		{
			/// <summary>
			/// The probability of the prediction.
			/// </summary>
			public double Probability { get; private set; }

			/// <summary>
			/// The hand cards of the prediction.
			/// </summary>
			public Hand Hand { get; private set; }

			/// <summary>
			/// The deck of the prediction.
			/// </summary>
			public Deck Deck { get; protected set; }

			/// <summary>
			/// TODO: API
			/// </summary>
			/// <param name="probability"></param>
			/// <param name="hand"></param>
			/// <param name="deck"></param>
			public Prediction(double probability, Hand hand, Deck deck)
			{
				Probability = probability;
				Hand = hand;
				Deck = deck;
			}
		}
				
	}
}
