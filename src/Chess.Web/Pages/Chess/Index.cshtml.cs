using Chess.Application.Common.Results;
using Chess.Application.Games.Commands.MakeMove;
using Chess.Application.Games.Queries.GetLegalMoves;
using Chess.Application.Games.Queries.ViewGame;
using Chess.Domain.Entities;
using Chess.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace Chess.Web.Pages.Chess
{
    /// <summary> 
    /// Handles requests for the chess game page, including retrieving the current game state and processing 
    /// chess moves.
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly IMediator _mediator;

        public IndexModel(IMediator mediator) => _mediator = mediator;

        public ViewGameDto? GameDetails { get; private set; }

        /// <summary>
        /// Handles the initial GET request for the chess game page.
        /// </summary>
        /// <param name="gameId">The unique identifier of the <see cref="ChessGame"/>.</param>
        /// <returns>
        /// The chess game page when the game can be loaded, otherwise, an appropriate error or 
        /// access-denied response.
        /// </returns>
        public async Task<IActionResult> OnGetAsync([FromRoute] Guid gameId)
        {
            string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (currentUserId == null)
               return Unauthorized();

            ViewGameQuery query = new(gameId, currentUserId);

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

        /// <summary>
        /// Retrieves the legal moves for a piece at the specified board position.
        /// </summary>
        /// <param name="gameId">The unique identifier of the <see cref="ChessGame"/>.</param>
        /// <param name="file">The zero-based file of the selected piece.</param>
        /// <param name="rank">The zero-based rank of the selected piece.</param>
        /// <returns>
        /// A JSON response containing the legal moves.
        /// </returns>
        public async Task<IActionResult> OnGetPossibleMovesAsync(Guid gameId, int file, int rank)
        {
            string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (currentUserId == null)
               return Unauthorized();

            GetLegalMovesQuery command = new(gameId, currentUserId, new Position(file, rank));

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
            string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (currentUserId == null)
                return Unauthorized();

            MakeMoveCommand command = new(
                request.GameId,
                currentUserId,
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
