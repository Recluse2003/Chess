using Chess.Application.Common.Results;
using Chess.Application.Games.Commands.CreateGame;
using Chess.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace Chess.Web.Pages.Game
{
    /// <summary>
    /// Handles requests for creating a new <see cref="ChessGame"/>.
    /// </summary>
    public class CreateModel : PageModel
    {
        private readonly IMediator _mediator;

        public CreateModel(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Handles the initial GET request for the create game page.
        /// </summary>
        /// <returns>
        /// The create game page.
        /// </returns>
        public IActionResult OnGet()
        {
            return Page();
        }

        /// <summary>
        /// Handles the form submission for creating a new chess game.
        /// </summary>
        /// <returns>
        /// A redirect to the game lobby when the game is created successfully, otherwise, the current page with 
        /// the creation error displayed.
        /// </returns>
        public async Task<IActionResult> OnPostAsync()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId is null)
                return Unauthorized();

            var command = new CreateGameCommand(userId);

            Result<CreateGameDto> result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Error!.Description);

                return Page();
            }

            return RedirectToPage("/Game/Lobby", new { gameId = result.Value.GameId });
        }
    }
}
