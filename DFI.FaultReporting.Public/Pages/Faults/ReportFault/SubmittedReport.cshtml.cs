using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DFI.FaultReporting.Public.Pages.Faults.ReportFault
{
    public class SubmittedReportModel : PageModel
    {
        public async Task<IActionResult> OnGetAsync()
        {
            //Clear session to ensure fresh start.
            HttpContext.Session.Clear();

            //Return the page.
            return Page();
        }
    }
}
