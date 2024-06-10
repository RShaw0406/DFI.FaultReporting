using DFI.FaultReporting.Services.Interfaces.Emails;
using DFI.FaultReporting.Services.Interfaces.Roles;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Tokens;
using DFI.FaultReporting.Services.Interfaces.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DFI.FaultReporting.Public.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly IUserService _userService;
        private IUserRoleService _userRoleService;
        private IRoleService _roleService;
        private readonly ILogger<LoginModel> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISettingsService _settingsService;
        private readonly IEmailService _emailService;
        private readonly IVerificationTokenService _verificationTokenService;

        public LogoutModel(IUserService userService, IUserRoleService userRoleService, IRoleService roleService, ILogger<LoginModel> logger, IHttpContextAccessor httpContextAccessor,
            ISettingsService settingsService, IEmailService emailService, IVerificationTokenService verificationTokenService)
        {
            _userService = userService;
            _userRoleService = userRoleService;
            _roleService = roleService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _settingsService = settingsService;
            _emailService = emailService;
            _verificationTokenService = verificationTokenService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostLogout()
        {
            TempData.Clear();

            if (HttpContext.User.Identity.IsAuthenticated == true)
            {
                await HttpContext.SignOutAsync();

                _logger.LogInformation("User logged in.");          
            }

            return Redirect("/Index");
        }

        public async Task<IActionResult> OnPostCancel()
        {
            return Redirect("/Index");
        }
    }
}
