using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloGame.Events
{
    public class GameStateChangedEventArgs : OthelloGameBaseEventArgs
    {
        public Game.GameStates old_state { get; private set; }
        public Game.GameStates new_state { get; private set; }

        public GameStateChangedEventArgs(Game.GameStates old_state, Game.GameStates new_state)
        {
            this.old_state = old_state;
            this.new_state = new_state;
        }
    }
}
