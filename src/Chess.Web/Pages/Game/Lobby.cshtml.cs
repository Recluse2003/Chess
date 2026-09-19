using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MediatR;

namespace Chess.Web.Pages.Game
{
    public class LobbyModel : PageModel
    {
        private readonly IMediator _mediator;

        public LobbyModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            return Page();
        }
    }
}