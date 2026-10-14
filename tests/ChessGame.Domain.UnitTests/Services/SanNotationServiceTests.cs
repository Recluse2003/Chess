using Chess.Domain.Services;
using Chess.Domain.ValueObjects;
using Xunit;

namespace Chess.Domain.Tests.Services;

public class SanNotationServiceTests
{
    private readonly SanNotationService _sut;

    public SanNotationServiceTests()
    {
        var rules = new ChessRulesService();
        _sut = new SanNotationService(rules);
    }

    [Fact]
    public void Generate_PawnAdvance_ReturnsDestinationSquare()
    {
        var before = CreateBoard(
            whiteToMove: true,
            ("e2", 'P'),
            ("e8", 'k'),
            ("a8", 'K'));

        var move = CreateMove("e2", "e4");

        var after = before.ApplyMove(move).Board;

        var result = _sut.Generate(before, move, after);

        Assert.Equal("e4", result);
    }

    [Fact]
    public void Generate_KnightMove_IncludesPieceLetter()
    {
        var before = CreateBoard(
            whiteToMove: true,
            ("b1", 'N'),
            ("e1", 'K'),
            ("e8", 'k'));

        var move = CreateMove("b1", "c3");
        var after = before.ApplyMove(move).Board;

        var result = _sut.Generate(before, move, after);

        Assert.Equal("Nc3", result);
    }

    [Fact]
    public void Generate_PieceCapture_IncludesCaptureMarker()
    {
        var before = CreateBoard(
            whiteToMove: true,
            ("c4", 'B'),
            ("f7", 'p'),
            ("e1", 'K'),
            ("e8", 'k'));

        var move = CreateMove("c4", "f7");
        var after = before.ApplyMove(move).Board;

        var result = _sut.Generate(before, move, after);

        Assert.Equal("Bxf7+", result);
    }

    [Fact]
    public void Generate_PawnCapture_IncludesSourceFile()
    {
        var before = CreateBoard(
            whiteToMove: true,
            ("e4", 'P'),
            ("d5", 'p'),
            ("e1", 'K'),
            ("e8", 'k'));

        var move = CreateMove("e4", "d5");
        var after = before.ApplyMove(move).Board;

        var result = _sut.Generate(before, move, after);

        Assert.Equal("exd5", result);
    }

    [Fact]
    public void Generate_KingsideCastling_ReturnsCastlingNotation()
    {
        var before = CreateBoard(
            whiteToMove: true,
            ("e1", 'K'),
            ("h1", 'R'),
            ("e8", 'k'),
            ("a8", 'K'));

        var move = CreateMove("e1", "g1");
        var after = before.ApplyMove(move).Board;

        var result = _sut.Generate(before, move, after);

        Assert.Equal("O-O", result);
    }

    [Fact]
    public void Generate_QueensideCastling_ReturnsCastlingNotation()
    {
        var before = CreateBoard(
            whiteToMove: true,
            ("e1", 'K'),
            ("a1", 'R'),
            ("e8", 'k'),
            ("h8", 'K'));

        var move = CreateMove("e1", "c1");
        var after = before.ApplyMove(move).Board;

        var result = _sut.Generate(before, move, after);

        Assert.Equal("O-O-O", result);
    }

    [Fact]
    public void Generate_PawnPromotion_IncludesPromotionPiece()
    {
        var before = CreateBoard(
            whiteToMove: true,
            ("e7", 'P'),
            ("a1", 'K'),
            ("h8", 'k'));

        var move = CreateMove("e7", "e8", 'Q');
        var after = before.ApplyMove(move).Board;

        var result = _sut.Generate(before, move, after);

        Assert.Equal("e8=Q+", result);
    }

    [Fact]
    public void Generate_AmbiguousKnights_UsesFileDisambiguation()
    {
        var before = CreateBoard(
            whiteToMove: true,
            ("b1", 'N'),
            ("f1", 'N'),
            ("a1", 'K'),
            ("h8", 'k'));

        var move = CreateMove("b1", "d2");

        var after = before.ApplyMove(move).Board;

        var result = _sut.Generate(before, move, after);

        Assert.Equal("Nbd2", result);
    }

    [Fact]
    public void Generate_MoveGivesCheck_AppendsPlus()
    {
        var before = CreateBoard(
            whiteToMove: true,
            ("d1", 'R'),
            ("a1", 'K'),
            ("e8", 'k'));

        var move = CreateMove("d1", "e1");
        var after = before.ApplyMove(move).Board;

        var result = _sut.Generate(before, move, after);

        Assert.Equal("Re1+", result);
    }

    [Fact]
    public void Generate_AmbiguousKnightsOnSameFile_UsesRankDisambiguation()
    {
        var before = CreateBoard(
            whiteToMove: true,
            ("d1", 'K'),
            ("b1", 'N'),
            ("b5", 'N'),
            ("h8", 'k'));

        var move = CreateMove("b1", "d2");
        var after = before.ApplyMove(move).Board;

        var result = _sut.Generate(before, move, after);

        Assert.Equal("N1d2", result);
    }

    [Fact]
    public void Generate_AmbiguousKnightsOnSameFileAndRank_UsesFullSquare()
    {
        var before = CreateBoard(
            whiteToMove: true,
            ("a1", 'K'),
            ("b3", 'N'),
            ("b5", 'N'),
            ("h8", 'k'));

        var move = CreateMove("b3", "d4");
        var after = before.ApplyMove(move).Board;

        var result = _sut.Generate(before, move, after);

        Assert.Equal("Nb3d4", result);
    }

