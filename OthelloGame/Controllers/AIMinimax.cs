using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace OthelloGame.Controllers
{
    /// <summary>
    /// Supporting and interfacing code for implementation of Minimax AI algorithm.
    /// </summary>
    public class AIMinimax : ControllerBase
    {
        public AIMinimax(Game game, int player)
        {
            Game = game;
            Player = player;

            Depth = 8;
            UseThreading = true;

            MoveTrimming = false;

            FullSolvePoint = 8;

            LastTranspositionTable = new Dictionary<string, Minimax.MinimaxDataSet>();

            OppoentMoveData = new List<MoveEvalData>();

            Game.MoveMade += Game_MoveMade;

        }

        /// <summary>
        /// Game instance the AIMinimax controller is for.
        /// </summary>
        public Game Game { get; set; }

        /// <summary>
        /// Player ID the AIMinimax controller is playing for (0 or 1).
        /// </summary>
        public int Player { get; set; }

        /// <summary>
        /// Determines if move trimming is used at the top level to trim moves.
        /// </summary>
        public bool MoveTrimming { get; set; }

        /// <summary>
        /// How many moves should remain after trimming.
        /// </summary>
        public int MoveTrimTo { get; set; }

        /// <summary>
        /// Minimax search depth.
        /// </summary>
        public int Depth { get; set; }

        /// <summary>
        /// When, at most, this number of moves remain the depth will be modified to solve for every possible combination.
        /// This works as towards the end of the game the number of moves that need to be evaluated at deeper depths get smaller rather than expand rapidly.
        /// </summary>
        public int FullSolvePoint { get; set; }

        /// <summary>
        /// Determines if evaluation of each possible top-level move should be threaded.
        /// Not using threading is not supported.
        /// </summary>
        public bool UseThreading { get; set; }

        /// <summary>
        /// The transposition table resulting from the last Minimax search, effectively contains a lookup to depth - 2.
        /// </summary>
        public Dictionary<string, Minimax.MinimaxDataSet> LastTranspositionTable { get; set; }

        /// <summary>
        /// The last Minimax result (used for drawing data).
        /// </summary>
        public Minimax.MinimaxDataSet LastMinimaxResult { get; private set; }

#if DEBUG || DEBUG_STATS_ONLY

        public long LastResultTime { get; private set; }
        public int RealEvaluatedMoves { get; private set; }
        public List<int> UntrimmedMoves { get; set; }
#endif
        /// <summary>
        /// The last best move Index.
        /// </summary>
        public int BestMoveIdx { get; private set; }

        /// <summary>
        /// The last best move Weight.
        /// </summary>
        public int BestMoveWeight { get; private set; }

        /// <summary>
        /// Information about the last moves evaluated.
        /// </summary>
        public Dictionary<int, Minimax.MoveInfo> ValidMovesInfo { get; private set; }

        /// <summary>
        /// Class used to tie-break.
        /// </summary>
        public Tiebreaks.ITiebreak Tiebreak { get; set; }

        /// <summary>
        /// Class used to weight non-terminal board states.
        /// </summary>
        public Weighting.WeightingBase Weighting { get; set; }

        /// <summary>
        /// Class used to weight terminal board states.
        /// </summary>
        public EndgameWeighting.EndgameWeightingBase EndgameWeighting { get; set; }

        /// <summary>
        /// Class used to select a move given data about all valid moves.
        /// </summary>
        public MoveSelectors.IMoveSelector MoveSelector { get; set; }

        /// <summary>
        /// Determines if the AI will automatically make the move it determines to be the best.
        /// If set to true, making the move will have to be triggered by external code.
        /// </summary>
        public bool Pause { get; set; }

        /// <summary>
        /// Resets the internal state of the controller for use in a new game.
        /// </summary>
        public override void Reset()
        {
            LastMinimaxResult = null;
            LastTranspositionTable = null;

            LastTranspositionTable = new Dictionary<string, Minimax.MinimaxDataSet>();

            OppoentMoveData = new List<MoveEvalData>();
        }

        /// <summary>
        /// Called when it's the controller's players turn.
        /// </summary>
        public override void YourMove()
        {
            OnPlayerCalculating(Player);

            PlayFoward();
        }

        /// <summary>
        /// Setup code for playing forward.
        /// </summary>
        public void PlayFoward()
        {
            if (Depth < 0)
                throw new ArgumentOutOfRangeException();

            // Hack to allow for testing the adaptive AI vs. other weighting algorithms.
            if(AlterWeighting != null)
            {
                // Store previous information.
                var tmp_weighting_store = Weighting;
                Weighting = AlterWeighting;
                var old_best_move_idx = BestMoveIdx;
                var old_last_result = LastMinimaxResult;
                var old_prev_results = LastTranspositionTable;

                // Perform Minimax evaluation using the Depth from the other controller.
                MinimaxStart(Game, (Game.PlayerControllers[Game.OtherPlayer(Player)] as AIMinimax).Depth);

                // Store results in Alt stores.
                AltLastMinimaxResult = LastMinimaxResult;

                // Revert changed data.
                Weighting = tmp_weighting_store;
                if (old_last_result != null)
                {
                    ValidMovesInfo = old_last_result.valid_move_w_weights;
                    BestMoveIdx = old_best_move_idx;
                    BestMoveWeight = old_last_result.weight;
                }
                
                LastMinimaxResult = old_last_result;
                LastTranspositionTable = old_prev_results;
            }

            MinimaxStart(Game, Depth);
        }


        // As recommended by Microsoft: http://msdn.microsoft.com/en-us/library/c5kehkcz.aspx
        private Object minimax_thread_lock = new Object();

        /// <summary>
        /// Core method for setting up, starting, and combining the results from minimax evaluation.
        /// </summary>
        /// <param name="game">Game to use as the starting point.</param>
        /// <param name="depth">Depth to explore to.</param>
        public void MinimaxStart(Game game, int depth)
        {
            var minimax = GetMinimaxInstance();

            Minimax.MinimaxDataSet minimax_result;


#if DEBUG || DEBUG_STATS_ONLY
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif

            int scale_to;

            // Number of moves remaining for the game.
            int remaining = (game.Board.Length - game.MoveHistory.Count - 4);

            // Check if we're to the point where we want to completely solve the board, if so modify the depth to do so.
            if (remaining <= FullSolvePoint)
            {
                scale_to = FullSolvePoint + 1;
                depth = scale_to;
            }

            // Copy of game's move list (so we can modify it safely).
            var moves = new List<int>(game.ValidMoves);

            // If we're using move trimming then we optimize the top level move order so as to easily be able to trim moves by getting a subrange of the moves.
            if (MoveTrimming)
            {
                Minimax.TranspositionFetchInfo fetch_info;
                Minimax.MinimaxDataSet saved_entry = Minimax.TranspositionGet(game, LastTranspositionTable, null, out fetch_info);

                if (saved_entry != null)
                {
                    moves = minimax.OptimizeMoveOrder(game, true, saved_entry, fetch_info);
#if DEBUG || DEBUG_STATS_ONLY
                    UntrimmedMoves = moves;
#endif

                }

            }
#if FIX_LATER
            #region Check for transpositions
            // This region deals with looking for transpositions of the current moves available, 
            // and if found, recording them and removing all but one from the moves to be evaluated.
            // This step can see significant performance improvements because, unlike when dealing with a single thread,
            // top level transpositions are never found (as all work is done concurrently).
            // This can waste valuable processor time.

            // The lookup table for the transpositions found below, all ints are board indexes. The key is the one which is evaluated.
            var transpositions_lookup = new Dictionary<int, List<int>>();

            // List of moves that need to be checked still; moves are removed from this list once they no longer need to be checked.
            var moves_to_check = new List<int>(moves);

            #region Transposition Checker
            // This method is used to check if new_i (new index) exists in the moves to check list,
            // and if so performs several actions to setup the correct transposition link.
            Action<int, int> act = (int old_i, int new_i) =>
            {
                int idx;

                if ((idx = moves_to_check.IndexOf(new_i)) != -1)
                {
                    if (!transpositions_lookup.ContainsKey(old_i))
                        transpositions_lookup.Add(old_i, new List<int>());

                    transpositions_lookup[old_i].Add(new_i);

                    moves_to_check.RemoveAt(idx);

                    idx = moves.IndexOf(new_i);

                    if (idx != -1)
                        moves.RemoveAt(idx);
                }
            };
            #endregion

            // Loop to check all relevant moves.
            // Checked moves are removed from the collection, and there can be no valid transpositions with only one move.
            while (moves_to_check.Count > 1)
            {
                var old_index = moves_to_check.ElementAt(0);
                moves_to_check.RemoveAt(0);

                // 180rot transposition index.
                var new_index = game.Board.Length - 1 - old_index;
                act(old_index, new_index);

                // Horizontal flip transposition index.
                var xy = game.GetXY(old_index);
                new_index = game.GetIndex(xy.y, xy.x);
                act(old_index, new_index);

                // Vertical flip transposition index.
                new_index = game.GetIndex(game.BoardSize - 1 - xy.y, game.BoardSize - 1 - xy.x);
                act(old_index, new_index);

            }
            #endregion
#endif
            // Moves are actually trimmed after transpositions are removed, as transpositions are just free trims.
            if (MoveTrimming && MoveTrimTo < moves.Count)
            {
                moves = moves.GetRange(0, MoveTrimTo);
            }

            // It might be good to change this to the number of cores' one has in their computer.
            ThreadPool.SetMinThreads(4, 0);

            // Event collection, used to wait for all minimax evaluations to finish before continuing.
            var events = new ManualResetEvent[moves.Count];

            // The results from the evaluations.
            var results = new Dictionary<int, Minimax.MinimaxDataSet>(moves.Count);

            // The new transposition table; this is shared across all threads.
            var new_perms = new System.Collections.Concurrent.ConcurrentDictionary<string, Minimax.MinimaxDataSet>();


            // Iterate over all moves, queuing up a threaded Minimax evaluation for each one.
            for (var i = 0; i < moves.Count; i++)
            {
                // Get the new values for evaluation.
                Game new_game;
                int new_depth;
                bool new_maximizing;

                minimax.LoopPre(moves[i], game, depth, out new_game, out new_depth, out new_maximizing);

                // Create a new reset event for syncing.
                var resetEvent = new ManualResetEvent(false);

                // Queue up the minimax evaluation.
                ThreadPool.QueueUserWorkItem(result_idx =>
                {
                    var res = minimax.Eval(new_game, new_depth, new_maximizing, ref new_perms, LastTranspositionTable);

                    lock (minimax_thread_lock)
                    {
                        // Add the result to the results collection, with the key being the board index of the move.
                        results.Add((int)result_idx, res);
                    }

                    // Report that we're done with this thread's work.
                    resetEvent.Set();
                }, moves[i]); // We pass the move's board index in as to correctly set the result's key.

                // Add our event to the events array, so we can call wait on each of them.
                events[i] = resetEvent;
            }

            // Wait for all minimax evaluations to finish.
            for (var i = 0; i < events.Length; i++)
                events[i].WaitOne();

            // New data set which is used to merge back the data from the threads.
            var data_set = new Minimax.MinimaxDataSet();
            var weights = new Dictionary<int, Minimax.MoveInfo>(game.ValidMoves.Count);

            // Iterate over the results from each thread (with one thread being one possible move).
            foreach (var item in results)
            {
                var move_info = new Minimax.MoveInfo(item.Value.weight);
                weights.Add(item.Key, move_info);

#if FIX_LATER
                // Check if any transpositions exist, and if so expand out the results for each of the transpositions.
                List<int> transpoitions;
                if (transpositions_lookup.TryGetValue(item.Key, out transpoitions))
                {
                    foreach (var iitem in transpoitions)
                    {
                        weights.Add(iitem, move_info);
                    }
                }
#endif
#if DEBUG || DEBUG_STATS_ONLY
                data_set.found_nodes += item.Value.found_nodes;
                data_set.endpoints += item.Value.endpoints;
                data_set.nodes += item.Value.nodes;
#endif

#if DEBUG
                data_set.subsets.Add(item.Value);
#endif
            }


            int best_weight;

            // Get the best move and best weight.
            var moves_by_weight = Minimax.MovesByWeight(weights);
            best_weight = MoveSelector.Select(moves_by_weight, game);
            BestMoveIdx = Tiebreak.Do(moves_by_weight[best_weight], game);

            data_set.weight = best_weight;
            data_set.valid_move_w_weights = weights;
            ValidMovesInfo = weights;
            BestMoveWeight = best_weight;

            data_set.depth = depth;

            minimax_result = data_set;

            // Convert the resulting transposition table from a concurrent dictionary to a normal dictionary (it doesn't need to be concurrent anymore as it's only being read).
            LastTranspositionTable = new_perms.ToDictionary(kp => kp.Key, kp => kp.Value);

            // Perform finalization with the combined minimax result.
            OnPlayerCalculated(Player);
            DoMove();

#if DEBUG || DEBUG_STATS_ONLY
            sw.Stop();
            LastResultTime = sw.ElapsedMilliseconds;

            
            RealEvaluatedMoves = moves.Count;
#endif
            LastMinimaxResult = minimax_result;
        }

        /// <summary>
        /// Either performs the best move, or raises a ready to move event if Pause is true.
        /// </summary>
        public void DoMove()
        {
            if (Pause)
            {
                OnPlayerReadyToMove(Player);
            }
            else
            {
                if (!Game.Move(BestMoveIdx))
                {
                    throw new Exceptions.LogicError("AI Made an Invalid Move");
                }
            }
        }

        /// <summary>
        /// Gets a Minimax instance initialized to the settings set on this instance.
        /// </summary>
        /// <returns></returns>
        private Minimax GetMinimaxInstance()
        {
            var minimax = new Minimax(Player, LastTranspositionTable);
            minimax.Tiebreak = this.Tiebreak.Do;
            minimax.Weighting = this.Weighting.Do;
            minimax.EndgameWeighting = this.EndgameWeighting.Do;

            return minimax;
        }

        public override void DrawBoard(GameRender renderer)
        {
            foreach(var item in ValidMovesInfo)
            {
                var index = item.Key;
                var weight = item.Value.weight;
                var tile = renderer.GameTiles[index];

                if(BestMoveWeight == weight) 
                {
                    if (BestMoveIdx == index)
                        tile.HighlightMode = GameTile.HighlightModes.ChosenMove;
                    else
                        tile.HighlightMode = GameTile.HighlightModes.BestMove;
                }

                tile.HeaderText = weight.ToString();

                if (AltLastMinimaxResult != null && AltLastMinimaxResult.valid_move_w_weights.ContainsKey(index))
                    tile.HeaderText += " | " + AltLastMinimaxResult.valid_move_w_weights[index].weight;

#if DEBUG || DEBUG_STATS_ONLY
                if(UntrimmedMoves != null)
                {
                    if (UntrimmedMoves.IndexOf(index) != -1)
                        tile.HeaderText += " (Untrimmed)";
                    else
                        tile.HeaderText += " (Trim Target)";
                }
#endif
            }

#if DEBUG || DEBUG_STATS_ONLY

            if (LastMinimaxResult != null)
                renderer.DebugLabel.Content = InfoboxPreStr + " || Handycap: " + Handycap + " || Time: " + LastResultTime + "ms Endpoints: " + LastMinimaxResult.endpoints + " Nodes: " + LastMinimaxResult.nodes + " Found: " + LastMinimaxResult.found_nodes;
            else
                renderer.DebugLabel.Content = "Time: " + LastResultTime + "ms";

#endif
        }

        public override string UniqueIdentString()
        {
            return
                "AIMinimax|0.30|C#||" +
                "MoveTrimming:" + MoveTrimming + ";" +
                "MoveTrimTo:" + MoveTrimTo + ";" +
                "Depth:" + Depth + ";" +
                "FullSolvePoint:" + FullSolvePoint + ";" +
                "UseThreading:" + UseThreading + ";" +
                "Tiebreak:" + Tiebreak.GetType().Name + ";" +
                "Weighting:" + Weighting.GetType().Name + ";" +
                "AlterWeighting:" + (AlterWeighting == null ? "Null" : AlterWeighting.GetType().Name) + ";" +
                "EndgameWeighting:" + EndgameWeighting.GetType().Name + ";" +
                "MoveSelector:" + MoveSelector.GetType().Name + ";";
        }

        #region Events

        protected virtual void OnPlayerCalculating(int player)
        {
            EventHandler<Events.PlayerCalculatingEventArgs> handler = PlayerCalculating;
            if (handler != null)
            {
                handler(this, new Events.PlayerCalculatingEventArgs(player));
            }
        }

        protected virtual void OnPlayerCalculated(int player)
        {
            EventHandler<Events.PlayerCalculatedEventArgs> handler = PlayerCalculated;
            if (handler != null)
            {
                handler(this, new Events.PlayerCalculatedEventArgs(player));
            }
        }

        protected virtual void OnPlayerReadyToMove(int player)
        {
            EventHandler<Events.PlayerReadyToMoveEventArgs> handler = PlayerReadyToMove;
            if (handler != null)
            {
                handler(this, new Events.PlayerReadyToMoveEventArgs(player));
            }
        }

        /// <summary>
        /// Raised before Minimax calculations begin.
        /// </summary>
        public event EventHandler<Events.PlayerCalculatingEventArgs> PlayerCalculating;

        /// <summary>
        /// Raised after Minimax calculations have finished and the player is ready to move, but before the actual move has taken place.
        /// </summary>
        public event EventHandler<Events.PlayerCalculatedEventArgs> PlayerCalculated;

        /// <summary>
        /// Raised after PlayerCalculated event if Pause is true.
        /// </summary>
        public event EventHandler<Events.PlayerReadyToMoveEventArgs> PlayerReadyToMove;

        #endregion


        #region Data-gathering code for adaptive AI

        public Weighting.WeightingBase AlterWeighting { get; set; }
        public Minimax.MinimaxDataSet AltLastMinimaxResult { get; private set; }

        public List<MoveEvalData> OppoentMoveData { get; set; }
        public int Handycap { get; set; }
        public string InfoboxPreStr { get; set; }

        /// <summary>
        /// Records various data points about the other player's moves for use with the adaptive AI code.
        /// </summary>
        void Game_MoveMade(object sender, Events.MoveBaseEventArgs e)
        {
            if (e.valid == false)
                return;

            if (e.player == Player)
                return;

            var game = (Game)sender;
            var other_player = game.OtherPlayer(Player);

            var opponent_ai = game.PlayerControllers[other_player] as AIMinimax;
            var last_result = opponent_ai.LastMinimaxResult;
            if (opponent_ai.Weighting.GetType().Equals(Weighting.GetType()) == false)
            {
                last_result = opponent_ai.AltLastMinimaxResult;
            }

            var res = opponent_ai.LastMinimaxResult;

            if (opponent_ai.AltLastMinimaxResult != null)
                res = opponent_ai.AltLastMinimaxResult;

            if (res != null && res.valid_move_w_weights.ContainsKey(game.MoveHistory.Peek()))
            {
                var data = new MoveEvalData();

                var oppoent_last_move_weight = res.valid_move_w_weights[game.MoveHistory.Peek()].weight;
                var oppoent_best_available_move_weight = res.weight;

                data.best_move_weight = oppoent_best_available_move_weight;
                data.chosen_move_weight = oppoent_last_move_weight;

                data.chosen_dev_weight = Math.Abs(oppoent_best_available_move_weight - oppoent_last_move_weight);

                double count_picked = 0, count_worse = 0, count_total = res.valid_move_w_weights.Count;

                foreach (var item in res.valid_move_w_weights)
                {
                    if (item.Value.weight == oppoent_last_move_weight)
                        count_picked += 1;
                    else if (item.Value.weight < oppoent_last_move_weight)
                        count_worse += 1;
                }

                data.prob_picked = count_picked / count_total;
                data.prob_worse = count_worse / count_total;
                data.prob_better = (count_total - (count_picked + count_worse)) / count_total;

                OppoentMoveData.Add(data);
            }
        }

        public struct MoveEvalData
        {
            public int best_move_weight;
            public int chosen_move_weight;
            public int chosen_dev_weight;
            public double prob_picked;
            public double prob_worse;
            public double prob_better;
        }

        #endregion
    }
}
