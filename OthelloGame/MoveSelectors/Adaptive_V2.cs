using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloGame.MoveSelectors
{
    class Adaptive_V2_R2 : IMoveSelector
    {
        public int Select(SortedDictionary<int, Dictionary<int, Minimax.MoveInfo>> moves_by_weight, Game game)
        {
            var controller = (game.ActivePlayerController as Controllers.AIMinimax);
            var other_controller = (game.PlayerControllers[game.OtherPlayer(game.ActivePlayer)] as Controllers.AIMinimax);

            if (other_controller.Weighting.GetType() != controller.Weighting.GetType() && other_controller.AlterWeighting.GetType() != controller.Weighting.GetType())
                throw new Exceptions.LogicError("No valid weighting data on other player is available.");

            var handicap = 0;

            var lookback_min = 3;
            var lookback_target = controller.OppoentMoveData.Count;

            if (controller.OppoentMoveData.Count >= lookback_min)
            {

                Func<Controllers.AIMinimax.MoveEvalData, double> func = v =>
                {
                    return v.chosen_dev_weight * (1.0 - v.prob_picked) * v.prob_better;
                };

                var other_lookback_target = other_controller.OppoentMoveData.Count;

                double data_avg = controller.OppoentMoveData.GetRange(controller.OppoentMoveData.Count - lookback_target, lookback_target).Select(func).Average() -
                                    other_controller.OppoentMoveData.GetRange(other_controller.OppoentMoveData.Count - other_lookback_target, other_lookback_target).Select(func).Average();

                handicap = (int)Math.Round(data_avg);

                controller.InfoboxPreStr = "";

                if (handicap < 0)
                    handicap = 0;
            }
            controller.Handycap = handicap;

            var best_weight = int.MinValue;
            var best_possible_weight = moves_by_weight.ElementAt(moves_by_weight.Count - 1).Key;

            var board_solved = false;

            // Board is solved, we now try to get as close to a least advantage win as possible.
            if (best_possible_weight >= Globals.WIN)
            {
                handicap = 0;
                board_solved = true;
            }
            else if (game.MoveHistory.Count >= 50)
            {
                //  handicap = 0;
            }

            if (board_solved)
            {
                for (var i = moves_by_weight.Count - 1; i >= 0; i--)
                {
                    var item = moves_by_weight.ElementAt(i);
                    if (best_weight == int.MinValue)
                    {
                        best_weight = item.Key;
                    }
                    else if (item.Key > Globals.WIN)
                    {
                        best_weight = item.Key;
                    }
                }
            }
            else
            {
                for (var i = moves_by_weight.Count - 1; i >= 0; i--)
                {
                    var item = moves_by_weight.ElementAt(i);
                    if (best_weight == int.MinValue)
                    {
                        best_weight = item.Key;
                    }
                    else if ((item.Key + handicap) > best_possible_weight)// && item.Key > 0)
                    {
                        best_weight = item.Key;
                    }
                }
            }

            return best_weight;
        }

        // ref: http://stackoverflow.com/a/6252351
        private double CalculateStdDev(IEnumerable<double> values)
        {
            double avg = values.Average();
            return Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
        }
    }


}

