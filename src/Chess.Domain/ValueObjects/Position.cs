namespace Chess.Domain.ValueObjects
{
    /// <summary>
    /// Represents a position on an 8-by-8 chess board.
    /// </summary>
    /// <remarks>
    /// The board uses zero based coordinates. This means that files range from 0 to 7, which represents <c>a</c>
    /// through <c>h</c>, and ranks range from 0 to 7, representing to <c>1</c> through <c>8</c>.
    /// </remarks>
    public readonly record struct Position
    {
        public int File { get; }
        public int Rank { get; }

        public Position(int file, int rank)
        {
            File = file;
            Rank = rank;
        }

        /// <summary>
        /// Creates a new <see cref="Position"/> from chess notation.
        /// </summary>
        /// <param name="notation">The chess notation being converted.</param>
        /// <returns>
        /// The converted <see cref="Position"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the supplied notation is null, empty, has an invalid length, or contains a file 
        /// or rank outside the chess board.
        /// </exception>
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

        /// <summary>
        /// Converts this position to standard chess notation
        /// </summary>
        /// <returns>
        /// A two character chess position such as <c>a1</c>, <c>e4</c>, or /// <c>h8</c>.
        /// </returns>
        public string ToChessNotation()
        {
            return $"{(char)('a' + File)}{Rank + 1}";
        }
    }
}
