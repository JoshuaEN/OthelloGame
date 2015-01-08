using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OthelloGame;
using System.Collections.Concurrent;

namespace OthelloGame
{
    /// <summary>
    /// Core Minimax code.
    /// </summary>
    public class Minimax
    {
        /// <summary>
        /// Creates a new Minimax instance for use with evaluating a single move.
        /// </summary>
        /// <param name="player">The player who the Minimax instance is playing for.</param>
        /// <param name="prevous_permutations">Previous transposition table to reference, if any.</param>
        public Minimax(int player, Dictionary<string, MinimaxDataSet> prevous_permutations)
        {
            Player = player;
            ReferenceTranspositionTable = prevous_permutations;
        }

        /// <summary>
        /// The player who the Minimax instance is playing for.
        /// </summary>
        public int Player { get; private set; }

        /// <summary>
        /// Tiebreak method.
        /// </summary>
        public Func<Dictionary<int, MoveInfo>, Game, int> Tiebreak { get; set; }

        /// <summary>
        /// Weighting method for non-terminal states.
        /// </summary>
        public Func<Game, int, int> Weighting { get; set; }

        /// <summary>
        /// Weighting method for terminal states.
        /// </summary>
        public Func<Game, int, int> EndgameWeighting { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, MinimaxDataSet> ReferenceTranspositionTable { get; set; }

        /// <summary>
        /// Internal tiebreak used to return best move index, as required.
        /// </summary>
        private static readonly Func<Dictionary<int, MoveInfo>, Game, int> moveOrderingTiebreak = new Tiebreaks.TileWeight().Do;

        /// <summary>
        /// Core function to perform Minimax lookup.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="depth"></param>
        /// <param name="maximizing"></param>
        /// <param name="alpha"></param>
        /// <param name="beta"></param>
        /// <param name="permutations"></param>
        /// <param name="prev_permutations"></param>
        /// <returns></returns>
        public MinimaxDataSet Eval(Game game, int depth, bool maximizing, int alpha, int beta, ref ConcurrentDictionary<string, MinimaxDataSet> permutations, Dictionary<string, MinimaxDataSet> prev_permutations)
        {
            // If it's the end of the game, OR the search depth limit has been reached, return a weight.
            if (depth < 1 || game.Finished)
            {
                var endpoint_data_set = new MinimaxDataSet();
                endpoint_data_set.weight = GetWeight(game);

#if DEBUG || DEBUG_STATS_ONLY
                endpoint_data_set.endpoints += 1;
#endif
#if DEBUG
                endpoint_data_set.endpoint = true;
                endpoint_data_set.maximizing = maximizing;
                endpoint_data_set.boardString = game.BoardPrettyString();
#endif

                return endpoint_data_set;
            }

            // Copy the game's list of valid moves so we can modify it.
            var ordered_moves = new List<int>(game.ValidMoves);
            var game_copy = game;

            #region Check for saved entry in transposition table

            TranspositionFetchInfo transpoition_info;
            MinimaxDataSet saved_entry = Minimax.TranspositionGet(game, permutations, prev_permutations, out transpoition_info);

            if (saved_entry != null)
            {
                // If the saved entry has an equal, or greater, level of information than further Minimax
                // evaluation of the current board state, we simply return the saved entry.
                if (saved_entry.depth >= depth)
                {
                    var recorded_data_set = (MinimaxDataSet)saved_entry.Clone();

#if DEBUG
                    recorded_data_set.via_lookup = true;
                    recorded_data_set.endpoint = true;
                    recorded_data_set.stopped = true;
                    recorded_data_set.subsets = new List<MinimaxDataSet>();
                    recorded_data_set.boardString = game.BoardPrettyString();
                    recorded_data_set.beta = beta;
                    recorded_data_set.alpha = alpha;
#endif
#if DEBUG || DEBUG_STATS_ONLY
                    recorded_data_set.endpoints = 1;
                    recorded_data_set.nodes = 0;
                    recorded_data_set.found_nodes = 1;
#endif

                    return recorded_data_set;
                }

                // Otherwise, we use the saved entry for move optimization.
                else
                {
                    ordered_moves = OptimizeMoveOrder(game, maximizing, saved_entry, transpoition_info);
                }


            }

            #endregion

            var data_set = new MinimaxDataSet();

#if DEBUG
            data_set.ialpha = alpha;
            data_set.ibeta = beta;
#endif
#if DEBUG || DEBUG_STATS_ONLY
            data_set.nodes += 1;
#endif

            var weights = new Dictionary<int, MoveInfo>(game_copy.ValidMoves.Count);
            var stop = false;
            var i = 0;
            for (i = 0; i < ordered_moves.Count; i++)
            {
                Game new_game;
                int new_depth;
                bool new_maximizing;

                LoopPre(ordered_moves[i], game_copy, depth, out new_game, out new_depth, out new_maximizing);

                MinimaxDataSet minimax_res = Eval(new_game, new_depth, new_maximizing, alpha, beta, ref permutations, prev_permutations);
                weights.Add(ordered_moves[i], new MoveInfo(minimax_res.weight));

                stop = AlphaBeta(maximizing, minimax_res.weight, ref alpha, ref beta);

#if DEBUG || DEBUG_STATS_ONLY

                data_set.found_nodes += minimax_res.found_nodes;
                data_set.endpoints += minimax_res.endpoints;
                data_set.nodes += minimax_res.nodes;
#endif
#if DEBUG
                data_set.subsets.Add(minimax_res);
#endif

                // Potential early out via AlphaBeta.
                if (stop)
                {
#if DEBUG
                    data_set.stopped = true;
#endif
                    break;
                }
            }

            data_set.weight = GetBestWeight(weights, maximizing);
            data_set.valid_move_w_weights = weights;
            data_set.depth = depth;

            //if (stop)
            //    data_set.weight = maximizing ? alpha : beta;

#if DEBUG
            data_set.alpha = alpha;
            data_set.beta = beta;
            data_set.maximizing = maximizing;
            data_set.boardString = game_copy.BoardPrettyString();
#endif

            Minimax.TranspositionUncheckedSet(game_copy, data_set, ref permutations);


            return data_set;
        }

        public MinimaxDataSet Eval(Game game, int depth, bool maximizing, ref ConcurrentDictionary<string, MinimaxDataSet> permutations, Dictionary<string, MinimaxDataSet> prev_permutations)
        {
            return Eval(game, depth, maximizing, Globals.MIN, Globals.MAX, ref permutations, prev_permutations);
        }

        public MinimaxDataSet Eval(Game game, int depth, bool maximizing)
        {
            var permutations = new ConcurrentDictionary<string, MinimaxDataSet>();
            return Eval(game, depth, maximizing, Globals.MIN, Globals.MAX, ref permutations, ReferenceTranspositionTable);
        }

        /// <summary>
        /// Initializes a copy of the given Game, advances the board by placing a disk at the given index, and determines the new depth and maximizing state.
        /// </summary>
        /// <param name="index">Index to place a disk.</param>
        /// <param name="game">Game state to copy.</param>
        /// <param name="depth">Current depth to base new depth off of.</param>
        /// <param name="new_game">The new game instance with the disk placed and its state updated.</param>
        /// <param name="new_depth">The new depth after the operations.</param>
        /// <param name="new_maximizing">The new maximizing status.</param>
        public void LoopPre(int index, Game game, int depth, out Game new_game, out int new_depth, out bool new_maximizing)
        {
            new_game = (Game)game.Clone();
            new_game.PlaceDisk(index);
            new_game.AdvanceBoard();
            new_game.GameAtEnd();

            new_depth = depth;

            // If the current player doesn't have a valid move, but the other player does, advance the board so it's their turn.
            if (new_game.SkipTurn() & game.GameAtEnd() == false)
            {
                new_game.AdvanceBoard();
            }
            
            new_depth -= 1;

            // Because we may skip a player's turn, !maximizing isn't always correct.
            new_maximizing = (new_game.ActivePlayer == Player);
        }

        /// <summary>
        /// Used to update alpha beta variables and determine if there is a need to continue evaluation of the current game tree branch.
        /// </summary>
        /// <param name="maximizing">Current maximizing state.</param>
        /// <param name="weight">Weight result from most recent evaluation.</param>
        /// <param name="alpha">Current alpha value.</param>
        /// <param name="beta">Current beta value.</param>
        /// <returns>If evaluation of current game tree branch can stop.</returns>
        public bool AlphaBeta(bool maximizing, int weight, ref int alpha, ref int beta)
        {
            if (maximizing)
            {
                if (weight > alpha)
                    alpha = weight;
            }
            else
            {
                if (weight < beta)
                    beta = weight;
            }

            return alpha >= beta;
        }

        /// <summary>
        /// Gets the best move given a list of moves and other related data.
        /// </summary>
        private int GetBestWeight(Dictionary<int, MoveInfo> moves, bool maximizing)
        {
            int best_weight;
            if (maximizing)
                best_weight = int.MinValue;
            else
                best_weight = int.MaxValue;

            foreach (var item in moves)
            {
                var weight = item.Value.weight;
                if (
                    (maximizing && weight >= best_weight) ||
                    (!maximizing && weight <= best_weight)
                  )
                {
                    best_weight = weight;
                }
            }

            return best_weight;
        }

        /// <summary>
        /// Gets the weighting value of the current game state.
        /// </summary>
        /// <param name="game">Game state to measure weight value of.</param>
        /// <returns></returns>
        public int GetWeight(Game game)
        {
            if (game.Finished)
            {
                return EndgameWeighting(game, Player);
            }
            else
            {
                return Weighting(game, Player);
            }
        }

        #region Transposition Getters and Setters

        public static MinimaxDataSet TranspositionGet(Game game, IDictionary<string, MinimaxDataSet> table, IDictionary<string, MinimaxDataSet> prev_table)
        {
            var boards = game.BoardPermutations();
            string[] board_strings = new string[boards.Length];
            MinimaxDataSet try_value;

            for (var i = 0; i < boards.Length; i++)
            {
                var boardString = Game.BoardString(boards[i]);

                if (table.TryGetValue(boardString, out try_value))
                    return try_value;

                board_strings[i] = boardString;
            }

            if (prev_table != null)
            {
                for (var i = 0; i < boards.Length; i++)
                {
                    var boardString = board_strings[i];

                    if (prev_table.TryGetValue(boardString, out try_value))
                        return try_value;
                }
            }

            return null;
        }

        public static MinimaxDataSet TranspositionGet(Game game, IDictionary<string, MinimaxDataSet> table, IDictionary<string, MinimaxDataSet> prev_table, out TranspositionFetchInfo info)
        {
            var boards = game.BoardPermutations(); //game.BoardPermutations();
            string[] board_strings = new string[boards.Length];
            MinimaxDataSet try_value;

            for (var i = 0; i < boards.Length; i++)
            {
                var boardString = Game.BoardString(boards[i]);

                if (table.TryGetValue(boardString, out try_value))
                {
                    info = new TranspositionFetchInfo(boardString, i, true);
                    return try_value;
                }

                board_strings[i] = boardString;
            }

            if (prev_table != null)
            {
                for (var i = 0; i < boards.Length; i++)
                {
                    var boardString = board_strings[i];

                    if (prev_table.TryGetValue(boardString, out try_value))
                    {
                        info = new TranspositionFetchInfo(boardString, i, false);
                        return try_value;
                    }
                }
            }
            info = new TranspositionFetchInfo();
            return null;
        }

        public static void TranspositionSet(Game game, MinimaxDataSet entry, ref ConcurrentDictionary<string, MinimaxDataSet> table, bool noRetry)
        {
            // We don't want to match to the previous table, so we pass null; after all, we're looking to add a new entry, if it already exist in the previous table it doesn't matter.
            var existing_entry = TranspositionGet(game, table, null);

            // If the existing entry is equal to, or better than, the new one ignore it.
            if (existing_entry == null)
            {
                if (table.TryAdd(game.BoardString(), entry) == false && noRetry == false)
                {
                    TranspositionSet(game, entry, ref table, true);
                }
            }
            else if (existing_entry.depth < entry.depth)
            {
                if (table.TryUpdate(game.BoardString(), entry, existing_entry) && noRetry == false)
                {
                    TranspositionSet(game, entry, ref table, true);
                }
            }

        }

        public static void TranspositionSet(Game game, MinimaxDataSet entry, ref ConcurrentDictionary<string, MinimaxDataSet> table)
        {
            TranspositionSet(game, entry, ref table, false);
        }

        public static void TranspositionUncheckedSet(Game game, MinimaxDataSet entry, ref ConcurrentDictionary<string, MinimaxDataSet> table)
        {
            table[game.BoardString()] = entry;
        }

        #endregion

        /// <summary>
        /// Converts a list of moves into a list of moves grouped by weight.
        /// </summary>
        /// <param name="weights">Weights to group</param>
        /// <returns>Grouped list of weights.</returns>
        public static SortedDictionary<int, Dictionary<int, Minimax.MoveInfo>> MovesByWeight(IDictionary<int, Minimax.MoveInfo> weights)
        {
            var best_move_indexes = new SortedDictionary<int, Dictionary<int, Minimax.MoveInfo>>();

            foreach (var item in weights)
            {
                if (!best_move_indexes.ContainsKey(item.Value.weight))
                    best_move_indexes.Add(item.Value.weight, new Dictionary<int, Minimax.MoveInfo>());

                best_move_indexes[item.Value.weight].Add(item.Key, item.Value);
            }

            return best_move_indexes;
        }

        /// <summary>
        /// Optimizes the current move set from the given game using the saved entry provided.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="maximizing"></param>
        /// <param name="saved_entry"></param>
        /// <param name="transpoition_info"></param>
        /// <returns></returns>
        public List<int> OptimizeMoveOrder(Game game, bool maximizing, MinimaxDataSet saved_entry, TranspositionFetchInfo transpoition_info)
        {
            var new_saved_entry = (MinimaxDataSet)saved_entry.Clone();
            int[] valid_move_w_weights_keys;

            #region Transpositions for move info based on board permutation, relative to current board state.

            // We only store one of the four possible transpositions in the transposition table,
            // if the one we stored (and thus have loaded) isn't the same, we have to "rotate" the data that was stored to match the current state of the board.
            switch (transpoition_info.boardPermutationIndex)
            {
                case 0:     // Current board state, no action required.
                    break;

                case 1:     // Rot 180

                    valid_move_w_weights_keys = saved_entry.valid_move_w_weights.Keys.ToArray();
                    new_saved_entry.valid_move_w_weights = new Dictionary<int, MoveInfo>(valid_move_w_weights_keys.Length);

                    for (var l = 0; l < valid_move_w_weights_keys.Length; l++)
                    {
                        // old_index is the old index of the key, we need to convert this to the new board index.
                        var old_index = valid_move_w_weights_keys[l];

                        var new_index = game.Board.Length - 1 - old_index;

                        new_saved_entry.valid_move_w_weights.Add(new_index, saved_entry.valid_move_w_weights[old_index]);
                    }
                    break;

                case 2:     // Flip H

                    valid_move_w_weights_keys = saved_entry.valid_move_w_weights.Keys.ToArray();
                    new_saved_entry.valid_move_w_weights = new Dictionary<int, MoveInfo>(valid_move_w_weights_keys.Length);
                    for (var l = 0; l < valid_move_w_weights_keys.Length; l++)
                    {
                        var old_index = valid_move_w_weights_keys[l];

                        // Convert the old index into XY.
                        var xy = game.GetXY(old_index);

                        // Get the new index by flipping x and y.
                        var new_index = game.GetIndex(xy.y, xy.x);

                        new_saved_entry.valid_move_w_weights.Add(new_index, saved_entry.valid_move_w_weights[old_index]);

                    }
                    break;
                case 3:     // Flip V

                    valid_move_w_weights_keys = saved_entry.valid_move_w_weights.Keys.ToArray();
                    new_saved_entry.valid_move_w_weights = new Dictionary<int, MoveInfo>(valid_move_w_weights_keys.Length);
                    var boardSize = game.BoardSize - 1;

                    for (var l = 0; l < valid_move_w_weights_keys.Length; l++)
                    {
                        var old_index = valid_move_w_weights_keys[l];

                        // Convert the old index into XY.
                        var xy = game.GetXY(old_index);

                        var new_index = game.GetIndex(boardSize - xy.y, boardSize - xy.x);

                        new_saved_entry.valid_move_w_weights.Add(new_index, saved_entry.valid_move_w_weights[old_index]);
                    }
                    break;
                default:
                    throw new Exceptions.LogicError();

            }
            #endregion

            var ordered_moves = MoveOrderOptimizer(game, maximizing, new_saved_entry.valid_move_w_weights);

            // Due to pruning (via alpha beta) the valid move weights may not have all possible moves,
            // if so we need to add back in those moves so that none are missed.
            if (ordered_moves.Count != game.ValidMoves.Count)
            {
                for (var k = 0; k < game.ValidMoves.Count; k++)
                {
                    var move = game.ValidMoves[k];
                    if (ordered_moves.IndexOf(move) == -1)
                        ordered_moves.Add(move);
                }
            }

            return ordered_moves;
        }

        /// <summary>
        /// Takes a set of moves and weight information about them and returns an optimized move order based off of it.
        /// </summary>
        private List<int> MoveOrderOptimizer(Game new_game, bool maximizing, Dictionary<int, MoveInfo> moves)
        {
            List<int> new_valid_moves;

            var mapped_weights = new Dictionary<int, Dictionary<int, MoveInfo>>();
            var uniq_weights = new List<int>();

            // Map out the moves to weights.
            foreach (var item in moves)
            {
                var move_index = item.Key;
                var move_weight = item.Value.weight;

                if (!mapped_weights.ContainsKey(move_weight))
                {
                    mapped_weights.Add(move_weight, new Dictionary<int, MoveInfo>());
                    uniq_weights.Add(move_weight);
                }

                mapped_weights[move_weight].Add(item.Key, item.Value);
            }

            // Order the unique weights correctly; this is used as a starting point for
            // correctly ordering the new valid move list.
            if (maximizing)
            {
                uniq_weights.Sort();
                uniq_weights.Reverse();
            }
            else
            {
                uniq_weights.Sort();

            }

            new_valid_moves = new List<int>(moves.Count);

            // Order the moves in the most optimal way, first by weight then by tiebreak.
            for (var i = 0; i < uniq_weights.Count; i++)
            {
                var weight = uniq_weights[i];
                var move_options = mapped_weights[weight];

                while (move_options.Count > 0)
                {
                    var best_move = moveOrderingTiebreak(move_options, new_game);
                    move_options.Remove(best_move);
                    new_valid_moves.Add(best_move);
                }
            }

            return new_valid_moves;
        }

        /// <summary>
        /// Class used to track results from Minimax evaluation.
        /// </summary>
        public class MinimaxDataSet : ICloneable
        {
            public MinimaxDataSet()
            {

                weight = 0;
                valid_move_w_weights = new Dictionary<int, MoveInfo>();
                depth = 0;

#if DEBUG || DEBUG_STATS_ONLY
                endpoints = 0;
                nodes = 0;
                found_nodes = 0;
#endif
#if DEBUG
                stopped = false;
                endpoint = false;
                via_lookup = false;

                maximizing = true;
                subsets = new List<MinimaxDataSet>();
                boardString = "";

                alpha = 0;
                beta = 0;
#endif

            }


            public int weight { get; set; }
            public Dictionary<int, MoveInfo> valid_move_w_weights { get; set; }
            public int depth { get; set; }

#if DEBUG || DEBUG_STATS_ONLY

            public int endpoints { get; set; }
            public int nodes { get; set; }
            public int found_nodes { get; set; }

#endif

#if DEBUG
            public bool stopped { get; set; }
            public bool endpoint { get; set; }
            public bool via_lookup { get; set; }


            public int ialpha { get; set; }
            public int ibeta { get; set; }
            public int alpha { get; set; }
            public int beta { get; set; }

            public bool maximizing { get; set; }

            public List<MinimaxDataSet> subsets { get; set; }

            public string boardString { get; set; }
#endif



            public object Clone()
            {
                MinimaxDataSet mds = new MinimaxDataSet();

                mds.weight = this.weight;
                mds.valid_move_w_weights = new Dictionary<int, MoveInfo>(this.valid_move_w_weights);
                mds.depth = this.depth;

#if DEBUG
                mds.subsets = new List<MinimaxDataSet>();
                mds.alpha = this.alpha;
                mds.beta = this.beta;
#endif
                return mds;

            }
        }

        /// <summary>
        /// Struct used to track info about a single move.
        /// </summary>
        public struct MoveInfo
        {
            public MoveInfo(int weight)
            {
                this.weight = weight;
            }

            public int weight;
        }

        /// <summary>
        /// Struct used to track info about a retrieved saved transposition entry.
        /// </summary>
        public struct TranspositionFetchInfo
        {
            public TranspositionFetchInfo(string boardKey, int boardPermutationIndex, bool fromCurrent)
            {
                this.boardKey = boardKey;
                this.boardPermutationIndex = boardPermutationIndex;
                this.fromCurrent = fromCurrent;
            }
            public string boardKey;
            public int boardPermutationIndex;
            public bool fromCurrent;
        }

    }
}