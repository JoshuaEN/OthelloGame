using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloGame.MoveSelectors.RandomErrors
{
    class RandomError_Rl_T1 : RandomErrorBase
    {
        public RandomError_Rl_T1()
        {
            this.region = Regions.LateGame;
            this.times = 1;
        }
    }
}
