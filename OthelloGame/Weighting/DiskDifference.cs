using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloGame.Weighting
{
    class DiskDifference : Weighting.WeightingBase
    {
        public override int Do(OthelloGame.Game game, int player)
        {
            var us = player;
            var them = game.OtherPlayer(us);

            var counts = game.GetCounts();

            return counts[us] - counts[them];
        }
    }
}
