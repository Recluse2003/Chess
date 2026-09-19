using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Domain.Exceptions
{
    public class InvalidTurnException : DomainException
    {
        public InvalidTurnException() : base("An attempt was made to move a piece out of turn sequence.") { }
    }
}
