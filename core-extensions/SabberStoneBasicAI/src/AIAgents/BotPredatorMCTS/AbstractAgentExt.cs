using System;
using System.Collections.Generic;
using System.Diagnostics;
using SabberStoneCore.Tasks.PlayerTasks;
using SabberStoneCore.Enums;
using SabberStoneCore.Model.Entities;
using SabberStoneBasicAI.PartialObservation;

namespace SabberStoneBasicAI.AIAgents.PredatorMCTS
{
	abstract class AbstractAgentExt : AbstractAgent
	{
		/// <summary>
		/// TODO: API
		/// </summary>
		private readonly Random _rnd = new Random();

		/// <summary>
		/// TODO: API
		/// </summary>
		public Score.Score _scoring;

		/// <summary>
		/// The current calculated solutions of the agent. 
		/// </summary>
		private Queue<PlayerTask> _currentSolutions;

		/// <summary>
		/// The stop watch of the agent for measuring the computation time.
		/// </summary>
		private Stopwatch _watch;

		/// <summary>
		/// Returns the stop watch of the agent.
		/// </summary>
		protected Stopwatch Watch
		{
			get
			{
				// lazily instantiate
				if (_watch == null)
				{
					_watch = new Stopwatch();
				}
				return _watch;
			}
		}


		public override void InitializeAgent() { }
		
		public override void InitializeGame()
		{
			_currentSolutions = null;
		}

		public override PlayerTask GetMove(POGame poGame)
		{
			// start stop watch
			if (!Watch.IsRunning)
			{
				Watch.Start();
			}

			// calculate a new bunch of solutions  
			if (_currentSolutions == null)
			{
				_currentSolutions = new Queue<PlayerTask>();
			}
			
			Controller currentPlayer = poGame.CurrentPlayer;
			if (_currentSolutions != null && _currentSolutions.Count < 1)
			{
				List<PlayerTask> solutions = getSolutions(poGame, currentPlayer.Id, _scoring);
				foreach (PlayerTask solution in solutions)
				{
					_currentSolutions.Enqueue(solution);
				}
			}
			PlayerTask result = _currentSolutions.Dequeue();

			if (result.PlayerTaskType == PlayerTaskType.CHOOSE
				&& poGame.CurrentPlayer.Choice == null)
			{
				result = EndTurnTask.Any(currentPlayer);
			}

			// reset watch
			if (result.PlayerTaskType == PlayerTaskType.END_TURN
				|| poGame.State == State.COMPLETE
				|| poGame.State == State.INVALID)
			{
				Watch.Reset();
			}
			
			return result;
		}
		
		public override void FinalizeGame(SabberStoneCore.Model.Game game, Controller controllers) { }

		public override void FinalizeAgent() { }

		protected abstract List<PlayerTask> getSolutions(POGame poGame, int playerID, Score.Score scoring);
		
	}
}
