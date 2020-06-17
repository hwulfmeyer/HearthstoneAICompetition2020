using Newtonsoft.Json;
using SabberStoneCore.Enums;
using SabberStoneCore.Model.Entities;
using SabberStoneCore.Model.Zones;
using SabberStoneCore.Tasks;
using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using SabberStoneCore.Model;
using System.Diagnostics;


namespace SabberStoneCoreAi.Agent
{
	class MyAgent : AbstractAgent
	{
		private Random Rnd = new Random();
		private Queue<PlayerTask> ListPlayerTasksToDo;


		public override void InitializeGame()
		{
		}


		public override void InitializeAgent()
		{
			Rnd = new Random();
			ListPlayerTasksToDo = new Queue<PlayerTask>();
		}


		public override void FinalizeAgent()
		{
		}


		public override void FinalizeGame()
		{
		}


		public override void FinalizeGame(PlayState playState)
		{
			Console.WriteLine(playState==PlayState.WON?1:0);
			ListPlayerTasksToDo = new Queue<PlayerTask>();
		}


		public override PlayerTask GetMove(POGame.POGame poGame)
		{
			//return GetMoveRandom(poGame);
			return GetMoveSearchTree(poGame, 2);
		}


		public PlayerTask GetMoveRandom(POGame.POGame poGame)
		{
			List<PlayerTask> options = poGame.CurrentPlayer.Options();
			return options[Rnd.Next(options.Count)];
		}


		public PlayerTask GetMoveSearchTree(POGame.POGame poGame, int depth)
		{
			if (poGame.CurrentPlayer.Options().Count == 1)
			{
				return poGame.CurrentPlayer.Options().First();
			}
			else if(poGame.CurrentPlayer.Options().Count == 2)
			{
				return poGame.CurrentPlayer.Options()[1];
			}
			else
			{
				if (ListPlayerTasksToDo.Count == 0)
				{
					ListPlayerTasksToDo = new Queue<PlayerTask>();
					var root = new NodeGameState(poGame);
					root.IDDFS(depth);
					ListPlayerTasksToDo = root.GetPlayerTasks();
				}
				return ListPlayerTasksToDo.Dequeue();
			}
		}
	}


	class NodeGameState
	{
		public NodeGameState prt;	// parent
		public List<NodeGameState> chdr; // children
		public POGame.POGame game; //gamestate of the note
		int PlayerId;
		public PlayerTask task;
		public bool isEnemyNode;
		public bool WasExpanded;
		public float score;

		public NodeGameState(POGame.POGame poGame, PlayerTask task = null,  NodeGameState parent = null)
		{
			chdr = new List<NodeGameState>();
			prt = parent;
			game = poGame.getCopy();
			this.task = task;
			WasExpanded = false;
			PlayerId = game.CurrentPlayer.PlayerId;
			if (parent == null) isEnemyNode = false; // root is always MyPlayer
			else if(PlayerId != parent.PlayerId)
			{
				isEnemyNode = !parent.isEnemyNode;
			}
			else
			{
				isEnemyNode = parent.isEnemyNode;
			}
		}


		public bool IsRoot
		{
			get { return prt == null; }
		}


		public bool IsLeaf
		{
			get { return chdr.Count == 0; }
		}


		/// <summary>
		/// Iterative deepening depth-first (recursive depth-limited DFS)
		/// </summary>
		public void IDDFS(int maxDepth)
		{
			for (int i=0;i< maxDepth; i++)
			{
				DLS();
			}
		}


