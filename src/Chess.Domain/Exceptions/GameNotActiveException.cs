using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Domain.Exceptions
{
    public class GameNotActiveException : DomainException
    {
        public GameNotActiveException() : base("The chess game is not currently in an active state.") { }
    }
}
