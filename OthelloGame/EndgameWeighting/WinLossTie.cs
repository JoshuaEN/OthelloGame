using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloGame.EndgameWeighting
{
    /// <summary>
    /// Simple endgame weighting which returns Globals.WIN, TIE, or LOSS depending upon the game's outcome and the provided player number.
    /// </summary>
    class WinLossTie : EndgameWeightingBase
    {
        public override int Do(OthelloGame.Game game, int player)
        {
            if (game.Winner == player)
                return Globals.WIN;
            else if (game.Winner == -1)
                return Globals.TIE;
            else if (game.Winner == game.OtherPlayer(player))
                return Globals.LOSS;
            else
                throw new Exceptions.LogicError();
        }
    }
}
