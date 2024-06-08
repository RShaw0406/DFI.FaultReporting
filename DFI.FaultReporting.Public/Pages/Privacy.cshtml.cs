using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace DFI.FaultReporting.Public.Pages
{
    public class PrivacyModel : PageModel
    {
        private readonly ILogger<PrivacyModel> _logger;

        public PrivacyModel(ILogger<PrivacyModel> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            TempData.Keep();

            if (TempData["isAuth"] != null)
            {
                ViewData["isAuth"] = true;
                ViewData["UserName"] = TempData["UserName"];
            }

            return Page();
        }
    }
}
