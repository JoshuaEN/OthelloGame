using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloGame.Events
{
    public class MoveBaseEventArgs : OthelloGameBaseEventArgs
    {
        public int tile_index { get; private set; }
        public int player { get; private set; }
        public bool valid { get; private set; }

        public MoveBaseEventArgs(int player, int tile_index, bool valid)
        {
            this.player = player;
            this.valid = valid;
            this.tile_index = tile_index;
        }
    }
}
