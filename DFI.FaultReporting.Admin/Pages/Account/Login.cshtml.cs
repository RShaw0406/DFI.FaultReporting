using DFI.FaultReporting.Services.Interfaces.Emails;
using DFI.FaultReporting.Services.Interfaces.Roles;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Tokens;
using DFI.FaultReporting.Services.Interfaces.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.AspNetCore.Authentication;
using DFI.FaultReporting.JWT.Response;
using DFI.FaultReporting.Services.Users;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using SendGrid.Helpers.Mail;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SendGrid;
using DFI.FaultReporting.JWT.Requests;

namespace DFI.FaultReporting.Admin.Pages.Account
{
    public class LoginModel : PageModel
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
        public LoginModel(IStaffService staffService, IStaffRoleService staffRoleService, IRoleService roleService, ILogger<LoginModel> logger, IHttpContextAccessor httpContextAccessor,
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

        #region Properties
        //Declare LoginInputModel property, this is needed when requesting login to API.
        [BindProperty]
        public LoginInputModel loginInput { get; set; }

        //Declare VerificationCodeInput property, this is needed when inputting sent verification code when logging in.
        [BindProperty]
        public VerificationCodeModel verificationCodeInput { get; set; }

        //Declare verificationCodeSent property, this is needed for displaying the section for inputting the verification code.
        [BindProperty]
        public bool verificationCodeSent { get; set; }

        //Declare IncorrectAttempts property, this is needed for counting invalid login attempts by users.
        public int? IncorrectAttempts { get; set; }

        //Declare LoginInputModel class, this is needed when requesting login to API.
        public class LoginInputModel
        {
            [Required]
            [EmailAddress]
            public string? Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string? Password { get; set; }
        }

        //Declare VerificationCodeModel class, this is needed when inputting sent verification code when logging in.
        public class VerificationCodeModel
        {
            [Required]
            [DisplayName("Verification code")]
            public string? VerificationCode { get; set; }
        }
        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is executed when the page loads and is used for clearing TempData to ensure fresh start along with ensuring that no user is logged in.
        public async Task<IActionResult> OnGetAsync()
        {
            //Clear session to ensure fresh start.
            HttpContext.Session.Clear();

            //Clear TempData to ensure fresh start.
            TempData.Clear();

            //The HttpContext user is already authenticated.
            if (HttpContext.User.Identity.IsAuthenticated == true)
            {
                return RedirectToPage("/Index");
            }

            //Redirect to already logged in page.
            await HttpContext.SignOutAsync();

            //Return the Page.
            return Page();
        }
        #endregion Page Load

        #region Login
        //Method Summary:
        //This method is excuted when the "Request verification code" button is clicked on the "Enter your login details" section.
        //When excuted the "Enter your verification code" section is displayed.
        public async Task<IActionResult> OnPostRequestVerificationCode()
        {
            //User has entered both their email and password.
            if (loginInput.Email != null && loginInput.Password != null)
            {
                //Create a new LoginRequest to pass to auth controller, populate with entered email and password.
                LoginRequest loginRequest = new LoginRequest();
                loginRequest.Email = loginInput.Email;
                loginRequest.Password = loginInput.Password;

                //Get an AuthRepsonse by calling the Login method in the _staffService.
                //This will authenticate the staff through API and return a JWT token in response.
                //This JWT token will then be added to the front-end cookies authentication as claim to be used when staff attempts to acces API endpoints.
                AuthResponse authResponse = await _staffService.Login(loginRequest);

                //User has been successfully authenticated.
                if (authResponse.ReturnStatusCodeMessage == null)
                {
                    //Store TempData information to be used later in the login process after valid verification code has been input.
                    TempData["LoginEmail"] = loginInput.Email;
                    TempData["LoginPassword"] = loginInput.Password;
                    TempData["UserID"] = authResponse.UserID;
                    TempData["UserName"] = authResponse.UserName;
                    TempData["JWTToken"] = authResponse.Token;

                    //Get a new verification code by calling the GenerateToken method in the _verificationTokenService.
                    int verficationToken = await _verificationTokenService.GenerateToken();

                    //Set the verificationCodeSent property to true as this will be needed to show the textbox for the user to input the code they received.
                    verificationCodeSent = true;

                    //Set the sent verification code in TempData to be user for matching later.
                    TempData["VerificationToken"] = verficationToken;
                    TempData["VerificationCodeSent"] = verificationCodeSent;

                    //Keep TempData incase it is needed again.
                    TempData.Keep();

                    ////Declare new Response to store the reponse from the email service and populate by calling the SendVerificationCode method.
                    //Response emailResponse = await SendVerificationCode(loginInput.Email, verficationToken);

                    ////The email has successfully been sent.
                    //if (emailResponse.IsSuccessStatusCode)
                    //{
                    //    //Set the verificationCodeSent property to true as this will be needed to show the textbox for the user to input the code they received.
                    //    verificationCodeSent = true;

                    //    //Set the sent verification code in TempData to be user for matching later.
                    //    TempData["VerificationToken"] = verficationToken;
                    //    TempData["VerificationCodeSent"] = verificationCodeSent;

                    //    //Keep TempData incase it is needed again.
                    //    TempData.Keep();
                    //}
                    ////The email has not been sent successfully.
                    //else
                    //{
                    //    //Set the verificationCodeSent property to false to ensure the enter login details section remains visible.
                    //    verificationCodeSent = false;

                    //    //Keep TempData incase it is needed again.
                    //    TempData.Keep();

                    //    //Add an error to the ModelState to inform the user that the email was not sent.
                    //    ModelState.AddModelError(string.Empty, "There was a problem sending the verification code");

                    //    //Return the Page.
                    //    return Page();
                    //}
                }
                //User has not been authenticated.
                else
                {
                    //Set verificationCodeSent property to false to ensure verification code input is not shown.
                    verificationCodeSent = false;

                    //AuthResponse has returned stating that the users account is locked.
                    if (authResponse.ReturnStatusCodeMessage == "Account locked")
                    {
                        //Add an error to the ModelState to inform the user that their account is locked.
                        ModelState.AddModelError(string.Empty, "Your account is currently locked try again later");
                    }
                    //AuthResponse has returned stating that the user has input an invalid email address.
                    else if (authResponse.ReturnStatusCodeMessage == "Invalid email")
                    {
                        //Add an error to the ModelState to inform the user that their login attempt did not work.
                        ModelState.AddModelError(string.Empty, "Invalid login attempt");
                    }
                    //AuthResponse has returned stating that the user has input an invalid password.
                    else
                    {

                        //Add an error to the ModelState to inform the user that their login attempt did not work.
                        ModelState.AddModelError(string.Empty, "Invalid login attempt");

                        //TempData has IncorrectAttempts value.
                        if (TempData["IncorrectAttempts"] != null)
                        {
                            //Set IncorrectAttempts property to TempData value.
                            IncorrectAttempts = (int)TempData["IncorrectAttempts"];
                            //Increment IncorrectAttempts.
                            IncorrectAttempts++;
                        }
                        //TempData does not have IncorrectAttempts value.
                        else
                        {
                            //Set IncorrectAttempts property to 1.
                            IncorrectAttempts = 1;
                        }


                        //User has input invalid login details 5 times.
                        if (IncorrectAttempts == 5)
                        {
                            //Lock the staff account by calling the Lock method in the _staffService
                            authResponse = await _staffService.Lock(loginInput.Email);

                            //Redirect user to the AccountLocked page.
                            return Redirect("/Account/AccountLocked");
                        }

                        //Set IncorrectAttempts in TempData.
                        TempData["IncorrectAttempts"] = IncorrectAttempts;

                        //Keep TempData to ensure that IncorrectAttempts is stored.
                        TempData.Keep();
                    }

                    //Return the Page.
                    return Page();
                }
            }

            //Keep TempData incase it is needed again.
            TempData.Keep();

            //Return the Page.
            return Page();
        }

        //Method Summary:
        //This method executes when the "Login" button is clicked in the "Enter your verification code" section.
        //When executed the VerificationCodeInput model is validated and if valid the CurrentUser logged in.
        public async Task<IActionResult> OnPostLogin()
        {
            //Set the verificationCodeSent property value to the value stored in TempData.
            verificationCodeSent = Boolean.Parse(TempData["VerificationCodeSent"].ToString());

            verificationCodeInput.VerificationCode = TempData["VerificationToken"].ToString();

            //User has entered a verification code.
            if (verificationCodeInput.VerificationCode != null)
            {
                //The entered verification code matches the sent verification code.
                if (verificationCodeInput.VerificationCode == TempData["VerificationToken"].ToString())
                {
                    //Create a ClaimsPrinicpal and populated with claims contained in the JWTToken generated earlier.
                    ClaimsPrincipal jwtClaimsPrincipal = await ValidateJWTToken(TempData["JWTToken"].ToString());

                    //Create a ClaimsIdentity and add the UserID, Email, and JWT token as claims.
                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, TempData["UserID"].ToString()));
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, TempData["UserName"].ToString()));
                    claimsIdentity.AddClaim(new Claim("Token", TempData["JWTToken"].ToString()));

