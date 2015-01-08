using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloGame.Events
{
    public class PlayerReadyToMoveEventArgs : OthelloGameBaseEventArgs
    {
        public int Player { get; private set; }

        public PlayerReadyToMoveEventArgs(int player)
        {
            Player = player;
        }
    }
}
