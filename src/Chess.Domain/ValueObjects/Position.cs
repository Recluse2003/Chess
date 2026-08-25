using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Domain.ValueObjects
{
    public readonly record struct Position
    {
        public int File { get; }
        public int Rank { get; }

        public Position(int file, int rank)
        {
            if (file < 0 || file > 7)
                throw new ArgumentOutOfRangeException(nameof(file));

            if (rank < 0 || rank > 7)
                throw new ArgumentOutOfRangeException(nameof(rank));

            File = file;
            Rank = rank;
        }

        public static Position operator +(Position position, Position offset)
        {
            return new Position(
                position.File + offset.File,
                position.Rank + offset.Rank);
        }
    }
}
