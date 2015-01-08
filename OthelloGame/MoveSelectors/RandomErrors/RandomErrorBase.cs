using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloGame.MoveSelectors.RandomErrors
{
    public abstract class RandomErrorBase : IMoveSelector
    {
        protected Regions region = Regions.Any;
        protected int times = 0;

        private static readonly Best best_selector = new Best();
        private static readonly RandomMove random_selector = new RandomMove();


        // ref: http://blogs.msdn.com/b/jfoscoding/archive/2006/07/18/670497.aspx
        // Because no solution is ever perfect I guess.
#if FIXED_RNG
        [ThreadStatic]
        private static System.Random _rng;

        public static System.Random rng 
        { 
            get 
            {
                if (_rng == null)
                    _rng = new System.Random(Globals.RNG_SEED);

                return _rng;
            } 
        }
#else
        [ThreadStatic]
        private static System.Random _rng;

        public static System.Random rng 
        { 
            get 
            {
                if (_rng == null)
                    _rng = new System.Random();

                return _rng;
            } 
        }
#endif
        public static readonly System.Random rng2 = new System.Random();
        public int Select(SortedDictionary<int, Dictionary<int, Minimax.MoveInfo>> moves_by_weight, Game game)
        {

            int current_move = game.MoveHistory.Count + 1;
            bool canHaveRng = false;
            if ( region == Regions.Any ||
                (region == Regions.EarlyGame && current_move <= 20) ||
                (region == Regions.MidGame && current_move <= 40 && current_move > 20) ||
                (region == Regions.LateGame && current_move > 40))
                canHaveRng = true;


            // Each region is 20 moves, 10 per a side.
            int total_moves = 10;
            if (region == Regions.Any)
                total_moves = 30;

            // Logic! If distribution is random (and thus equal for every number within the range), logic indicates that each number should come up once,
            // if the rng is checked the same number of times as the range.
            // Thus, by checking if the result of the next rng value is less than times, we effective randomly select a move x times within the region.
            var rng_r = rng.Next(0, total_moves);
            if (canHaveRng && times > rng_r)
                if(moves_by_weight.Count == 1) // If there is only one weight to pick, just return it.
                    return moves_by_weight.ElementAt(0).Key;
                else // Otherwise, we never want to pick the best weight; random "error" indicates that we want an actual error to occur.
                    // Without excluding the possibility of picking the best weight, there is at times a significant chance that it will be picked.
                    return random_selector.Select(new SortedDictionary<int, Dictionary<int, Minimax.MoveInfo>>(moves_by_weight.Take(moves_by_weight.Count-1).ToDictionary(kp => kp.Key, kp => kp.Value)), game);
            else
                return best_selector.Select(moves_by_weight, game);
        }

        protected enum Regions { Any = -1, EarlyGame, MidGame, LateGame };
    }
}
