﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloGame.Weighting
{
    /// <summary>
    /// One of, if not the best, weighting algorithm; Tiered Weighting Compressed was ultimately chosen because it's smaller jumps appeared to work better for being Adaptive.
    /// </summary>
    class TieredWeighting : IWeighting
    {
        public int Do(OthelloGame.Game game, int player)
        {
            var them = game.OtherPlayer(player);
            var us = player;

            var stable_disks = game.GetStableCounts();
            var stable_disk_ratio = GetRatio(stable_disks, us, them);

            var frontier_disks = game.GetFrontierCounts();
            var frontier_disk_ratio = GetRatio(frontier_disks, them, us); // Flip them and us, because we want less, rather than more, frontier disks.

            var unstable_disks = game.GetCounts();
            unstable_disks[0] -= stable_disks[0];
            unstable_disks[1] -= stable_disks[1];

            var unstable_disk_ratio = GetRatio(unstable_disks, them, us); // Again, we want to have less unstable disks.

            var weight = 0;

            // Use multipliers to create tiering.
            if (stable_disk_ratio != 0)
                weight += stable_disk_ratio * 10000;

            if(frontier_disk_ratio != 0)
                weight += frontier_disk_ratio * 100;

            if (unstable_disk_ratio != 0)
                weight += unstable_disk_ratio * 1;

            return weight;
        }

        protected int GetRatio(Game.Counts counter, int us, int them)
        {
            return counter[us] - counter[them];
        }
    }
}
