using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DFI.FaultReporting.Public.Pages.Faults.ReportFault
{
    public class ReportFaultModel : PageModel
    {
        public async Task<IActionResult> OnGetAsync()
        {
            //Clear session to ensure fresh start.
            HttpContext.Session.Clear();

            //Return the page.
            return Page();
        }

        //Method Summary:
        //This method is executed when the begin button is clicked.
        //When executed the user is redirected to Step1.
        public async Task<IActionResult> OnPostBegin()
        {
            return Redirect("/Faults/ReportFault/Step1");
        }
    }
}
