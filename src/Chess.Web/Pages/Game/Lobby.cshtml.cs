using Chess.Application.Common.Results;
using Chess.Application.Games.Queries.GetGameLobby;
using Chess.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace Chess.Web.Pages.Game
{
    /// <summary>
    /// Handles requests for displaying the lobby of a chess game.
    /// </summary>
    public class LobbyModel : PageModel
    {
        private readonly IMediator _mediator;

        public LobbyModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        public GameLobbyDto Lobby { get; set; } = null!;


        /// <summary>
        /// Handles the initial GET request for a <see cref="ChessGame"/> lobby.
        /// </summary>
        /// <param name="gameId">The unique identifier of the <see cref="ChessGame"/> whose lobby should 
        /// be displayed.</param>
        /// <returns>
        /// The lobby page when the user is authorised to access it, otherwise,  an appropriate error or 
        /// access-denied page.
        /// </returns>
        public async Task<IActionResult> OnGetAsync([FromRoute] Guid gameId)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId is null)
                return Unauthorized();

            GetGameLobbyQuery query = new(gameId, userId);

            Result<GameLobbyDto> result = await _mediator.Send(query);

            if (!result.IsSuccess)
            {
                return result.Error!.Type switch
                {
                    ErrorType.NotFound => RedirectToPage("/NotFound"),
                    ErrorType.Unauthorized => RedirectToPage("/AccessDenied"),
                    _ => RedirectToPage("/Error")
                };
            }

            Lobby = result.Value;

            return Page();
        }
    }
}