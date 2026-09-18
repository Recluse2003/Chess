using Chess.Application.Games.Commands.MakeMove;
using Chess.Application.Games.Queries.GetLegalMoves;
using Chess.Application.Games.Queries.ViewGame;
using Chess.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace Chess.Web.Pages.Chess
{
    public class IndexModel : PageModel
    {
        private readonly ViewGameQueryHandler _viewGameQueryHandler;
        private readonly GetLegalMovesQueryHandler _getLegalMovesQueryHandler;
        private readonly MakeMoveCommandHandler _makeMoveCommandHandler;


        public IndexModel(
            ViewGameQueryHandler viewGameQueryHandler, 
            GetLegalMovesQueryHandler getLegalMovesQueryHandler,
            MakeMoveCommandHandler makeMoveCommandHandler)
        {
            _viewGameQueryHandler = viewGameQueryHandler;
            _getLegalMovesQueryHandler = getLegalMovesQueryHandler;
            _makeMoveCommandHandler = makeMoveCommandHandler;
        }

        public ViewGameDto? Game { get; private set; }

        public async Task<IActionResult> OnGetAsync(Guid gameId)
        {
            // string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // if (currentUserId == null)
            //    return Unauthorized();

            Game = await _viewGameQueryHandler.ExecuteAsync(new ViewGameQuery(gameId, "whitePlayer"));

            if (Game == null)
                return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnGetPossibleMovesAsync(Guid gameId, int file, int rank)
        {
            // string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // if (currentUserId == null)
            //    return Unauthorized();

            List<LegalMoveDto> moves = await _getLegalMovesQueryHandler.ExecuteAsync(
                new GetLegalMovesQuery(
                    gameId,
                    "whitePlayer", 
                    new Position(file, rank)));

            return new JsonResult(moves);
        }

        public async Task<IActionResult> OnPostMoveAsync([FromBody] MakeMoveRequest request)
        {
            // string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // if (currentUserId == null)
            //    return Unauthorized();

            MakeMoveCommand command = new MakeMoveCommand(
                request.GameId,
                "whitePlayer",
                new Position(request.FromFile, request.FromRank),
                new Position(request.ToFile, request.ToRank),
                request.PromotionPiece);

            MoveResultDto result = await _makeMoveCommandHandler.ExecuteAsync(command);

            return new JsonResult(result);
        }

        public string GetPieceSymbol(char piece)
        {
            return piece switch
            {
                'K' => "♔",
                'Q' => "♕",
                'R' => "♖",
                'B' => "♗",
                'N' => "♘",
                'P' => "♙",

                'k' => "♚",
                'q' => "♛",
                'r' => "♜",
                'b' => "♝",
                'n' => "♞",
                'p' => "♟",

                _ => string.Empty
            };
        }
    }
}
