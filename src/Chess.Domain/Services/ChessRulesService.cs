using Chess.Domain.ValueObjects;

namespace Chess.Domain.Services
{
    /// <summary>
    /// Provides chess rule validation and move-generation functionality.
    /// </summary>
    /// <remarks>  
    /// This service is responsible for determining which moves are available 
    /// to pieces, filtering moves that would leave the player's king in check, 
    /// detecting checkmate and stalemate, and determining whether a position 
    /// contains insufficient material to checkmate.
    /// </remarks>
    public class ChessRulesService
    {
        /// <summary>
        /// Calculates where a specified move is legal for the current board. 
        /// </summary>
        /// <param name="board">The current state of the chess board.</param>
        /// <param name="move">The move to validate. It includes the starting and ending positions.</param>
        /// <returns>
        /// <c>true</c> if the move is legal; otherwise, <c>false</c>.
        /// </returns>
        public bool IsMoveLegal(Board board, Move move)
        {
            if (board.GetPiece(move.From) == '.')
                return false;

            var legalMoves = GetLegalMoves(board, move.From);

            return legalMoves.Contains(move.To);
        }

        /// <summary>
        /// Retrieves all legal moves available for a piece at the specified position.
        /// </summary>
        /// <param name="board">The current state of the chess board.</param>
        /// <param name="piecePosition">The position of the piece whose legal moves will be determined.</param>
        /// <returns>
        /// A collection containing all legal destination positions for the piece.
        /// </returns>
        public List<Position> GetLegalMoves(Board board, Position piecePosition)
        {
            var candidateMoves = GetCandidateMoves(board, piecePosition);

            return RemoveKingCheckMoves(board, piecePosition, candidateMoves);
        }

        /// <summary>
        /// Gets the possible movement destinations for a piece without determining if the move would leave its
        /// own king in check.
        /// </summary>
        /// <param name="board">The current state of the chess board.</param>
        /// <param name="piecePosition">The position of the piece whose candidate moves should be calculated.</param>
        /// <returns>
        /// A list containing candidate destination positions.
        /// </returns>
        public List<Position> GetCandidateMoves(Board board, Position piecePosition)
        {
            var piece = board.GetPiece(piecePosition);

            var candidateMoves = piece switch
            {
                'p' or 'P' => GetPawnMoves(board, piecePosition),
                'n' or 'N' => GetKnightMoves(board, piecePosition),
                'r' or 'R' => GetRookMoves(board, piecePosition),
                'b' or 'B' => GetBishopMoves(board, piecePosition),
                'q' or 'Q' => GetQueenMoves(board, piecePosition),
                'k' or 'K' => GetKingMoves(board, piecePosition),
                _ => []
            };

            return candidateMoves;
        }

        /// <summary>
        /// Generates candidate moves for a pawn.
        /// </summary>
        /// <param name="board">The current state of the chess board.</param>
        /// <param name="piecePosition">The current position of the pawn.</param>
        /// <returns>
        /// A list containing valid pawn destinations including regular and eligible en passant captures.
        /// </returns>
        private List<Position> GetPawnMoves(Board board, Position piecePosition)
        {
            char piece = board.GetPiece(piecePosition);

            bool isWhitePiece = char.IsUpper(piece);
            int direction = isWhitePiece ? 1 : -1;
            int startingRank = isWhitePiece ? 1 : 6;

            List<Position> candidatePositions = new List<Position>();

            // One square forward
            Position forward = new Position(piecePosition.File, piecePosition.Rank + direction);

            if (board.GetPiece(forward) == '.')
            {
                candidatePositions.Add(forward);

                // Two squares forward
                if (piecePosition.Rank == startingRank)
                {
                    int doubleForwardRank = piecePosition.Rank + direction * 2;
                    Position doubleForward = new Position(piecePosition.File, doubleForwardRank);

                    if (board.GetPiece(doubleForward) == '.')
                        candidatePositions.Add(doubleForward);
                }
            }

            // Capture diagonally left, from white perspective
            Position forwardLeft = new Position(piecePosition.File - 1, piecePosition.Rank + direction);

            if (forwardLeft.File >= 0)
            {
                char forwardLeftValue = board.GetPiece(forwardLeft);
                
                // Verify that pawn can capture diagonally left, or en passant is possible
                if ((forwardLeftValue != '.' && char.IsUpper(forwardLeftValue) != isWhitePiece) || 
                    forwardLeftValue == '.' && board.EnPassantTarget == forwardLeft)
                {
                    candidatePositions.Add(forwardLeft);
                }
            }

            // Capture diagonally right, from white perspective
            Position forwardRight = new Position(piecePosition.File + 1, piecePosition.Rank + direction);

            if (forwardRight.File <= 7)
            {
                char forwardRightValue = board.GetPiece(forwardRight);

                // Verify that pawn can capture diagonally right, or en passant is possible
                if ((forwardRightValue != '.' && char.IsUpper(forwardRightValue) != isWhitePiece) ||
                    forwardRightValue == '.' && board.EnPassantTarget == forwardRight)
                {
                    candidatePositions.Add(forwardRight);
                }
            }

            return candidatePositions;
        }

