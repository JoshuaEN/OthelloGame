using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloGame.Controllers
{
    /// <summary>
    /// Base class for Controllers.
    /// </summary>
    public abstract class ControllerBase
    {
        /// <summary>
        /// Called when it's the controller's players turn.
        /// </summary>
        public abstract void YourMove();

        /// <summary>
        /// Resets the internal state of the controller for use in a new game.
        /// </summary>
        public virtual void Reset()
        {
            // NOOP
        }

        /// <summary>
        /// Draws information about the last evaluated moves on the game board.
        /// </summary>
        /// <param name="renderer">The game renderer to draw on.</param>
        public virtual void DrawBoard(GameRender renderer)
        {
            // NOOP
        }

        /// <summary>
        /// Returns the identifier string used for recording AI match data to an external database.
        /// </summary>
        /// <returns>Unique Identifier String</returns>
        public virtual string UniqueIdentString()
        {
            return "ControllerBase|0.0|C#||";
        }
    }
}
