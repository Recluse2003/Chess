using Chess.Domain.Enums;

namespace Chess.Domain.ValueObjects
{
    public record BoardChange(int File, int Rank, char? Piece);
}
