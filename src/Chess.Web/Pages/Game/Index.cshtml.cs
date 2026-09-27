using Chess.Application.Common.Results;
using Chess.Application.Games.Commands.CreateGame;
using Chess.Application.Games.Commands.JoinGame;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace Chess.Web.Pages.Game
{
    public class IndexModel : PageModel
    {
        private readonly IMediator _mediator;

        public IndexModel(IMediator mediator) => _mediator = mediator;

        [BindProperty]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Handles the initial GET request for the join game page.
        /// </summary>
        /// <returns>
        /// The join game page.
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
        public async Task<IActionResult> OnPostCreateAsync()
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

        /// <summary>
        /// Handles submission of the lobby code and attempts to join the associated chess game.
        /// </summary>
        /// <returns>
        /// A redirect to the game lobby when the user joins successfully, otherwise, the current
        /// page with the join error displayed.
        /// </returns>
        public async Task<IActionResult> OnPostCode()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId is null)
                return Unauthorized();

            JoinGameCommand command = new(Code, userId);

            Result<Guid> result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError("code", result.Error!.Description);

                return Page();
            }

            return RedirectToPage("/Game/Lobby", new { gameId = result.Value });
        }
    }
}
