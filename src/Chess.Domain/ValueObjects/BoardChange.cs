using Chess.Domain.Enums;

namespace Chess.Domain.ValueObjects
{
    /// <summary>
    /// Represents the change of a specified square after a move.
    /// </summary>
    /// <param name="File">The file of the square.</param>
    /// <param name="Rank">The rank of the square.</param>
    /// <param name="Piece">The piece the square changed to after the move. Can also represent an empty
    /// square.</param>
    public record BoardChange(int File, int Rank, char? Piece);
}