        /// <summary>
        /// Generates candidate moves for a knight.
        /// </summary>
        /// <param name="board">The current state of the chess board.</param>
        /// <param name="piecePosition">The current position of the knight.</param>
        /// <returns>
        /// A list containing valid knight destinations including regular captures.
        /// </returns>
        private List<Position> GetKnightMoves(Board board, Position piecePosition)
        {
            var piece = board.GetPiece(piecePosition);

            // All possible knight movements
            var offsets = new List<(int File, int Rank)> { (-2, -1), (-2, +1), (-1, -2), (-1, +2), (+1, -2), (+1, +2), (+2, -1), (+2, +1) };

            var candidatePositions = new List<Position>();

            // Check each possible movement and see if they would be valid or not
            foreach (var offset in offsets)
            {
                int file = piecePosition.File + offset.File;
                int rank = piecePosition.Rank + offset.Rank;

                // Prevents the creation of moves that would be off the board
                if (file < 0 || file > 7 || rank < 0 || rank > 7)
                    continue;

                var newPosition = new Position(file, rank);
                var destination = board.GetPiece(newPosition);

                // Only adds movements that land the knight on an empty square, or one occupied by an opposing piece
                if (destination == '.' || char.IsUpper(piece) != char.IsUpper(destination))
                    candidatePositions.Add(newPosition);
            }

            return candidatePositions;
        }

        /// <summary>
        /// Generates candidate moves for a rook.
        /// </summary>
        /// <param name="board">The current state of the chess board.</param>
        /// <param name="piecePosition">The current position of the rook.</param>
        /// <returns>
        /// A list containing valid rook destinations including regular captures.
        /// </returns>
        private List<Position> GetRookMoves(Board board, Position piecePosition)
        {
            return GetSlidingMoves(board, piecePosition, new() { (1, 0), (0, 1), (-1, 0), (0, -1) });
        }

        /// <summary>
        /// Generates candidate moves for a bishop.
        /// </summary>
        /// <param name="board">The current state of the chess board.</param>
        /// <param name="piecePosition">The current position of the bishop.</param>
        /// <returns>
        /// A list containing valid bishop destinations including regular captures.
        /// </returns>
        private List<Position> GetBishopMoves(Board board, Position piecePosition)
        {
            return GetSlidingMoves(board, piecePosition, new() { (1, -1), (1, 1), (-1, 1), (-1, -1) });
        }


        /// <summary>
        /// Generates candidate moves for a queen.
        /// </summary>
        /// <param name="board">The current state of the chess board.</param>
        /// <param name="piecePosition">The current position of the queen.</param>
        /// <returns>
        /// A list containing all valid queen destinations including regular captures.
        /// </returns>
        private List<Position> GetQueenMoves(Board board, Position piecePosition)
        {
            return GetSlidingMoves(board, piecePosition, new() { (1, 0), (0, 1), (-1, 0), (0, -1), (1, -1), (1, 1), (-1, 1), (-1, -1) });
        }

        /// <summary>
        /// Generates candidate moves for sliding pieces including rooks, bishops and queens.
        /// </summary>
        /// <param name="board">The current state of the chess board.</param>
        /// <param name="piecePosition">The current position of the sliding piece.</param>
        /// <param name="offsets"> The directions in which the piece can move. </param>
        /// <returns>
        /// A list containing all valid destinations including regular captures.
        /// </returns>
        /// <remarks> 
        /// Movement continues in each direction until the edge of the board 
        /// or another piece is encountered. Friendly pieces block movement, 
        /// while opposing pieces can be captured. 
        /// </remarks>
        private List<Position> GetSlidingMoves(Board board, Position piecePosition, List<(int File, int Rank)> offsets)
        {
            var piece = board.GetPiece(piecePosition);

            var candidatePositions = new List<Position>();

            // Generate all canidate moves for each provided direction, and only stops until it reaches the edge of board, or 
            // encounters a piece. 
            foreach (var offset in offsets)
            {
                for (int distance = 1; distance < 8; distance++)
                {
                    int file = piecePosition.File + offset.File * distance;
                    int rank = piecePosition.Rank + offset.Rank * distance;

                    if (file < 0 || file > 7 || rank < 0 || rank > 7)
                        break;

                    var newPosition = new Position(file, rank);
                    var destination = board.GetPiece(newPosition);

                    if (destination == '.')
                    {
                        candidatePositions.Add(newPosition);
                        continue;
                    }

                    if (char.IsUpper(piece) != char.IsUpper(destination))
                        candidatePositions.Add(newPosition);

                    break;
                }
            }

            return candidatePositions;
        }


