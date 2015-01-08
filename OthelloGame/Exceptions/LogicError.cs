using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloGame.Exceptions
{
    /// <summary>
    /// Error thrown when something illogical occurs.
    /// </summary>
    class LogicError : OthelloGame.Exceptions.OthelloGameExceptionBase
    {
        public LogicError() : base("Logic Error") { }

        public LogicError(string message) : base(message) { }


    }
}
