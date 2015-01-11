using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloGame.MoveSelectors
{
    /// <summary>
    /// Picks the best possible move weight.
    /// </summary>
    class Best : IMoveSelector
    {
        public int Select(SortedDictionary<int, Dictionary<int, Minimax.MoveInfo>> moves_by_weight, Game game)
        {
            // Dictionary is sorted lowest to highest, thus the last entry has the best weight.
            return moves_by_weight.Last().Key;
        }
    }
}
