using Chess.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chess.Web.Pages.Game
{
    public class LobbyModel : PageModel
    {
        private readonly IChessGameRepository _gameRepository;

        public LobbyModel(IChessGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            return Page();
        }
    }
}