		private void DLS()
		{
			if (!WasExpanded)
			{
				//fill HandZone of enemy if it is enemys turn
				if (isEnemyNode)
				{
					int cardsToAdd = game.CurrentPlayer.HandZone.Count;
					//Added while loop because apparently the foreach only removes every 2nd card? :/
					while (game.CurrentPlayer.HandZone.Count > 0)
					{
						foreach (IPlayable x in game.CurrentPlayer.HandZone)
						{
							game.CurrentPlayer.HandZone.Remove(x);
						}
					}
					IEnumerable<Card> cards = game.CurrentPlayer.DeckZone.Controller.Standard;

					while (!game.CurrentPlayer.HandZone.IsFull && cardsToAdd > 0)
					{
						Card card = Util.Choose<Card>(cards.ToList());

						// don't add cards that have already reached max occurence in hand + graveyard + boardzone of current Player
						if (game.CurrentPlayer.HandZone.Count(c => c.Card == card) +
							game.CurrentPlayer.GraveyardZone.Count(c => c.Card == card) +
							game.CurrentPlayer.BoardZone.Count(c => c.Card == card) >= card.MaxAllowedInDeck)
							continue;

						IPlayable entity = Entity.FromCard(game.CurrentPlayer.DeckZone.Controller, card);
						game.CurrentPlayer.HandZone.Add(entity);

						cardsToAdd--;
					}
				}

				Expand();
			}
			else
			{
				foreach (NodeGameState child in chdr)
				{
					child.DLS();
				}
			}

		}


		/// <summary>
		/// MinMax Search
		/// </summary>
		/// <returns></returns>
		internal Queue<PlayerTask> GetPlayerTasks()
		{
			score = MiniMax();
			return GetPlayerTasksMinimax();
		}


		/// <summary>
		/// Minimax algorithm
		/// </summary>
		private float MiniMax()
		{
			if (IsLeaf)
			{
				return SelectionPolicy();
			}
			else
			{
				foreach (NodeGameState child in chdr)
				{
					child.score = child.MiniMax();
				}
				
				if (isEnemyNode)
				{
					//Minimize
					return chdr.Min(chdr => chdr.score);
				}
				else
				{
					//Maximize
					return chdr.Max(chdr => chdr.score);
				}
			}
		}


		private Queue<PlayerTask> GetPlayerTasksMinimax()
		{
			var que = new Queue<PlayerTask>();
			if (!IsRoot) que.Enqueue(task);
			if (!IsLeaf)
			{
				IOrderedEnumerable<NodeGameState> orderedChildren = chdr.OrderByDescending(p => p.score);
				//Min if true, Max if false
				NodeGameState nextNode = isEnemyNode ? orderedChildren.Last() : orderedChildren.First();
				Queue<PlayerTask> queueTail = nextNode.GetPlayerTasksMinimax();
				while (queueTail.Count > 0)
				{
					que.Enqueue(queueTail.Dequeue());
				}
			}

			return que;
		}


		private float SelectionPolicy()
		{
			var score = new Score.ControlScore
			{
				Controller = isEnemyNode ? game.CurrentOpponent : game.CurrentPlayer
			};
			return score.Rate();
		}


		private void Expand()
		{
			WasExpanded = true;
			Dictionary<PlayerTask, POGame.POGame> dict = game.Simulate(game.CurrentPlayer.Options());
			//set game to null so that it gets cleaned up faster by the gc to reduce needed memory

			foreach (KeyValuePair<PlayerTask, POGame.POGame> item in dict)
			{
				//poGame is null if exception occurs
				if (item.Value != null)
				{
					chdr.Add(new NodeGameState(item.Value, item.Key, this));
				}
			}
		}
	}


	public class MyScore : Score.Score
	{
		public override int Rate()
		{
			if (OpHeroHp < 1)
				return Int32.MaxValue;

			if (HeroHp < 1)
				return Int32.MinValue;

			int result = 0;

			if (OpMinionTotHealthTaunt > 0)
				result += OpMinionTotHealthTaunt * -100;

			if (OpBoardZone.Count == 0 && BoardZone.Count > 0)
				result += 1000;

			if (OpMinionTotHealthTaunt > 0)
				result += MinionTotHealthTaunt * -500;

			result += (BoardZone.Count - OpBoardZone.Count) * 10;

			result += MinionTotAtk * 10;

			result += (HeroHp - OpHeroHp) * 10;

			result += (MinionTotHealth - OpMinionTotHealth) * 10;

			result += (MinionTotAtk - OpMinionTotAtk) * 10;

			result += (MinionTotHealthTaunt - OpMinionTotHealthTaunt) * 25;

			result += (BoardZone.Count - OpBoardZone.Count) * 5;

			//result += Controller.RemainingMana * 5;

			return result;
		}

		public override Func<List<IPlayable>, List<int>> MulliganRule()
		{
			return p => p.Where(t => t.Cost > 3).Select(t => t.Id).ToList();
		}
	}




}
