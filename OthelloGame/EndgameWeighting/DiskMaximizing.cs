using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloGame.EndgameWeighting
{
    /// <summary>
    /// Returns the number of pieces being won or lost by, added to the base endgame weighting value.
    /// </summary>
    class DiskMaximizing : IEndgameWeighting
    {
        private static readonly WinLossTie win_loss_tie = new WinLossTie();

        public int Do(OthelloGame.Game game, int player)
        {
            // Get the base result value.
            var baseline = win_loss_tie.Do(game, player);

            var counts = game.GetCounts();

            // The winner is given credit for empty board spaces.
            // ref: http://www.cs.cornell.edu/~yuli/othello/othello.html
            if (game.Winner == player)
                baseline += counts.nil;

            // -1 indicates a tie.
            if (game.Winner == -1)
                return baseline;
            else if(game.Winner == player)
                return baseline + counts[player];
            else
                return baseline + counts[player] - game.Board.Length * 2;
        }
    }
}
