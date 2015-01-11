﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloGame.MoveSelectors
{
    class Adaptive_V4_R8 : IMoveSelector
    {
        public int Select(SortedDictionary<int, Dictionary<int, Minimax.MoveInfo>> moves_by_weight, Game game)
        {
            var controller = (game.ActivePlayerController as Controllers.AIMinimax);
            var other_controller = (game.PlayerControllers[game.OtherPlayer(game.ActivePlayer)] as Controllers.AIMinimax);

            if (other_controller.Weighting.GetType() != controller.Weighting.GetType() && other_controller.AlterWeighting.GetType() != controller.Weighting.GetType())
                throw new Exceptions.LogicError("No valid weighting data on other player is available.");

            var handicap = 0.0;
            var prob_handicap = 0.0;

            var lookback_min = 3;
            var lookback_target = (controller.OppoentMoveData.Count > 20 ? 20 : controller.OppoentMoveData.Count);

            if (controller.OppoentMoveData.Count >= lookback_min)
            {

                Func<Controllers.AIMinimax.MoveEvalData, double> func = v =>
                {
                    var bonusa = (v.chosen_dev_weight * (1.0 - v.prob_picked));
                    var bonusb = (v.chosen_dev_weight * v.prob_better);
                    var act_bonus = (bonusa > bonusb ? bonusa : bonusb);

                    return v.chosen_dev_weight + (v.chosen_dev_weight * (1.0 - v.prob_picked)) * v.prob_better;// +(v.chosen_dev_weight * v.prob_better);
                };

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
                double o_data_avg = o_theirs;// - o_ours);

                //if(o_theirs < o_ours)

                handicap = data_avg;
                prob_handicap = o_theirs;

                //if (o_theirs < o_ours)
                //  handicap = (int)Math.Round((handicap + o_data_avg)/2);

                controller.InfoboxPreStr = "handicap: " + handicap + " | prob handicap: " + prob_handicap;

               // handicap = their;

                if (handicap < 0)
                    handicap = 0;
            }
            controller.Handycap = (int)handicap;

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
                    else if (item.Key >= Globals.WIN)
                    {
                        best_weight = item.Key;
                    }
                }
            }
            else
            {
                double total = 0.0;
                foreach (var item in moves_by_weight)
                    total += item.Value.Count;

                double items_better = 0.0;

                for (var i = moves_by_weight.Count - 1; i >= 0; i--)
                {
                    var item = moves_by_weight.ElementAt(i);
                    if (best_weight == int.MinValue)
                    {
                        best_weight = item.Key;
                    }
                        // Never pick a move which will lose the game, unless it's the only option.
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

        // ref: http://stackoverflow.com/a/6252351
        private double CalculateStdDev(IEnumerable<double> values)
        {
            double avg = values.Average();
            return Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
        }
    }


}

