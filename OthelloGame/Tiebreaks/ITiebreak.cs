using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OthelloGame;

namespace OthelloGame.Tiebreaks
{
    /// <summary>
    /// Interface for tiebreaking.
    /// </summary>
    public interface ITiebreak
    {
        int Do(IDictionary<int, Minimax.MoveInfo> moves, Game game);
    }
}
