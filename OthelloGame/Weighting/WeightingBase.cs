using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloGame.Weighting
{
    public abstract class WeightingBase
    {
        public abstract int Do(OthelloGame.Game game, int player);

        public virtual int GetDepth(Controllers.AIMinimax ai)
        {
            return -1;
        }

        public virtual bool CanUndo { get { return false; } }
        public virtual int Undo(int weight)
        {
            throw new NotImplementedException();
        }
    }
}
