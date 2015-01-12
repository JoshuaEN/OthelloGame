using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloGame.MoveSelectors
{
    /// <summary>
    /// Final Adaptive AI move selection code.
    /// Uses two variables related to move quality to determine two chose the best move to achieve least advantage.
    /// </summary>
    class Adaptive_V4_R8 : IMoveSelector
    {
        public int Select(SortedDictionary<int, Dictionary<int, Minimax.MoveInfo>> moves_by_weight, Game game)
        {
            // Get controllers.
            var controller = (game.ActivePlayerController as Controllers.AIMinimax);
            var other_controller = (game.PlayerControllers[game.OtherPlayer(game.ActivePlayer)] as Controllers.AIMinimax);

            // Make sure the data we'll get from the other controller is valid.
            if (other_controller.Weighting.GetType() != controller.Weighting.GetType() && other_controller.AlterWeighting.GetType() != controller.Weighting.GetType())
                throw new Exceptions.LogicError("No valid weighting data on other player is available.");

            // The handicap represents the maximum acceptable weight difference between the best possible weight and the chosen move weight.
            var handicap = 0.0;

            // The probability handicap represents the highest acceptable probability that a better move (than the one picked) would have been picked at random.
            var prob_handicap = 0.0;

            // The minimum number of opponent's moves that must be available to apply any handicap.
            var lookback_min = 3;

            if (controller.OppoentMoveData.Count >= lookback_min)
            {
                // Function used to determine the handicap value of each move.
                Func<Controllers.AIMinimax.MoveEvalData, double> func = v =>
                {
                    return v.chosen_dev_weight + (v.chosen_dev_weight * (1.0 - v.prob_picked)) * v.prob_better;
                };

                // How far to look back into the move history for the opponent.
                var lookback_target = (controller.OppoentMoveData.Count > 20 ? 20 : controller.OppoentMoveData.Count);

                // How far to look back into the move history for our own moves.
                // As an aside, no, I didn't intend for this mismatch in data to occur (the lookback_target assignment above used to be somewhere else). 
                // However, it seems that this accident produces a much better result than if this too is constrained to 20.
                // I think it's more related to the fact the Adaptive AI needs to scale back how much its willing to give up,
                // later in the game to protect itself from losing more, as this is an imperfect solution that is
                // leading to the much lower win-rate against the Stable Disk Ratio weighting.
                var other_lookback_target = other_controller.OppoentMoveData.Count;


                double their = controller.OppoentMoveData.GetRange(controller.OppoentMoveData.Count - lookback_target, lookback_target).Select(func).Average();
                double ours = other_controller.OppoentMoveData.GetRange(other_controller.OppoentMoveData.Count - other_lookback_target, other_lookback_target).Select(func).Average();
                double data_avg = their - ours;

                Func<Controllers.AIMinimax.MoveEvalData, double> o_func = v =>
                {
                    return v.prob_better;
                };

                double o_theirs = controller.OppoentMoveData.GetRange(controller.OppoentMoveData.Count - lookback_target, lookback_target).Select(o_func).Average();
                double o_ours = other_controller.OppoentMoveData.GetRange(other_controller.OppoentMoveData.Count - other_lookback_target, other_lookback_target).Select(o_func).Average();
                double o_data_avg = o_theirs;

                handicap = data_avg;
                prob_handicap = o_theirs;

                controller.InfoboxPreStr = "handicap: " + handicap + " | prob handicap: " + prob_handicap;

                // The handicap can never be negative.
                // A handicap of zero means no handicap.
                if (handicap < 0)
                    handicap = 0;
            }

            var best_weight = int.MinValue;
            var best_possible_weight = moves_by_weight.ElementAt(moves_by_weight.Count - 1).Key;

            var board_solved = false;

            // Board is solved, we now try to get as close to a least advantage win as possible.
            if (best_possible_weight >= Globals.WIN)
            {
                handicap = 0;
                board_solved = true;
            }

            // If the board is solved, pick the lowest possible winning weight (if any).
            if (board_solved)
            {
                // Iterate backwards as the dictionary is sorted from lowest to highest.
                for (var i = moves_by_weight.Count - 1; i >= 0; i--)
                {
                    var item = moves_by_weight.ElementAt(i);
                    if (best_weight == int.MinValue)
                    {
                        best_weight = item.Key;
                    }
                    else if (item.Key >= Globals.WIN)
                    {
                        best_weight = item.Key;
                    }
                }
            }
            // If the board isn't solved, pick the lowest possible weight within the limits of the handicap.
            else
            {
                double total = 0.0;

                // Count up the total number of moves.
                foreach (var item in moves_by_weight)
                    total += item.Value.Count;

                // Tracker for the number of moves which are better than the current one.
                double items_better = 0.0;

                // Iterate backwards as the dictionary is sorted from lowest to highest.
                for (var i = moves_by_weight.Count - 1; i >= 0; i--)
                {
                    var item = moves_by_weight.ElementAt(i);
                    if (best_weight == int.MinValue)
                    {
                        best_weight = item.Key;
                    }
                    // Never pick a move which will lose the game.
                    else if(item.Key <= Globals.LOSS)
                    {
                        break;
                    }
                    else
                    {
                        double prob_better = items_better / total;
                        
                        if( (item.Key + handicap) > best_possible_weight && prob_better < prob_handicap)
                            best_weight = item.Key;
                    }

                    items_better += item.Value.Count;
                }
            }

            return best_weight;
        }

    }

}

