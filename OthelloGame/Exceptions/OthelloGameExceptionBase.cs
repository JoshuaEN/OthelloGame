using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloGame.Exceptions
{
    /// <summary>
    /// Base exception class for project.
    /// </summary>
    class OthelloGameExceptionBase : Exception
    {
        public OthelloGameExceptionBase() : base() { }
        public OthelloGameExceptionBase(string message) : base() { }
        public OthelloGameExceptionBase(string message, Exception inner) : base(message, inner) { }
    }
}