        /// <summary>
        /// Generates candidate moves for a king.
        /// </summary>
        /// <param name="board">The current state of the chess board.</param>
        /// <param name="piecePosition">The current position of the king.</param>
        /// <returns>
        /// A list containing all valid king destinations including regular captures and castling destinations where the
        /// castling conditions are met.
        /// </returns>
        private List<Position> GetKingMoves(Board board, Position piecePosition)
        {
            var piece = board.GetPiece(piecePosition);

            var offsets = new List<(int File, int Rank)> { (1, 0), (0, 1), (-1, 0), (0, -1), (1, -1), (1, 1), (-1, 1), (-1, -1) };

            var candidatePositions = new List<Position>();

            // Checks each square in the region of the king to see if they are valid.
            foreach (var offset in offsets)
            {
                int file = piecePosition.File + offset.File;
                int rank = piecePosition.Rank + offset.Rank;

                if (file < 0 || file > 7 || rank < 0 || rank > 7)
                    continue;

                var newPosition = new Position(file, rank);
                var destination = board.GetPiece(newPosition);

                if (destination != Board.Empty && char.IsUpper(piece) == char.IsUpper(destination))
                    continue;

                candidatePositions.Add(newPosition);
            }

            // White queenside castling.
            if (board.IsWhiteTurn && board.CastlingRights.Contains('Q')
                && board.GetPiece(new Position(3, 0)) == '.'
                && board.GetPiece(new Position(2, 0)) == '.'
                && board.GetPiece(new Position(1, 0)) == '.')
            {
                candidatePositions.Add(new Position(2, 0));
            }

            // White kingside castling.
            if (board.IsWhiteTurn && board.CastlingRights.Contains('K')
                && board.GetPiece(new Position(5, 0)) == '.'
                && board.GetPiece(new Position(6, 0)) == '.')
            {
                candidatePositions.Add(new Position(6, 0));
            }

            // Black queenside castling.
            if (!board.IsWhiteTurn && board.CastlingRights.Contains('q')
                && board.GetPiece(new Position(3, 7)) == '.'
                && board.GetPiece(new Position(2, 7)) == '.'
                && board.GetPiece(new Position(1, 7)) == '.')
            {
                candidatePositions.Add(new Position(2, 7));
            }

            // Black kingside castling.
            if (!board.IsWhiteTurn && board.CastlingRights.Contains('k') 
                && board.GetPiece(new Position(5, 7)) == '.' 
                && board.GetPiece(new Position(6, 7)) == '.')
            {
                candidatePositions.Add(new Position(6, 7));
            }

            return candidatePositions;
        }

        /// <summary>
        /// Finds the king belonging to the specified side. 
        /// </summary>
        /// <param name="board">The current state of the chess board.</param>
        /// <param name="isWhitePiece"><c>true</c> to find the white king, <c>false</c> to find</param>
        /// <returns>
        /// The position of the requested king.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the requested king cannot be found on the board.
        /// </exception>
        private Position FindKing(Board board, bool isWhitePiece)
        {
            for (int rank = 7; rank >= 0; rank--)
            {
                for (int file = 0; file < 8; file++)
                {
                    var location = board.GetPiece(new Position(file, rank));

                    if (char.ToLower(location).Equals('k') && char.IsUpper(location) == isWhitePiece)
                        return new Position(file, rank);
                }
            }

            throw new InvalidOperationException("King not found on board.");
        }

