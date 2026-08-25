using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Domain.ValueObjects
{
    public record Move
    {
        public Position From { get; }
        public Position To { get; }

        public Move(Position from, Position to)
        {
            From = from;
            To = to;
        }
    }
}
