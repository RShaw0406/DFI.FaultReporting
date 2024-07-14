using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DFI.FaultReporting.Public.Pages.Claims.SubmitClaim
{
    public class Step2Model : PageModel
    {
        public void OnGet()
        {
        }

        #region Page Buttons
        //Method Summary:
        //This method is executed when the back button is clicked.
        //When executed the user is redirected to SubmitClaim page.
        public async Task<IActionResult> OnPostBack()
        {
            return Redirect("/Claims/SubmitClaim/Step1");
        }
        #endregion Page Buttons
    }
}