        /// <summary>
        /// Removes candidate moves that would leave the moving player's king in check.
        /// </summary>
        /// <param name="board">The current state of the chess board.</param>
        /// <param name="piecePosition">The original position of the moving piece.</param>
        /// <param name="candidateMoves">The candidate moves generated for the piece.</param>
        /// <returns>
        /// A list containing only moves that do not leave the player's king in check.
        /// </returns>
        /// <remarks>
        /// Each candidate move is temporarily applied to a simulated board. The resulting position is then checked 
        /// to determine whether the player's king is attacked. 
        /// </remarks>
        private List<Position> RemoveKingCheckMoves(Board board, Position piecePosition, List<Position> candidateMoves)
        {
            char piece = board.GetPiece(piecePosition);
            bool isWhitePiece = char.IsUpper(piece);

            Position originalKingPosition;
            bool isKing = false;

            if (piece == 'k' || piece == 'K')
            {
                originalKingPosition = piecePosition;
                isKing = true;
            }
            else
            {
                originalKingPosition = FindKing(board, isWhitePiece);
            }

            var legalMoves = new List<Position>();

            foreach (var destination in candidateMoves)
            {
                BoardMoveResult simulatedBoardResult = board.ApplyMove(new Move(Guid.NewGuid(), piecePosition, destination));

                Position kingPosition = isKing ? destination : originalKingPosition;

                if (!IsSquareAttacked(simulatedBoardResult.Board, kingPosition, !isWhitePiece))
                    legalMoves.Add(destination);
            }

            return legalMoves;
        }

