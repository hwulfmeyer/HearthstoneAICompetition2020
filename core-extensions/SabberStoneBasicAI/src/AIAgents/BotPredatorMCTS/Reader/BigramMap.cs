using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SabberStoneBasicAI.AIAgents.PredatorMCTS
{
	/// <summary>
	/// The data stucture which contains the bigram information with occurrences of all played cards.
	/// </summary>
	class BigramMap
	{
		/// <summary>
		/// POCO for a single card with the number of occurrences.
		/// </summary>
		public class CardOcc
		{
			/// <summary>
			/// The card id.
			/// </summary>
			public string cardId;
			/// <summary>
			/// The number of occurrences of this card.
			/// </summary>
			public int numOcc;

			/// <summary>
			/// Contructor.
			/// </summary>
			/// <param name="cardId">The card id.</param>
			/// <param name="numOcc">The number of occurrences of this card.</param>
			public CardOcc(string cardId, int numOcc)
			{
				this.cardId = cardId;
				this.numOcc = numOcc;
			}
		}

		/// <summary>
		/// The main data structure is a dictionary which maps a given card (id) to a list of co-occurrences.
		/// </summary>
		private ConcurrentDictionary<string, List<CardOcc>> dic = new ConcurrentDictionary<string, List<CardOcc>>();

		/// <summary>
		/// Adds to a given key card athe co-occured card with number of occurrences.
		/// use this method for deserialization purpose.
		/// </summary>
		/// <param name="keyCard">The key card.</param>
		/// <param name="valCard">The co-occured card with the key card.</param>
		/// <param name="numOcc">The number of occurrences.</param>
		public void Add(string keyCard, string valCard, int numOcc)
		{
			var newCardOcc = new CardOcc(valCard, numOcc);

			// key alreasy exists => add a new entry to the occurrences list.
			if (dic.ContainsKey(keyCard))
			{
				List<CardOcc> listOcc = dic[keyCard];
				listOcc.Add(newCardOcc);
			}
			else // otherwise create new entry with key card and a new occurences list.
			{
				var newListOcc = new List<CardOcc>
				{
					newCardOcc
				};
				dic.TryAdd(keyCard, newListOcc);
			}
		}

		/// <summary>
		/// Sorts the occurrences lists in map by number of occurrences in descending order.
		/// </summary>
		public void Sort()
		{
			foreach (List<CardOcc> value in dic.Values)
			{
				// inverse sorting => occurrences sorted in descending order
				value.Sort((a, b) => b.numOcc - a.numOcc);
			}
		}

		/// <summary>
		/// Returns the occurrences list of given key card.
		/// If key card does not exists or an exception an empty list will be returned.
		/// </summary>
		/// <param name="keyCard">The key card.</param>
		/// <returns>The occurrences list of given key card.</returns>
		public List<CardOcc> GetOccurrencesList(string keyCard)
		{
			try
			{
				return dic[keyCard];
			}
			catch (Exception e)
			{
				return new List<CardOcc>();
			}
		}
	}
}
