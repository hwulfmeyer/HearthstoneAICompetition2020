using System.Collections.Generic;
using System.Linq;
using SabberStoneCore.Enums;
using SabberStoneCore.Model.Entities;
using System.Text;
using SabberStoneCore.Tasks.PlayerTasks;
using SabberStoneBasicAI.PartialObservation;

namespace SabberStoneBasicAI.AIAgents.PredatorMCTS
{
	class POOptionNode
	{
		private readonly POOptionNode _parent;

		protected POGame _game;

		protected int _playerId;

		public PlayerTask PlayerTask { get; }

		public string Hash;

		private int _gameState = 0;
		public bool IsRunning => _gameState == 0;

		private int _endTurn = 0;

		public bool IsEndTurn => _endTurn > 0;

		public bool IsWon => _gameState > 0;

		public bool IsRoot => PlayerTask == null;

		public int Score { get; private set; } = 0;

		public Score.Score Scoring { get; private set; }

		protected bool _isOpponentTurn = false;

		public POOptionNode(POOptionNode parent, POGame game, int playerId, PlayerTask playerTask, Score.Score scoring)
		{
			_parent = parent;
			_game = game.getCopy(); // create clone
			_playerId = playerId;
			PlayerTask = playerTask;
			Scoring = scoring;

			if (!IsRoot)
				Execute();
		}

		public void Switch()
		{
			_isOpponentTurn = !_isOpponentTurn;

			//_playerId = _game.ControllerById(_playerId).Opponent.Id;
			var controller = (_game.CurrentPlayer.Id == _playerId) ? _game.CurrentPlayer : _game.CurrentOpponent;
			_playerId = controller.Opponent.Id;
		}

		public void Execute()
		{
			//_game.Process(PlayerTask);
			// workaround for simulating one task 
			Dictionary<PlayerTask, POGame> dic = _game.Simulate(new List<PlayerTask>() { PlayerTask });
			_game = dic[PlayerTask];

			//Controller controller = _game.ControllerById(_playerId);
			Controller controller = (_game.CurrentPlayer.Id == _playerId) ? _game.CurrentPlayer : _game.CurrentOpponent;

			_gameState = _game.State == State.RUNNING ? 0 : 1;

			_endTurn = _game.CurrentPlayer.Id != _playerId ? 1 : 0;

			// TODO: maybe asked if this is legit?
			// TODO: check content of 'Hash'
			//Hash = _game.Hash(GameTag.LAST_CARD_PLAYED, GameTag.ENTITY_ID);
			Hash = GetHashCode().ToString();

			// scoring every state
			Scoring.Controller = controller;
			Score = Scoring.Rate();

		}

		public void PlayerTasks(ref List<PlayerTask> list)
		{
			if (_parent == null)
				return;

			_parent.PlayerTasks(ref list);
			list.Add(PlayerTask);
		}

		public void Options(ref Dictionary<string, POOptionNode> optionNodes)
		{
			//List<PlayerTask> options = _game.ControllerById(_playerId).Options(!_isOpponentTurn);
			Controller controller = (_game.CurrentPlayer.Id == _playerId) ? _game.CurrentPlayer : _game.CurrentOpponent;
			List<PlayerTask> options = controller.Options(!_isOpponentTurn);

			foreach (PlayerTask option in options)
			{
				var optionNode = new POOptionNode(this, _game, _playerId, option, Scoring);
				if (!optionNodes.ContainsKey(optionNode.Hash))
				{
					optionNodes.Add(optionNode.Hash, optionNode);
				}
			}
		}

		public static List<POOptionNode> GetSolutions(POGame game, int playerId, Score.Score scoring, int maxDepth, int maxWidth)
		{
			var depthNodes = new Dictionary<string, POOptionNode> { ["root"] = new POOptionNode(null, game, playerId, null, scoring) };
			var endTurnNodes = new List<POOptionNode>();
			for (int i = 0; depthNodes.Count > 0 && i < maxDepth; i++)
			{
				var nextDepthNodes = new Dictionary<string, POOptionNode>();
				foreach (POOptionNode option in depthNodes.Values)
				{
					option.Options(ref nextDepthNodes);
				}

				endTurnNodes.AddRange(nextDepthNodes.Values.Where(p => p.IsEndTurn || !p.IsRunning));

				depthNodes = nextDepthNodes
					.Where(p => !p.Value.IsEndTurn && p.Value.IsRunning)
					.OrderByDescending(p => p.Value.Score)
					.Take(maxWidth)
					.ToDictionary(p => p.Key, p => p.Value);

				//Console.WriteLine($"Depth: {i + 1} --> {depthNodes.Count}/{nextDepthNodes.Count} options! [SOLUTIONS:{endTurnNodes.Count}]");
			}
			return endTurnNodes;
		}

	}
}