    [Fact]
    public void Generate_EnPassantCapture_IncludesCaptureMarker()
    {
        var before = CreateBoard(
            whiteToMove: true,
            ParseSquare("d6"),
            ("e5", 'P'),
            ("d5", 'p'),
            ("a1", 'K'),
            ("h8", 'k'));

        var move = CreateMove("e5", "d6");
        var after = before.ApplyMove(move).Board;

        var result = _sut.Generate(before, move, after);

        Assert.Equal("exd6", result);
    }

    [Fact]
    public void Generate_BlackPawnAdvance_ReturnsDestinationSquare()
    {
        var before = CreateBoard(
            whiteToMove: false,
            ("e7", 'p'),
            ("a1", 'K'),
            ("h8", 'k'));

        var move = CreateMove("e7", "e5");
        var after = before.ApplyMove(move).Board;

        var result = _sut.Generate(before, move, after);

        Assert.Equal("e5", result);
    }

    [Fact]
    public void Generate_BlackPieceCapture_IncludesCaptureMarker()
    {
        var before = CreateBoard(
            whiteToMove: false,
            ("c5", 'r'),
            ("c2", 'P'),
            ("a1", 'K'),
            ("h8", 'k'));

        var move = CreateMove("c5", "c2");
        var after = before.ApplyMove(move).Board;

        var result = _sut.Generate(before, move, after);

        Assert.Equal("Rxc2+", result);
    }

    [Fact]
    public void Generate_KingsideCastlingGivesCheck_AppendsPlus()
    {
        var before = CreateBoard(
            whiteToMove: true,
            ("e1", 'K'),
            ("h1", 'R'),
            ("f8", 'k'));

        var move = CreateMove("e1", "g1");
        var after = before.ApplyMove(move).Board;

        var result = _sut.Generate(before, move, after);

        Assert.Equal("O-O+", result);
    }

    [Fact]
    public void Generate_AmbiguousKnights_UsesFullSquareDisambiguation()
    {
        var before = CreateBoard(
            whiteToMove: true,
            ("a1", 'K'),
            ("b3", 'N'),
            ("b5", 'N'),
            ("f3", 'N'),
            ("h8", 'k'));

        var move = CreateMove("b3", "d4");
        var after = before.ApplyMove(move).Board;

        var result = _sut.Generate(before, move, after);

        Assert.Equal("Nb3d4", result);
    }

    [Fact]
    public void Generate_PawnPromotionToKnight_IncludesPromotionPiece()
    {
        var before = CreateBoard(
            whiteToMove: true,
            ("e7", 'P'),
            ("a1", 'K'),
            ("h8", 'k'));

        var move = CreateMove("e7", "e8", 'N');
        var after = before.ApplyMove(move).Board;

        var result = _sut.Generate(before, move, after);

        Assert.Equal("e8=N", result);
    }

    [Fact]
    public void Generate_PawnCapturePromotion_IncludesCaptureAndPromotion()
    {
        var before = CreateBoard(
            whiteToMove: true,
            ("e7", 'P'),
            ("d8", 'r'),
            ("a1", 'K'),
            ("h8", 'k'));

        var move = CreateMove("e7", "d8", 'Q');
        var after = before.ApplyMove(move).Board;

        var result = _sut.Generate(before, move, after);

        Assert.Equal("exd8=Q+", result);
    }

    [Fact]
    public void Generate_MoveDeliversCheckmate_AppendsHash()
    {
        var before = CreateBoard(
            whiteToMove: true,
            ("f6", 'Q'),
            ("f7", 'K'),
            ("h8", 'k'));

        var move = CreateMove("f6", "g7");
        var after = before.ApplyMove(move).Board;

        var result = _sut.Generate(before, move, after);

        Assert.Equal("Qg7#", result);
    }

    [Fact]
    public void Generate_PinnedAlternativeKnight_DoesNotDisambiguate()
    {
        var before = CreateBoard(
            whiteToMove: true,
            ("e1", 'K'),
            ("b1", 'N'),
            ("e4", 'N'),
            ("e8", 'r'),
            ("h8", 'k'));

        var move = CreateMove("b1", "d2");
        var after = before.ApplyMove(move).Board;

        var result = _sut.Generate(before, move, after);

        Assert.Equal("Nd2", result);
    }

    private static Board CreateBoard(bool whiteToMove, params (string Square, char Piece)[] pieces)
    {
        return CreateBoard(whiteToMove, null, pieces);
    }

    private static Board CreateBoard(bool whiteToMove, Position? enPassantTarget, params (string Square, char Piece)[] pieces)
    {
        var squares = Board.EmptyBoard();

        foreach (var (square, piece) in pieces)
        {
            var position = ParseSquare(square);
            squares[position.File, position.Rank] = piece;
        }

        return new Board(
            squares,
            whiteToMove,
            castlingRights: "",
            enPassantTarget: enPassantTarget,
            halfmoveClock: 0,
            fullmoveNumber: 1);
    }



    private static Move CreateMove(string from, string to, char? promotion = null)
    {
        return new Move(Guid.NewGuid(), ParseSquare(from), ParseSquare(to), promotion);
    }

    private static Position ParseSquare(string square)
    {
        return new Position(square[0] - 'a', square[1] - '1');
    }
}