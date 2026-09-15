using Chess.Application.Games.CreateGame;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace Chess.Web.Pages.CreateGame
{
    public class Index : PageModel
    {
        private readonly CreateGameCommandHandler _createGameHandler;

        public Index(CreateGameCommandHandler createGameHandler)
        {
            _createGameHandler = createGameHandler;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId is null)
                return Unauthorized();

            var command = new CreateGameCommand(userId);

            Guid gameId = await _createGameHandler.ExecuteAsync(command);

            return RedirectToPage("/Chess/Index", new { id = gameId });
        }
    }
}
