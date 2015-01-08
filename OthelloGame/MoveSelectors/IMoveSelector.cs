using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloGame.MoveSelectors
{
    /// <summary>
    /// Interface for move selectors.
    /// </summary>
    public interface IMoveSelector
    {
        int Select(SortedDictionary<int, Dictionary<int, Minimax.MoveInfo>> moves_by_weight, Game game);
    }
}