                    //Loop over each of the claims contained in the JWT token.
                    foreach (Claim claim in jwtClaimsPrincipal.Claims)
                    {
                        //Add each claim to the ClaimsIdentity.
                        claimsIdentity.AddClaim(claim);
                    }

                    //Create a ClaimsPrincipal, this will be used for authenticating the user on the front-end.
                    ClaimsPrincipal currentUser = new ClaimsPrincipal();

                    //Add the ClaimsIdentity to the ClaimsPrincipal to ensure front-end authentication contains all required claims.
                    currentUser.AddIdentity(claimsIdentity);

                    //Get the expires claim, this is needed to ensure that the front-end login session expires at the same time as the JWT token.
                    Claim expiresClaim = currentUser.FindFirst("exp");

                    //Convert the expires claim value to a long so it can be converted to a datetime.
                    long ticks = long.Parse(expiresClaim.Value);

                    //Convert the expires claims long value to a datetime.
                    DateTime? Expires = DateTimeOffset.FromUnixTimeSeconds(ticks).LocalDateTime;

                    //Initialise a new AuthenticationProperties instance, this is needed when logging the user into the application.
                    AuthenticationProperties authenticationProperties = new AuthenticationProperties();

                    //Set the expires of the AuthenticationProperties to the expires value from the expires claim of the claimsIdentity.
                    authenticationProperties.ExpiresUtc = Expires;

