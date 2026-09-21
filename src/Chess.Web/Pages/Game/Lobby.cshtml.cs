using Chess.Application.Common.Results;
using Chess.Application.Games.Commands.StartGame;
using Chess.Application.Games.Queries.GetGameLobby;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace Chess.Web.Pages.Game
{
    public class LobbyModel : PageModel
    {
        private readonly IMediator _mediator;

        public LobbyModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        public GameLobbyDto Lobby { get; set; } = null!;

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