using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Docusign_Connect.Pages
{
    [Authorize]
    public class ClaimsModel : PageModel
    {
        private readonly ILogger<ClaimsModel> _logger;

        public ClaimsModel(ILogger<ClaimsModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
    }
}