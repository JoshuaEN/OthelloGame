using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloGame.MoveSelectors
{
    /// <summary>
    /// Picks the worst possible move, used for the Polite Disposition.
    /// </summary>
    class Worst : IMoveSelector
    {
        public int Select(SortedDictionary<int, Dictionary<int, Minimax.MoveInfo>> moves_by_weight, Game game)
        {
            // Dictionary is sorted lowest to highest, thus the first entry is the one with the worst weight.
            return moves_by_weight.First().Key;
        }
    }
}
