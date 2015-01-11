using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloGame.Weighting
{
    /// <summary>
    /// This was an experiment to try making the AI adaptive by changing the weights.
    /// This has several flaws and was ultimately abandoned.
    /// </summary>
    class Adaptive_R1 : TieredWeightingCompressed_R2
    {
        public new int Do(Game game, int player)
        {
            var weight = base.Do(game, player);

            if (weight <= 0)
                return weight;

            // We're subtracting from a large number so the biggest weight has the lowest value,
            // and the -25 is a static lower limit.
            return 200000 - (weight - 25);
        }
    }
}
