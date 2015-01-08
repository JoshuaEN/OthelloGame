using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloGame.Weighting
{
    class FrontierDiskRatio : WeightingBase
    {
        public override int Do(OthelloGame.Game game, int player)
        {
            var them = game.OtherPlayer(player);
            var us = player;

            var frontier_disks = game.GetFrontierCounts();
            var frontier_disk_ratio = GetRatio(frontier_disks, them, us); // Flip them and us, because we want less, rather than more, frontier disks.

            var weight = 0;

            if (frontier_disk_ratio != 0)
                weight += frontier_disk_ratio * 100;

            return weight;
        }

        protected int GetRatio(Game.Counts counter, int us, int them)
        {
            return counter[us] - counter[them];
        }
    }
}
