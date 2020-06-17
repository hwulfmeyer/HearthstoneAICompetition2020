using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SabberStoneCore.Model.Entities;

namespace SabberStoneBasicAI.AIAgents.PredatorMCTS
{
	/// <summary>
	/// TODO: API
	/// </summary>
	class MCTSScore : Score.Score
	{
		/// <summary>
		/// TODO: API
		/// </summary>
		/// <returns>the scoring value</returns>
		public override int Rate()
		{
			return 0;
		}

		/// <summary>
		/// The function which is used to perform the mulligan. 
		/// </summary>
		/// <returns>the mulligan function</returns>
		public override Func<List<IPlayable>, List<int>> MulliganRule()
		{
			return p => p.Where(t => t.Cost > 3).Select(t => t.Id).ToList();
		}
	}
}
