using Chess.Application.Games.Commands.JoinGame;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chess.Web.Pages.Game
{
    public class JoinModel : PageModel
    {
        private readonly IMediator _mediator;

        public JoinModel(IMediator mediator) => _mediator = mediator;

        public IActionResult OnGet()
        {
            return Page();
        }
    }
}
