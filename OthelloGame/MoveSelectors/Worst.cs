using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloGame.MoveSelectors
{
    class Worst : IMoveSelector
    {
        public int Select(SortedDictionary<int, Dictionary<int, Minimax.MoveInfo>> moves_by_weight, Game game)
        {
            return moves_by_weight.First().Key;
        }
    }
}
