using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloGame.Weighting
{
    class StableDiskRatio : WeightingBase
    {

        public override int Do(OthelloGame.Game game, int player)
        {
            var them = game.OtherPlayer(player);
            var us = player;

            var stable_disks = game.GetStableCounts();
            var stable_disk_ratio = GetRatio(stable_disks, us, them);

            var weight = 0;

            if (stable_disk_ratio != 0)
                weight += stable_disk_ratio;

            return weight;
        }

        protected int GetRatio(Game.Counts counter, int us, int them)
        {
            return counter[us] - counter[them];
        }
    }
}
