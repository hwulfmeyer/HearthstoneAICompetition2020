using System;
using System.Collections.Generic;
using System.Linq;

using SabberStoneCore.Enums;
using SabberStoneCore.Model.Entities;
using SabberStoneCore.Tasks.PlayerTasks;
using SabberStoneBasicAI.PartialObservation;

namespace SabberStoneBasicAI.AIAgents.PredatorMCTS
{

    class MCTSNode
    {
		//private int id;

		private int _playerId;

		private double _score;

		//private IScore _scoring;
		private List<ScoreExt> _scorings;

		private MCTSNode _parent;

		private List<MCTSNode> _children;

		//private List<PlayerTask> _tasks;

		private PlayerTask _task;
		
		private POGame _game;

		private POGame _gameCopy;

		private int _gameState;

		private int _endTurn;

		public int PlayerId
		{
			get
			{
				return _playerId;
			}
		}

		public double VisitCount
		{
			get; set;
		}

		public double TotalScore
		{
			get; set;
		}

		public double Score
		{
			get
			{
				return _score;
			}
		}

		//public IScore Scoring
		//{
		//	get
		//	{
		//		return _scoring;
		//	}
		//}
		public List<ScoreExt> Scorings
		{
			get
			{
				return _scorings;
			}
		}

		public bool IsLeaf
		{
			get
			{
				return (_children == null || (_children.Count < 1));
			}
		}

		public MCTSNode Parent
		{
			get
			{
				return _parent;
			}
		}

		public List<MCTSNode> Children
		{
			get
			{
				return _children;
			}
			set
			{
				_children = value;
			}
		}

		public POGame Game
		{
			get
			{
				return _game;
			}

			private set
			{
				//if (value != null)
				//{
				//	_gameCopy = value;
				//} else
				//{
				//	Console.WriteLine("Test");
				//}
				_game = value;
			}
		}

		public SabberStoneCore.Model.Entities.Controller PlayerController
		{
			get
			{
				if (Game != null)
				{
					return (Game.CurrentPlayer.Id == _playerId) ?
						Game.CurrentPlayer : Game.CurrentOpponent;
				}
				return null;
			}
		}

		public bool IsEndTurn
		{
			get
			{
				return (_endTurn > 0 || (Task != null && Task.PlayerTaskType == PlayerTaskType.END_TURN));
			}
		}

		public bool IsRunning
		{
			get
			{
				return (_gameState == 0);
			}
		}

		public bool IsWon
		{
			get
			{
				return (_gameState > 0);
			}
		}

		public bool IsLost
		{
			get
			{
				return (_gameState < 0);
			}
		}

		public List<PlayerTask> Tasks
		{
			get
			{
				if (PlayerController != null)
				{
					return PlayerController.Options();
				}
				return new List<PlayerTask>();
			}
		}

		public PlayerTask Task
		{
			get
			{
				return _task;
			}
		}

		//public MCTSNode(POGame game, PlayerTask task, MCTSNode parent)
		//	: this(parent.PlayerId, parent.Scoring, game, task, parent) { }
		public MCTSNode(POGame game, PlayerTask task, MCTSNode parent)
			: this(parent.PlayerId, parent.Scorings, game, task, parent) { }

		//public MCTSNode(int playerId, IScore scoring, POGame game, PlayerTask task, MCTSNode parent)
		public MCTSNode(int playerId, List<ScoreExt> scorings, POGame game, PlayerTask task, MCTSNode parent)
		{
			_parent = parent;
			//_scoring = scoring;
			_scorings = scorings;
			_playerId = playerId;
			//_game = game.Clone();
			_game = game.getCopy();
			_task = task;

			VisitCount = 1;
			
			if (Task != null)
			{
				//Game.Process(Task);
				Dictionary<PlayerTask, POGame> dir = Game.Simulate(new List<PlayerTask> { Task });
				POGame newGame = dir[Task];

				Game = newGame;
				// simulation has failed, maybe reduce score? 
				if (Game == null)
				{
					//_gameState = _endTurn = 1;
					_endTurn = 1;
				}
				else
				{
					//if (Game.CurrentPlayer.Choice != null)
					//{
					//	Console.WriteLine("Choices are null");
					//}
					
					_gameState = Game.State == State.RUNNING ? 0
						: (PlayerController.PlayState == PlayState.WON ? 1 : -1);
					_endTurn = Game.CurrentPlayer.Id != _playerId ? 1 : 0;

					//Scoring.Controller = PlayerController;
					//_score = Scoring.Rate();
					foreach (ScoreExt scoring in Scorings)
					{
						scoring.Controller = PlayerController;
						_score += scoring.Value * scoring.Rate();
					}
					_score /= Scorings.Count;
					TotalScore += _score;
				}
			}
		}

		public List<PlayerTask> GetSolution()
		{
			var solutions = new List<PlayerTask>();
			MCTSNode bestChild = this;
			while (!bestChild.IsEndTurn && !bestChild.IsLeaf)
			{
				solutions.Add(bestChild.Task);
				bestChild = bestChild.Children.OrderByDescending(c => c.TotalScore).First();
			}

			if (bestChild.IsEndTurn || bestChild.IsLeaf)
			{
				solutions.Add(bestChild.Task);
				return solutions;
			}

			return solutions;
		}

		public int ParentCount()
		{
			int count = 0;
			MCTSNode child = this;
			while (child.Parent != null)
			{
				child = child.Parent;
				count++;
			}
			return count;
		}

		public class ScoreExt : Score.Score
		{
			private Score.Score _score;

			public double Value
			{
				get; private set;
			}

			public new Controller Controller
			{
				get
				{
					return _score.Controller;
				}

				set
				{
					_score.Controller = value;
				}

			}

			public ScoreExt(double value, Score.Score score)
			{
				Value = value;
				_score = score;
			}

			public override Func<List<IPlayable>, List<int>> MulliganRule()
			{
				return _score.MulliganRule();
			}

			public override int Rate()
			{
				return _score.Rate();
			}
		}

	}
}
