using Chess.Application.Common.Results;
using Chess.Application.Games.Commands.JoinGame;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chess.Web.Pages.Game
{
    public class JoinModel : PageModel
    {
        private readonly IMediator _mediator;

        public JoinModel(IMediator mediator) => _mediator = mediator;

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostCode(string code)
        {
            // string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // if (userId is null)
            //    return Unauthorized();

            JoinGameCommand command = new(code, "whitePlayer");

            Result<Guid> gameId = await _mediator.Send(command);

            return RedirectToPage("/Game/Lobby", new { gameId });
        }
    }
}
