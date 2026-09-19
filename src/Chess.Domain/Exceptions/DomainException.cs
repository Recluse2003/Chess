using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Domain.Exceptions
{
    public abstract class DomainException : Exception
    {
        protected DomainException(string message) : base(message) { }
    }
}
