using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloGame.Tiebreaks
{
    /// <summary>
    /// Picks a random move of the given moves.
    /// </summary>
    class RandomTiebreak : ITiebreak
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
        public int Do(IDictionary<int, Minimax.MoveInfo> moves, Game game)
        {
            if (moves.Count > 0)
                return moves.ElementAt(rng.Next(0, moves.Count)).Key;
            else
                throw new ArgumentException();
        }
    }
}
