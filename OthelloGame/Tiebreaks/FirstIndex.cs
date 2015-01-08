using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloGame.Tiebreaks
{
    class FirstIndex : ITiebreak
    {
        public int Do(IDictionary<int, Minimax.MoveInfo> moves, Game game)
        {
            if (moves.Count > 0)
                return moves.ElementAt(0).Key;
            else
                throw new ArgumentException();
        }
    }
}
