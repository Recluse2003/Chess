using Chess.Application.Common.Results;
using Chess.Application.Games.Commands.JoinGame;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace Chess.Web.Pages.Game
{
    public class JoinModel : PageModel
    {
        private readonly IMediator _mediator;

        public JoinModel(IMediator mediator) => _mediator = mediator;

        [BindProperty]
        public string Code { get; set; } = string.Empty;

        public IActionResult OnGet()
        {
            return Page();
        }

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
