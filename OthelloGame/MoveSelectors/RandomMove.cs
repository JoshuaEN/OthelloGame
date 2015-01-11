using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloGame.MoveSelectors
{
    /// <summary>
    /// Picks a move weight at random.
    /// </summary>
    public class RandomMove : IMoveSelector
    {
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

        public int Select(SortedDictionary<int, Dictionary<int, Minimax.MoveInfo>> moves_by_weight, Game game)
        {
            return moves_by_weight.ElementAt(rng.Next(0, moves_by_weight.Count)).Key;
        }
    }
}
