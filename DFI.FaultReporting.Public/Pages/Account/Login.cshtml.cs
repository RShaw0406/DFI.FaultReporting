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
using System.ComponentModel;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

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
        public LoginInputModel loginInput { get; set; }

        [BindProperty]
        public VerificationCodeModel verificationCodeInput { get; set; }

        [BindProperty]
        public bool verificationCodeSent { get; set; }

        public class LoginInputModel
        {
            [Required]
            [EmailAddress]
            public string? Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string? Password { get; set; }
        }

        public class VerificationCodeModel
        {
            [Required]
            [DisplayName("Verification Code")]
            public string? VerificationCode { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            TempData.Clear();

            if (HttpContext.User.Identity.IsAuthenticated == true)
            {
                await HttpContext.SignOutAsync();
            }          

            return Page();
        }

        public async Task<IActionResult> OnPostRequestVerificationCode()
        {
            if (loginInput.Email != null && loginInput.Password != null)
            {
                LoginRequest loginRequest = new LoginRequest();
                loginRequest.Email = loginInput.Email;
                loginRequest.Password = loginInput.Password;

                AuthResponse authResponse = await _userService.Login(loginRequest);

                if (authResponse != null)
                {
                    TempData["LoginEmail"] = loginInput.Email;
                    TempData["LoginPassword"] = loginInput.Password;
                    TempData["UserID"] = authResponse.UserID;
                    TempData["UserName"] = authResponse.UserName;
                    TempData["JWTToken"] = authResponse.Token;

                    int verficationToken = await _verificationTokenService.GenerateToken();

                    Response emailResponse = await SendVerificationCode(loginInput.Email, verficationToken);

                    if (emailResponse.IsSuccessStatusCode)
                    {
                        verificationCodeSent = true;

                        TempData["VerificationToken"] = verficationToken;
                        TempData["VerificationCodeSent"] = verificationCodeSent;
                        TempData.Keep();
                    }
                    else
                    {
                        verificationCodeSent = false;

                        TempData.Keep();

                        ModelState.AddModelError(string.Empty, "There was a problem sending the verification code");
                        return Page();
                    }
                }
                else
                {
                    TempData.Clear();

                    ModelState.AddModelError(string.Empty, "Invalid login attempt");
                    return Page();
                }
            }
            TempData.Keep();
            return Page();
        }

        public async Task<IActionResult> OnPostLogin()
        {
            verificationCodeSent = Boolean.Parse(TempData["VerificationCodeSent"].ToString());

            if (verificationCodeInput.VerificationCode != null)
            {
                if (verificationCodeInput.VerificationCode == TempData["VerificationToken"].ToString())
                {
                    ClaimsPrincipal jwtClaimsPrincipal = await ValidateJWTToken(TempData["JWTToken"].ToString());

                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, TempData["UserID"].ToString()));
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, TempData["UserName"].ToString()));
                    claimsIdentity.AddClaim(new Claim("Token", TempData["JWTToken"].ToString()));

                    foreach (Claim claim in jwtClaimsPrincipal.Claims)
                    {
                        claimsIdentity.AddClaim(claim);
                    }

                    ClaimsPrincipal currentUser = new ClaimsPrincipal();
                    currentUser.AddIdentity(claimsIdentity);

                    Claim expiresClaim = currentUser.FindFirst("exp");

                    long ticks = long.Parse(expiresClaim.Value);

                    DateTime? Expires = DateTimeOffset.FromUnixTimeSeconds(ticks).LocalDateTime;

                    AuthenticationProperties authenticationProperties = new AuthenticationProperties();

                    authenticationProperties.ExpiresUtc = Expires;

                    HttpContext.User = currentUser;

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, currentUser, authenticationProperties);

                    TempData.Clear();

                    _logger.LogInformation("User logged in.");
                    return Redirect("/Index");
                }
                else 
                {                
                    TempData.Keep();

                    ModelState.AddModelError(string.Empty, "Invalid Verification Code");
                    return Page();
                }
            }
            TempData.Keep();
            return Page();
        }

        public async Task<Response> SendVerificationCode(string emailAddress, int verficationToken)
        {         
            string subject = "DFI Fault Reporting: Verification Code";
            EmailAddress to = new EmailAddress(emailAddress);
            string textContent = string.Empty;
            string htmlContent = "<p>Hello,</p><p>Below is your verification code, do not share this code with anyone.</p><br /><p>Verification Code:</p><br /><strong>" + verficationToken.ToString() + "</strong>";
            Attachment? attachment = null;

            return await _emailService.SendEmail(subject, to, textContent, htmlContent, attachment);
        }

        public async Task<ClaimsPrincipal> ValidateJWTToken(string token)
        {
            string issuer = await _settingsService.GetSettingString(DFI.FaultReporting.Common.Constants.Settings.JWTISSUER);
            string audience = await _settingsService.GetSettingString(DFI.FaultReporting.Common.Constants.Settings.JWTAUDIENCE);

            SecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(await _settingsService.GetSettingString(DFI.FaultReporting.Common.Constants.Settings.JWTKEY)));

            SecurityToken tokenValidated;
            TokenValidationParameters tokenValidationParameters = new TokenValidationParameters();

            tokenValidationParameters.ValidateLifetime = true;
            tokenValidationParameters.ValidAudience = audience;
            tokenValidationParameters.ValidIssuer = issuer;
            tokenValidationParameters.IssuerSigningKey = key;

            ClaimsPrincipal claimsPrincipal = new JwtSecurityTokenHandler().ValidateToken(token, tokenValidationParameters, out tokenValidated);

            return claimsPrincipal;
        }
    }
}