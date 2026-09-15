using Chess.Application.Games.ViewGame;
using Chess.Domain.Entities;
using Chess.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace Chess.Web.Pages.Chess
{
    public class IndexModel : PageModel
    {
        private readonly ViewGameQueryHandler _queryHandler;

        public IndexModel(ViewGameQueryHandler queryHandler)
        {
            _queryHandler = queryHandler;
        }

        public ViewGameDto? Game { get; private set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (currentUserId == null)
                return Unauthorized();

            Game = await _queryHandler.ExecuteAsync(new ViewGameQuery(id, currentUserId));

            if (Game == null)
                return NotFound();

            return Page();
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
