using Chess.Application.Common.Results;
using Chess.Application.Games.Commands.CreateGame;
using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chess.Web.Pages.Game
{
    public class CreateModel : PageModel
    {
        private readonly IMediator _mediator;

        public CreateModel(IMediator mediator) => _mediator = mediator;

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

            Result<CreateGameDto> result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Error!.Description);

                return Page();
            }

            return RedirectToPage("/Chess/Index", new { gameId = result.Value.gameId, joinCode = result.Value.joinCode });
        }
    }
}
