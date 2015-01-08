using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloGame.Events
{
    public class MoveValidEventArgs : MoveBaseEventArgs
    {
        public MoveValidEventArgs(int player, int tile_index) : base(player, tile_index, true) { }
    }
}