        /// <summary>
        /// Determines whether a specified square is currently attacked by a specified side.
        /// </summary>
        /// <param name="board">The current state of the chess board.</param>
        /// <param name="position">The square being checked for possible attacks.</param>
        /// <param name="byWhite"><c>true</c> to check for attacks by white pieces, <c>false</c> to check for attacks by black pieces.</param>
        /// <returns>
        /// <c>true</c> if the square is attacked by the specified side, otherwise, <c>false</c>.
        /// </returns>
        private bool IsSquareAttacked(Board board, Position position, bool byWhite)
        {
            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    var piecePosition = new Position(file, rank);
                    var piece = board.GetPiece(piecePosition);

                    if (piece == Board.Empty)
                        continue;

                    if (char.IsUpper(piece) != byWhite)
                        continue;

                    List<Position> attacks = piece switch
                    {
                        'P' or 'p' => GetPawnAttacks(board, piecePosition),
                        'N' or 'n' => GetKnightMoves(board, piecePosition),
                        'B' or 'b' => GetBishopMoves(board, piecePosition),
                        'R' or 'r' => GetRookMoves(board, piecePosition),
                        'Q' or 'q' => GetQueenMoves(board, piecePosition),
                        'K' or 'k' => GetKingAttackSquares(board, piecePosition),
                        _ => []
                    };

                    if (attacks.Contains(position))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the squares attacked by a specified pawn.
        /// </summary>
        /// <param name="board">The current state of the chess board.</param>
        /// <param name="piecePosition">The current position of the pawn.</param>
        /// <returns>
        /// The diagonal squares attacked by the pawn.
        /// </returns>
        /// <remarks> 
        /// Pawn attacks differ from pawn movement because a pawn attacks diagonally but moves forward can not capture other
        /// pieces. This method therefore does not consider whether the destination contains a piece. 
        /// </remarks>
        private List<Position> GetPawnAttacks(Board board, Position piecePosition)
        {
            char piece = board.GetPiece(piecePosition);

            bool isWhitePiece = char.IsUpper(piece);
            int direction = isWhitePiece ? 1 : -1;

            var attacks = new List<Position>();

            int attackRank = piecePosition.Rank + direction;

            if (attackRank < 0 || attackRank > 7)
                return attacks;

            if (piecePosition.File > 0)
            { 
                attacks.Add(new Position(piecePosition.File - 1, attackRank));
            }

            if (piecePosition.File < 7)
            {
                attacks.Add(new Position(piecePosition.File + 1, attackRank));
            }

            return attacks;
        }

        /// <summary>
        /// Gets all squares directly attacked by a king.
        /// </summary>
        /// <param name="board">The current state of the chess board.</param>
        /// <param name="piecePosition">The current position of the king.</param>
        /// <returns>
        /// A list containing the squares immediately surrounding the king.
        /// </returns>
        /// <remarks>
        /// Castling is intentionally excluded because castling is a move, rather than a square directly attacked by the king. 
        /// </remarks>
        private List<Position> GetKingAttackSquares(Board board, Position piecePosition)
        {
            var offsets = new List<(int File, int Rank)>
            {
                (1, 0),
                (0, 1),
                (-1, 0),
                (0, -1),
                (1, -1),
                (1, 1),
                (-1, 1),
                (-1, -1)
            };

            var attacks = new List<Position>();

            foreach (var offset in offsets)
            {
                int file = piecePosition.File + offset.File;
                int rank = piecePosition.Rank + offset.Rank;

                if (file < 0 || file > 7 || rank < 0 || rank > 7)
                    continue;

                attacks.Add(new Position(file, rank));
            }

            return attacks;
        }

        /// <summary>
        /// Determines whether the current player is in checkmate.
        /// </summary>
        /// <param name="board">The current state of the chess board.</param>
        /// <returns>
        /// c>true</c> if the current player's king is in check and the player has no legal moves, otherwise <c>false</c>.
        /// </returns>
        public bool IsCheckmate(Board board)
        {
            return IsKingInCheck(board) && !HasAnyLegalMove(board);
        }

        /// <summary>
        /// Determines whether the current player is in stalemate.
        /// </summary>
        /// <param name="board">The current state of the chess board.</param>
        /// <returns>
        /// c>true</c> if the current player's king is not in check and the player has no legal moves, otherwise <c>false</c>.
        /// </returns>
        public bool IsStalemate(Board board)
        {
            return !IsKingInCheck(board) && !HasAnyLegalMove(board);
        }

        /// <summary>
        /// Determines whether the current player's king is in check.
        /// </summary>
        /// <param name="board">The current state of the chess board.</param>
        /// <returns>
        /// <c>true</c> if the current player's king is attacked by an opposing piece, otherwise <c>false</c>.
        /// </returns>
        private bool IsKingInCheck(Board board)
        {
            Position? kingPosition = FindKing(board, board.IsWhiteTurn);

            if (kingPosition is null)
                throw new InvalidOperationException("King not found on board.");

            return IsSquareAttacked(board, kingPosition.Value, !board.IsWhiteTurn);
        }


        /// <summary>
        /// Determines whether the current player has at least one legal move.
        /// </summary>
        /// <param name="board">The current state of the chess board.</param>
        /// <returns>
        /// <c>true</c> if the current player has at least one legal move, otherwise <c>false</c>.
        /// </returns>
        private bool HasAnyLegalMove(Board board)
        {
            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    var piecePosition = new Position(file, rank);
                    var piece = board.GetPiece(piecePosition);

                    if (piece == Board.Empty)
                        continue;

                    if (char.IsUpper(piece) != board.IsWhiteTurn)
                        continue;

                    if (GetLegalMoves(board, piecePosition).Count > 0)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether the current board position contains insufficient material to force checkmate.
        /// </summary>
        /// <param name="board">The current state of the chess board.</param>
        /// <returns>
        /// <c>true</c> if the position contains recognised insufficient mating material, otherwise <c>false</c>.
        /// </returns>
        public bool IsInsufficientMaterial(Board board)
        {
            var pieces = new List<(char Piece, Position Position)>();

            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    var position = new Position(file, rank);
                    char piece = board.GetPiece(position);

                    if (piece != Board.Empty)
                        pieces.Add((piece, position));
                }
            }

            // Kings only.
            if (pieces.Count == 2)
                return true;

            // Any pawn, rook or queen means there is mating material.
            if (pieces.Any(p => char.ToLowerInvariant(p.Piece) is 'p' or 'r' or 'q'))
            {
                return false;
            }

            // King and one minor piece versus king.
            if (pieces.Count == 3)
            {
                return pieces.Count(p => char.ToLowerInvariant(p.Piece) is 'b' or 'n') == 1;
            }

            // King and bishop versus king and bishop.
            if (pieces.Count == 4)
            {
                var bishops = pieces
                    .Where(p => char.ToLowerInvariant(p.Piece) == 'b')
                    .ToList();

                if (bishops.Count == 2)
                {
                    bool firstBishopIsWhite = char.IsUpper(bishops[0].Piece);

                    bool secondBishopIsWhite = char.IsUpper(bishops[1].Piece);

                    // Bishops must belong to opposite players.
                    if (firstBishopIsWhite != secondBishopIsWhite)
                    {
                        bool firstSquareIsDark = (bishops[0].Position.File + bishops[0].Position.Rank) % 2 == 1;

                        bool secondSquareIsDark = (bishops[1].Position.File + bishops[1].Position.Rank) % 2 == 1;

                        return firstSquareIsDark == secondSquareIsDark;
                    }
                }
            }

            return false;
        }
    }
}
