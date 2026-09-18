using Chess.Application.Games.Commands.JoinGame;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chess.Web.Pages.Game
{
    public class JoinModel : PageModel
    {
        private readonly JoinGameCommandHandler _joinGameHandler;

        public JoinModel(JoinGameCommandHandler joinGameHandler)
        {
            _joinGameHandler = joinGameHandler;
        }

        public IActionResult OnGet()
        {
            return Page();
        }
    }
}
