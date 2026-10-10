using Chess.Application.Common.Results;
using Chess.Application.Games.Queries.GetGamesByUserId;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace Chess.Web.Pages.Games
{
    public class ViewModel : PageModel
    {
        private readonly IMediator _mediator;

        public ViewModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1; // Default to page 1

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10; // Default to 10 items per page

        public PagedList<GameDto>? PaginatedGames { get; private set; }
        public string? ErrorMessage { get; private set; }

        public async Task<IActionResult> OnGetAsync()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId is null)
                return Unauthorized();

            if (PageNumber < 1) 
                PageNumber = 1;

            if (PageSize < 1) 
                PageSize = 10;

            var query = new GetGamesByUserIdQuery(userId, PageNumber, PageSize);
            Result<PagedList<GameDto>> result = await _mediator.Send(query);

            if (result.IsSuccess)
            {
                PaginatedGames = result.Value;
            }
            else
            {
                ErrorMessage = result.Error?.Description ?? "An error occurred fetching your games.";
            }

            return Page();
        }
    }
}