                    //Set the HttpContext user to the current user.
                    HttpContext.User = currentUser;

                    //Sign the user in to the front-end.
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, currentUser, authenticationProperties);

                    //Clear TempData to ensure fresh start.
                    TempData.Clear();

                    _logger.LogInformation("User logged in.");

                    //Redirect to the Index/Home page.
                    return Redirect("/Index");
                }
                //The entered verification code does not match the sent verification code.
                else
                {
                    //Keep TempData incase its needed again.
                    TempData.Keep();

                    //Add an error to the ModelState to inform the user that they entered an invalid verification code.
                    ModelState.AddModelError(string.Empty, "Invalid verification code");

                    //Return the Page.
                    return Page();
                }
            }

            //Keep TempData incase its needed again.
            TempData.Keep();

            //Return the Page.
            return Page();
        }

        //Method Summary:
        //This method is executed when the "Request verification code" button is clicked in the account details section.
        //When executed this method attempts to send a verification code email to the user and returns the response from the _emailService.
        public async Task<Response> SendVerificationCode(string emailAddress, int verficationToken)
        {
            //Declare a new EmailAddress object and assign the users email address as the value.
            EmailAddress to = new EmailAddress(emailAddress);

            //Call the SendVerificationCodeEmail in the _emailService and return the response.
            return await _emailService.SendVerificationCodeEmail(to, verficationToken);
        }

        //Method Summary:
        //This method is executed when the "Login" button is clicked.
        //When executed this method validates and gets the claims from the JWT token returned in the AuthResponse.
        public async Task<ClaimsPrincipal> ValidateJWTToken(string token)
        {
            //Get the JWT issuer from settings.
            string issuer = await _settingsService.GetSettingString(DFI.FaultReporting.Common.Constants.Settings.JWTISSUER);

            //Get the JWT audience from settings.
            string audience = await _settingsService.GetSettingString(DFI.FaultReporting.Common.Constants.Settings.JWTAUDIENCE);

            //Get the JWT key from settings.
            SecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(await _settingsService.GetSettingString(DFI.FaultReporting.Common.Constants.Settings.JWTKEY)));

            //Declare new SecurityToken, this is used as a return value for token validation below.
            SecurityToken tokenValidated;

            //Create new TokenValidationParameters to be used for token validation below.
            TokenValidationParameters tokenValidationParameters = new TokenValidationParameters();
            tokenValidationParameters.ValidateLifetime = true;
            tokenValidationParameters.ValidAudience = audience;
            tokenValidationParameters.ValidIssuer = issuer;
            tokenValidationParameters.IssuerSigningKey = key;

            //Create new ClaimsPrincipal which will contain JWT claims and populate by attempting to validate the JWT token.
            ClaimsPrincipal claimsPrincipal = new JwtSecurityTokenHandler().ValidateToken(token, tokenValidationParameters, out tokenValidated);

            //Return the ClaimsPrincipal.
            return claimsPrincipal;
        }
        #endregion Login
    }
}
