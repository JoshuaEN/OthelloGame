using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloGame.Weighting
{
    class Adaptive_R1 : TieredWeightingCompressed_R2
    {
        public override int Do(Game game, int player)
        {
            var weight = base.Do(game, player);

            if (weight <= 0)
                return weight;

            return 200000 - (weight - 25);
        }
    }
}
