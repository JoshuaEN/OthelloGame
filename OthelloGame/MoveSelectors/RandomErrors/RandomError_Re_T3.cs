using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloGame.MoveSelectors.RandomErrors
{
    class RandomError_Re_T3 : RandomErrorBase
    {
        public RandomError_Re_T3()
        {
            this.region = Regions.EarlyGame;
            this.times = 3;
        }
    }
}
