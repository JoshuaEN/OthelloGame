using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloGame.Events
{
    public class TileChangedEventArgs : OthelloGameBaseEventArgs
    {
        public int x { get; private set; }
        public int y { get; private set; }
        public int i { get; private set; }
        public int new_owner { get; private set; }

        public TileChangedEventArgs(int x, int y, int i, int new_owner)
        {
            this.x = x;
            this.y = y;
            this.i = i;
            this.new_owner = new_owner;
        }
    }
}
