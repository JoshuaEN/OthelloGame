using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloGame.MoveSelectors.RandomErrors
{
    class RandomError_Rl_T2 : RandomErrorBase
    {
        public RandomError_Rl_T2()
        {
            this.region = Regions.LateGame;
            this.times = 2;
        }
    }
}
