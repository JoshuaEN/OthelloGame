using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloGame.Weighting
{
    /// <summary>
    /// Very naive approach; attempts to have the most number of disks, relative to it's opponent, at all times.
    /// </summary>
    class DiskDifference : Weighting.IWeighting
    {
        public int Do(OthelloGame.Game game, int player)
        {
            var us = player;
            var them = game.OtherPlayer(us);

            var counts = game.GetCounts();

            return counts[us] - counts[them];
        }
    }
}
