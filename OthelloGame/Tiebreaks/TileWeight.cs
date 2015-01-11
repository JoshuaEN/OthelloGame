using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloGame.Tiebreaks
{
    /// <summary>
    /// Picks a move out of the given moves based on static tile weight values.
    /// If there's still a tie, tied moves are tie broken randomly.
    /// </summary>
    class TileWeight : ITiebreak
    {
        public static readonly Tiebreaks.RandomTiebreak random = new RandomTiebreak();
        public int Do(IDictionary<int, Minimax.MoveInfo> moves, Game game)
        {
            if (moves.Count > 0)
            {
                var tile_weights = new Dictionary<int, Dictionary<int, Minimax.MoveInfo>>();
                var max_weight = Globals.MIN;
                foreach (var item in moves)
                {
                    var index = item.Key;
                    var tile_weight = game.BoardWeights[index];

                    if (!tile_weights.ContainsKey(tile_weight))
                        tile_weights.Add(tile_weight, new Dictionary<int, Minimax.MoveInfo>());

                    tile_weights[tile_weight].Add(item.Key, item.Value);

                    if (tile_weight > max_weight)
                        max_weight = tile_weight;
                }

                var best = tile_weights[max_weight];

                if (best.Count == 1)
                    return best.ElementAt(0).Key;
                else
                    return random.Do(best, game);

            }
            else
            {
                throw new ArgumentException();
            }
        }
    }
}
