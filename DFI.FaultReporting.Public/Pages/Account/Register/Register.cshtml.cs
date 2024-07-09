using DFI.FaultReporting.JWT.Requests;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Emails;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Tokens;
using DFI.FaultReporting.Services.Interfaces.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using DFI.FaultReporting.JWT.Response;
using Microsoft.AspNetCore.Identity.Data;
using SendGrid;
using SendGrid.Helpers.Mail;
using DFI.FaultReporting.Common.SessionStorage;
using static DFI.FaultReporting.Public.Pages.Account.DetailsModel;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Models.FaultReports;

namespace DFI.FaultReporting.Public.Pages.Account.Register
{
    public class RegisterModel : PageModel
    {
        #region Page Load
        //Method Summary:
        //This method is executed when the page is loaded.
        //When executed the session and TempData are cleared, if the user is already authenticated they are redirected to the index page.
        public async Task<IActionResult> OnGetAsync()
        {
            //Clear session to ensure fresh start.
            HttpContext.Session.Clear();

            //Clear TempData to ensure fresh start.
            TempData.Clear();

            //The HttpContext user is already authenticated.
            if (HttpContext.User.Identity.IsAuthenticated == true)
            {
                //Redirect to the index page.
                Redirect("./Index");
            }

            //Return the page.
            return Page();
        }
        #endregion Page Load

        #region Begin Button
        //Method Summary:
        //This method is executed when the begin button is clicked.
        //When executed the user is redirected to Step1.
        public async Task<IActionResult> OnPostBegin()
        {
            return Redirect("/Account/Register/Step1");
        }
        #endregion Begin Button
    }
}
