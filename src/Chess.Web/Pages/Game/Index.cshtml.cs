using Chess.Application.Common.Results;
using Chess.Application.Games.Commands.CreateGame;
using Chess.Application.Games.Commands.JoinGame;
using Chess.Application.Games.Queries.GetRandomPublicGames;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace Chess.Web.Pages.Game
{
    [EnableRateLimiting("chess_game_creation_and_join")]
    public class IndexModel : PageModel
    {
        private readonly IMediator _mediator;

        public IndexModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        public List<PublicGameDto> PublicGames { get; set; } = new();

        [BindProperty]
        public CreateGameViewModel CreateGameViewModel { get; set; } = new();

        [BindProperty]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Handles the initial GET request for the join game page.
        /// </summary>
        /// <returns>
        /// The join game page.
        /// </returns>
        public async Task<IActionResult> OnGetAsync()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId is null)
                return Unauthorized();

            Result<List<PublicGameDto>> result = await _mediator.Send(new GetRandomPublicGamesQuery(userId, 10));

            PublicGames = result.Value;

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

            CreateGameCommand command = new(userId, CreateGameViewModel.IsPrivate);

            Result<CreateGameDto> result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(nameof(CreateGameViewModel), result.Error!.Description);

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
        public async Task<IActionResult> OnPostCodeAsync()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId is null)
                return Unauthorized();

            JoinGameCommand command = new(Code, userId);

            Result<Guid> result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(Code, result.Error!.Description);

                return Page();
            }

            return RedirectToPage("/Game/Lobby", new { gameId = result.Value });
        }
    }
}