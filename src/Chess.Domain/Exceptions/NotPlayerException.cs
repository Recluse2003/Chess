using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Domain.Exceptions
{
    public class NotPlayerException : DomainException
    {
        public NotPlayerException() : base("User is not a player of this game.") { }
    }
}
