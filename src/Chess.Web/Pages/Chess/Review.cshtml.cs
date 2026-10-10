using Chess.Application.Common.Results;
using Chess.Application.Games.Commands.MakeMove;
using Chess.Application.Games.Queries.GetReviewBoardChanges;
using Chess.Application.Games.Queries.ReviewGame;
using Chess.Domain.ValueObjects;
using Chess.Web.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using static Microsoft.CodeAnalysis.CSharp.SyntaxTokenParser;

namespace Chess.Web.Pages.Chess
{
    public class ReviewModel : PageModel
    {
        private readonly IMediator _mediator;

        public ReviewModel(IMediator mediator) => _mediator = mediator;

        public ReviewGameDto? GameDetails { get; private set; }

        public async Task<IActionResult> OnGetAsync([FromRoute] Guid gameId)
        {
            string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (currentUserId == null)
                return Unauthorized();

            ReviewGameQuery query = new(gameId, currentUserId);

            Result<ReviewGameDto> result = await _mediator.Send(query);

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

        public async Task<IActionResult> OnPostReviewMoveAsync([FromBody] ReviewMoveRequest request)
        {
            string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (currentUserId == null)
                return Unauthorized();

            GetReviewBoardChangesQuery query = new(request.GameId, currentUserId, request.CurrentMoveNumber, request.TargetMoveNumber);

            Result<List<BoardChange>> result = await _mediator.Send(query);

            if (!result.IsSuccess)
            {
                return result.Error!.Type switch
                {
                    ErrorType.NotFound => new NotFoundObjectResult(result.Error),
                    ErrorType.Unauthorized => new StatusCodeResult(StatusCodes.Status403Forbidden),
                    ErrorType.Failure => new BadRequestObjectResult(result.Error),
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
