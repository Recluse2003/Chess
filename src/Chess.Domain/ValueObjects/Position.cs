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

        public static Position FromChessNotation(string notation)
        {
            if (string.IsNullOrWhiteSpace(notation) || notation.Length != 2)
                throw new ArgumentException("Invalid chess position.", nameof(notation));

            char file = char.ToLowerInvariant(notation[0]);
            char rank = notation[1];

            if (file < 'a' || file > 'h')
                throw new ArgumentException("File must be between a and h.", nameof(notation));

            if (rank < '1' || rank > '8')
                throw new ArgumentException("Rank must be between 1 and 8.", nameof(notation));

            return new Position(file - 'a', rank - '1');
        }

        public string ToChessNotation()
        {
            return $"{(char)('a' + File)}{Rank + 1}";
        }
    }
}
