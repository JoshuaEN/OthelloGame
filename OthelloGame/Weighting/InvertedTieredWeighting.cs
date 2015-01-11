using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloGame.Weighting
{
    /// <summary>
    /// Experiment related to making an AI adaptive via the weighting method, doesn't work very well because Minimax assumes both players value things equally.
    /// </summary>
    class InvertedTieredWeighting : TieredWeighting
    {
        public new int Do(OthelloGame.Game game, int player)
        {
            return base.Do(game, player) * -1;
        }
    }
}
