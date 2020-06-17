using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using System.IO.Compression;

namespace SabberStoneBasicAI.AIAgents.PredatorMCTS
{
	/// <summary>
	/// Deserializes a biram map file.
	/// </summary>
	class BigramMapReader
	{
		/// <summary>
		/// Parses the given file.
		/// </summary>
		/// <param name="pathToFile">The path of the file to parse.</param>
		/// <returns></returns>
		public static BigramMap ParseFile(string pathToFile)
		{
			BigramMap map = new BigramMap();

			string jsonString = "";

			using (FileStream reader = File.OpenRead(pathToFile))
			using (var zip = new GZipStream(reader, CompressionMode.Decompress, true))
			using (var unzip = new StreamReader(zip))
				while (!unzip.EndOfStream)
					jsonString += unzip.ReadLine();

			var json = JObject.Parse(jsonString);
			foreach (KeyValuePair<string, JToken> app in json)
			{
				// ignore cardIds, only parse data of bigramMap
				if (app.Key.Equals("bigramMap"))
				{
					var bigramArray = JArray.Parse(app.Value.ToString());

					foreach (JObject bigramArrayEntry in bigramArray)
					{
						AddObjectToBigramMap(map, bigramArrayEntry);
					}
				}
			}

			map.Sort();
			return map;
		}

		/// <summary>
		/// Adds the given JSON array object to the bigram map.
		/// </summary>
		/// <param name="map">The bigram map.</param>
		/// <param name="bigramArrayEntry">The new bigram array entry.</param>
		private static void AddObjectToBigramMap(BigramMap map, JObject bigramArrayEntry)
		{
			foreach (KeyValuePair<string, JToken> entry in bigramArrayEntry)
			{
				string keyCard = entry.Key;
				var occurrencesArray = JArray.Parse(entry.Value.ToString());
				AddOccurrencesOfKeyCardToMap(map, keyCard, occurrencesArray);
			}
		}

		/// <summary>
		/// Add a occurances array of entries of a card to the bigram map.
		/// </summary>
		/// <param name="map">The bigram map.</param>
		/// <param name="keyCard">The key card.</param>
		/// <param name="occurrencesArray">The list of entris with the occurrences.</param>
		private static void AddOccurrencesOfKeyCardToMap(BigramMap map, string keyCard, JArray occurrencesArray)
		{
			string valCard = "";
			int numOcc = 0;
			foreach (JObject occObject in occurrencesArray)
			{
				foreach (KeyValuePair<string, JToken> occEntry in occObject)
				{
					if (occEntry.Key.Equals("numOcc"))
					{
						numOcc = (int) occEntry.Value;
					}
					else if (occEntry.Key.Equals("cardId"))
					{
						valCard = (string) occEntry.Value;
					}
				}
				map.Add(keyCard, valCard, numOcc);
			}
		}
	}
}
