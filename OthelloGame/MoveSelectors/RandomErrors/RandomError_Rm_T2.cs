using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloGame.MoveSelectors.RandomErrors
{
    class RandomError_Rm_T2 : RandomErrorBase
    {
        public RandomError_Rm_T2()
        {
            this.region = Regions.MidGame;
            this.times = 2;
        }
    }
}
