using Chess.Application.Games.Commands.CreateGame;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace Chess.Web.Pages.Game
{
    public class CreateModel : PageModel
    {
        private readonly CreateGameCommandHandler _createGameHandler;

        public CreateModel(CreateGameCommandHandler createGameHandler)
        {
            _createGameHandler = createGameHandler;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // if (userId is null)
            //    return Unauthorized();

            var command = new CreateGameCommand("whitePlayer");

            Guid gameId = await _createGameHandler.ExecuteAsync(command);

            return RedirectToPage("/Chess/Index", new { gameId });
        }
    }
}
