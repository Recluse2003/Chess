using Chess.Application.Common.Results;
using Chess.Application.Games.Commands.MakeMove;
using Chess.Application.Games.Queries.GetLegalMoves;
using Chess.Application.Games.Queries.ViewGame;
using Chess.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chess.Web.Pages.Chess
{
    public class IndexModel : PageModel
    {
        private readonly IMediator _mediator;

        public IndexModel(IMediator mediator) => _mediator = mediator;

        public ViewGameDto? GameDetails { get; private set; }

        public async Task<IActionResult> OnGetAsync([FromRoute] Guid gameId)
        {
            // string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // if (currentUserId == null)
            //    return Unauthorized();

            ViewGameQuery query = new(gameId, "whitePlayer");

            Result<ViewGameDto> result = await _mediator.Send(query);

            if (!result.IsSuccess)
            {
                return result.Error!.Type switch
                {
                    ErrorType.NotFound => RedirectToPage("/NotFound"), 
                    ErrorType.Unauthorized => RedirectToPage("/AccessDenied"), 
                    _ => RedirectToPage("/Error") 
                };
            }

            GameDetails = result.Value;

            return Page();
        }

        public async Task<IActionResult> OnGetPossibleMovesAsync(Guid gameId, int file, int rank)
        {
            // string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // if (currentUserId == null)
            //    return Unauthorized();

            GetLegalMovesQuery command = new(gameId, "whitePlayer", new Position(file, rank));

            Result<List<LegalMoveDto>> result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return result.Error!.Type switch
                {
                    ErrorType.NotFound => new NotFoundObjectResult(result.Error),
                    ErrorType.Unauthorized => new StatusCodeResult(StatusCodes.Status403Forbidden),
                    _ => new BadRequestObjectResult(result.Error)
                };
            }

            return new JsonResult(result.Value);
        }

        public async Task<IActionResult> OnPostMoveAsync([FromBody] MakeMoveRequest request)
        {
            // string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // if (currentUserId == null)
            //    return Unauthorized();

            MakeMoveCommand command = new(
                request.GameId,
                "userId",
                new Position(request.FromFile, request.FromRank),
                new Position(request.ToFile, request.ToRank),
                request.PromotionPiece);

            Result<MoveResultDto> result = await _mediator.Send(command);

            if (!result.IsSuccess) 
            {
                return result.Error!.Type switch
                {
                    ErrorType.NotFound => new NotFoundObjectResult(result.Error),
                    ErrorType.Conflict => new ConflictObjectResult(result.Error),
                    ErrorType.Unauthorized => new StatusCodeResult(StatusCodes.Status403Forbidden),
                    ErrorType.Validation => new BadRequestObjectResult(result.Error),
                    _ => new BadRequestObjectResult(result.Error)
                };
            }

            return new JsonResult(result.Value);
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
