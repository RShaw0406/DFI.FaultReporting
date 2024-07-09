using DFI.FaultReporting.Common.SessionStorage;
using DFI.FaultReporting.JWT.Requests;
using DFI.FaultReporting.JWT.Response;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using SendGrid;
using DFI.FaultReporting.Services.Interfaces.Admin;
using DFI.FaultReporting.Services.Interfaces.Emails;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Tokens;
using DFI.FaultReporting.Services.Interfaces.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.IdentityModel.Tokens;
using SendGrid.Helpers.Mail;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace DFI.FaultReporting.Public.Pages.Account.Register
{
    public class Step5Model : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<Step5Model> _logger;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISettingsService _settingsService;
        private readonly IEmailService _emailService;
        private readonly IVerificationTokenService _verificationTokenService;
        private readonly IContractorService _contractorService;

        //Inject dependencies in constructor.
        public Step5Model(ILogger<Step5Model> logger, IUserService userService, IHttpContextAccessor httpContextAccessor, ISettingsService settingsService, IEmailService emailService,
            IVerificationTokenService verificationTokenService, IContractorService contractor)
        {
            _logger = logger;
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
            _settingsService = settingsService;
            _emailService = emailService;
            _verificationTokenService = verificationTokenService;
            _contractorService = contractor;
        }
        #endregion Dependency Injection

        #region Properties
        //Declare RegistrationRequest property, this is needed when calling the _userService.
        [BindProperty]
        public RegistrationRequest RegistrationRequest { get; set; }

        //Declare isContractorEmail property, this is needed for storing whether the user is a contractor or not, so the date of birth fields can be hidden.
        [BindProperty]
        public bool isContractorEmail { get; set; }

        //Declare VerificationCodeInput property, this is needed when inputting sent verification code when registering.
        [BindProperty]
        public VerificationCodeModel VerificationCodeInput { get; set; }

        //Declare VerificationCodeModel class, this is needed when inputting sent verification code when registering.
        public class VerificationCodeModel
        {
            [Required]
            [DisplayName("Verification code")]
            public string? VerificationCode { get; set; }
        }
        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is executed when the page loads.
        //When executed a check is made to see if the user is already authenticated, if they are they are redirected to the index page.
        public async Task<IActionResult> OnGetAsync()
        {
            //The HttpContext user is already authenticated.
            if (HttpContext.User.Identity.IsAuthenticated == true)
            {
                //Redirect to the index page.

                Redirect("./Index");
            }

            //Get the session values.
            return Page();
        }
        #endregion Page Load

        #region Register

        //Method Summary:
        //This method executes when the "Register" button is clicked in the "Enter your verification code" section.
        //When executed the RegistrationRequest model is validated and if valid the user is registered.
        public async Task<IActionResult> OnPostRegister()
        {
            //Set the RegistrationRequest to the "RegistrationRequest" session object.
            RegistrationRequest = HttpContext.Session.GetFromSession<RegistrationRequest>("RegistrationRequest");

            //User has entered a verification code.
            if (VerificationCodeInput.VerificationCode != null)
            {
                //The entered verification code matches the sent verification code.
                if (VerificationCodeInput.VerificationCode == TempData["VerificationToken"].ToString())
                {
                    //Get an AuthRepsonse by calling the Register method in the _userService.
                    //This will authenticate the user through API and return a JWT token in response.
                    //This JWT token will then be added to the front-end cookies authentication as claim to be used when user attempts to acces API endpoints.
                    AuthResponse authResponse = await _userService.Register(RegistrationRequest);

                    //User has been successfully registered.
                    if (authResponse.ReturnStatusCodeMessage == null)
                    {
                        //Create a ClaimsPrinicpal and populated with claims contained in the JWTToken generated earlier.
                        ClaimsPrincipal jwtClaimsPrincipal = await ValidateJWTToken(authResponse.Token);

                        //Create a ClaimsIdentity and add the UserID, Email, and JWT token as claims.
                        ClaimsIdentity claimsIdentity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                        claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, authResponse.UserID.ToString()));
                        claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, authResponse.UserName.ToString()));
                        claimsIdentity.AddClaim(new Claim("Token", authResponse.Token.ToString()));

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

                        _logger.LogInformation("User registered.");

                        //Send an email to the user.
                        Response emailResponse = await SendRegistrationEmail(authResponse.UserName);

                        //Redirect to the Index/Home page.
                        return Redirect("/Index");
                    }
                    //User has not been registered.
                    else
                    {
                        //AuthResponse has returned stating that the email address is already in use.
                        if (authResponse.ReturnStatusCodeMessage == "Email address already used")
                        {
                            //Add an error to the ModelState to inform the user that email address is already in use.
                            ModelState.AddModelError(string.Empty, "An account with this email address already exists");
                        }
                        //AuthReponse has returned a different message.
                        else
                        {
                            //Add an error to the ModelState to inform the user that their registration attempt did not work.
                            ModelState.AddModelError(string.Empty, "There was a problem registering your account");
                        }

                        //Keep TempData incase its needed again.
                        TempData.Keep();

                        //Return the Page.
                        return Page();
                    }
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
        //This method is executed when the "Register" button is clicked.
        //When executed this method attempts to send an email to the user and returns the response from the _emailService.
        public async Task<Response> SendRegistrationEmail(string emailAddress)
        {
            //Get the isContractorEmail from the session.
            isContractorEmail = HttpContext.Session.GetFromSession<bool>("ContractorEmail");

            //Set the subject of the email explaining that the user that they have registered their account.
            string subject = "DFI Fault Reporting: Fault Report Submitted";

            //Declare a new EmailAddress object and assign the users email address as the value.
            EmailAddress to = new EmailAddress(emailAddress);

            //Set textContent to empty string as it will not be used here.
            string textContent = string.Empty;

            string htmlContent = string.Empty;

            if (isContractorEmail)
            {

                //Set the htmlContent to a message explaining to the user that their account has been successfully registered.
                htmlContent = "<p>Hello,</p><p>Thank you registering to use the DFI Fault Reporting application.</p>" +
                    "<p>You can now use the application to view your assigned repair jobs and report faults.</p>";
            }
            else
            {

                //Set the htmlContent to a message explaining to the user that their account has been successfully registered.
                htmlContent = "<p>Hello,</p><p>Thank you registering to use the DFI Fault Reporting application.</p>" +
                    "<p>You can now use the application to report faults on the road network and submit compensation claims.</p>";
            }

            //Set the attachment to null as it will not be used here.
            Attachment? attachment = null;

            //Call the SendEmail in the _emailService and return the response.
            return await _emailService.SendEmail(subject, to, textContent, htmlContent, attachment);
        }

        //Method Summary:
        //This method is executed when the "Register" button is clicked.
        //When executed this method validates and gets the claims from the JWT token returned in the AuthResponse.
        public async Task<ClaimsPrincipal> ValidateJWTToken(string token)
        {
            //Get the JWT issuer from settings.
            string issuer = await _settingsService.GetSettingString(Common.Constants.Settings.JWTISSUER);

            //Get the JWT audience from settings.
            string audience = await _settingsService.GetSettingString(Common.Constants.Settings.JWTAUDIENCE);

            //Get the JWT key from settings.
            SecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(await _settingsService.GetSettingString(Common.Constants.Settings.JWTKEY)));

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
        #endregion Register
    }
}
