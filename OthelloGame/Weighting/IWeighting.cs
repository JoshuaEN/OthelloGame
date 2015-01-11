using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloGame.Weighting
{
    /// <summary>
    /// Interface for Weighting.
    /// </summary>
    public interface IWeighting
    {
        int Do(OthelloGame.Game game, int player);
    }
}
