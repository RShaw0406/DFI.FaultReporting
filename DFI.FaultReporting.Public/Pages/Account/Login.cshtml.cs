using DFI.FaultReporting.JWT.Requests;
using DFI.FaultReporting.JWT.Response;
using DFI.FaultReporting.Models.Roles;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Roles;
using DFI.FaultReporting.Services.Interfaces.Users;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using SendGrid.Helpers.Mail;
using SendGrid;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Emails;
using DFI.FaultReporting.Services.Interfaces.Tokens;

namespace DFI.FaultReporting.Public.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IUserService _userService;
        private IUserRoleService _userRoleService;
        private IRoleService _roleService;
        private readonly ILogger<LoginModel> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISettingsService _settingsService;
        private readonly IEmailService _emailService;
        private readonly IVerificationTokenService _verificationTokenService;

        public LoginModel(IUserService userService, IUserRoleService userRoleService, IRoleService roleService, ILogger<LoginModel> logger, IHttpContextAccessor httpContextAccessor, 
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

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {             
                LoginRequest loginRequest = new LoginRequest();
                loginRequest.Email = Input.Email;
                loginRequest.Password = Input.Password;

                AuthResponse authResponse = await _userService.Login(loginRequest);

                if (authResponse != null)
                {
                    List<User> users = await _userService.GetUsers(authResponse.Token);

                    User user = users.Where(u => u.ID == authResponse.UserID).FirstOrDefault();

                    List<UserRole> UserRoles = await _userRoleService.GetUserRoles();

                    List<Role> Roles = await _roleService.GetRoles();

                    List<UserRole> assignedUserRoles = UserRoles.Where(ur => ur.UserID == user.ID).ToList();

                    List<Role> assignedRoles = new List<Role>();

                    foreach (UserRole userRole in assignedUserRoles)
                    {
                        Role role = Roles.Where(r => r.ID == userRole.RoleID).FirstOrDefault();

                        assignedRoles.Add(role);
                    }

                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, authResponse.UserID.ToString()));
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, authResponse.UserName));
                    claimsIdentity.AddClaim(new Claim("Token", authResponse.Token));

                    if (assignedRoles != null)
                    {
                        foreach (Role role in assignedRoles)
                        {
                            claimsIdentity.AddClaim((new Claim(ClaimTypes.Role, role.RoleDescription)));
                        }
                    }

                    ClaimsPrincipal currentUser = new ClaimsPrincipal();
                    currentUser.AddIdentity(claimsIdentity);

                    HttpContext.User = currentUser;

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, currentUser);

                    Response emailResponse = await SendEmail(loginRequest.Email);

                    if (emailResponse.IsSuccessStatusCode)
                    {

                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "There was a problem sending the verification code");
                        return Page();
                    }

                    _logger.LogInformation("User logged in.");
                    return Redirect("/Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt");
                    return Page();
                }
            }
            return Page();
        }

        public async Task<Response> SendEmail(string emailAddress)
        {
            int verficationToken = await _verificationTokenService.GenerateToken();

            string subject = "DFI Fault Reporting: Verification Code";
            EmailAddress to = new EmailAddress(emailAddress);
            string textContent = string.Empty;
            string htmlContent = "<p>Hello, below is your verification code, do not share this code with anyone.</p><br /><p>Verification Code:</p><br /><strong>" + verficationToken.ToString() + "</strong>";
            Attachment? attachment = null;

            return await _emailService.SendEmail(subject, to, textContent, htmlContent, attachment);
        }
    }
}