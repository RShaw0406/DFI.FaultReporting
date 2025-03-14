using DFI.FaultReporting.Services.Interfaces.Emails;
using DFI.FaultReporting.Services.Interfaces.Roles;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Tokens;
using DFI.FaultReporting.Services.Interfaces.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DFI.FaultReporting.Admin.Pages.Account
{
    public class LogoutModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly IStaffService _staffService;
        private IStaffRoleService _staffRoleService;
        private IRoleService _roleService;
        private readonly ILogger<LoginModel> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISettingsService _settingsService;
        private readonly IEmailService _emailService;
        private readonly IVerificationTokenService _verificationTokenService;

        //Inject dependencies in constructor.
        public LogoutModel(IStaffService staffService, IStaffRoleService staffRoleService, IRoleService roleService, ILogger<LoginModel> logger, IHttpContextAccessor httpContextAccessor,
            ISettingsService settingsService, IEmailService emailService, IVerificationTokenService verificationTokenService)
        {
            _staffService = staffService;
            _staffRoleService = staffRoleService;
            _roleService = roleService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _settingsService = settingsService;
            _emailService = emailService;
            _verificationTokenService = verificationTokenService;
        }
        #endregion Dependency Injection

        #region Page Load
        //Method Summary:
        //This method is executed when the page is loaded.
        //When executed it returns the page.
        public async Task<IActionResult> OnGetAsync()
        {
            return Page();
        }
        #endregion Page Load

        #region Logout
        //Method Summary:
        //This method is executed when the user clicks the logout button.
        //When executed it clears the TempData and logs the user out.
        public async Task<IActionResult> OnPostLogout()
        {
            //Clear TempData.
            TempData.Clear();

            //Check if the user is authenticated.
            if (HttpContext.User.Identity.IsAuthenticated == true)
            {
                //Log the user out.
                await HttpContext.SignOutAsync();

                _logger.LogInformation("User logged in.");
            }

            return Redirect("/Index");
        }

        //Method Summary:
        //This method is executed when the user clicks the cancel button.
        //When executed it returns the user to the index page.
        public async Task<IActionResult> OnPostCancel()
        {
            return Redirect("/Index");
        }
        #endregion Logout
    }
}
