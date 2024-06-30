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

namespace DFI.FaultReporting.Public.Pages.Account
{
    public class RegisterModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<RegisterModel> _logger;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISettingsService _settingsService;
        private readonly IEmailService _emailService;
        private readonly IVerificationTokenService _verificationTokenService;

        //Inject dependencies in constructor.
        public RegisterModel(ILogger<RegisterModel> logger, IUserService userService, IHttpContextAccessor httpContextAccessor, ISettingsService settingsService, IEmailService emailService,
            IVerificationTokenService verificationTokenService)
        {
            _logger = logger;
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
            _settingsService = settingsService;
            _emailService = emailService;
            _verificationTokenService = verificationTokenService;
        }
        #endregion Dependency Injection

        #region Properties
        //Declare RegistrationRequest property, this is needed when calling the _userService.
        [BindProperty]
        public RegistrationRequest RegistrationRequest { get; set; }

        //Declare VerificationCodeInput property, this is needed when inputting sent verification code when registering.
        [BindProperty]
        public VerificationCodeModel VerificationCodeInput { get; set; }

        //Declare VerificationCodeSent property, this is needed for displaying the section for inputting the verification code.
        public bool VerificationCodeSent { get; set; }

        //Declare ValidDOB property, this is needed for validating the input DOB when updating personal details.
        public bool ValidDOB { get; set; }

        //Declare InValidYearDOB property, this is needed for validating the input year when updating personal details.
        public bool InValidYearDOB { get; set; }

        //Declare InValidYearDOBMessage property, this is needed for storing the specific error message when upating personal details year.
        public string InValidYearDOBMessage = "";

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
        //This method is executed when the page loads and is used for clearing TempData to ensure fresh start.
        public async Task<IActionResult> OnGetAsync()
        {
            //Clear session to ensure fresh start.
            HttpContext.Session.Clear();

            //Clear TempData to ensure fresh start.
            TempData.Clear();

            //The HttpContext user is already authenticated.
            if (HttpContext.User.Identity.IsAuthenticated == true)
            {

            }
         
            //Set the ValidDOB property to true, this is needed for validation.
            ValidDOB = true;

            //Set the InValidYearDOB property to false, this is needed for validation.
            InValidYearDOB = false;

            //Return the Page.
            return Page();
        }
        #endregion Page Load

        #region Register

        //Method Summary:
        //This method is excuted when the "Request verification code" button is clicked on the "Enter your details below" section.
        //When excuted the "Enter your verification code" section is displayed.
        public async Task<IActionResult> OnPostRequestVerificationCode()
        {
            //User has changed at least 1 piece of information.
            if (RegistrationRequest.Email != null && RegistrationRequest.Password != null && RegistrationRequest.ConfirmPassword != null &&
                RegistrationRequest.Prefix != null && RegistrationRequest.FirstName != null && RegistrationRequest.LastName != null &&
                RegistrationRequest.DayDOB != null && RegistrationRequest.MonthDOB != null && RegistrationRequest.YearDOB != null &&
                RegistrationRequest.ContactNumber != null && RegistrationRequest.Postcode != null && RegistrationRequest.AddressLine1 != null)
            {
                //Initialise a new ValidationContext to be used to validate the RegistrationRequest model only.
                ValidationContext validationContext = new ValidationContext(RegistrationRequest);

                //Create a collection to store the returned RegistrationRequest model validation results.
                ICollection<ValidationResult> validationResults = new List<ValidationResult>();

                //Carry out validation check on the RegistrationRequest model.
                bool isRegistrationRequestValid = Validator.TryValidateObject(RegistrationRequest, validationContext, validationResults, true);

                //The entered year value is greater than the current year.
                if (RegistrationRequest.YearDOB > DateTime.Now.Year)
                {
                    //Set the InValidYearDOB to true.
                    InValidYearDOB = true;

                    //Set the InValidYearDOBMessage to an error message informing the user that the entered year cannot be in the future.
                    InValidYearDOBMessage = "Date of birth year cannot be in the future";

                    //Add the InValidYearDOBMessage to the validationResults.
                    validationResults.Add(new ValidationResult(InValidYearDOBMessage));
                }

                //The entered year is far in the past.
                if (RegistrationRequest.YearDOB < 1900)
                {
                    //Set the InValidYearDOB to true.
                    InValidYearDOB = true;

                    //Set the InValidYearDOBMessage to an error message informing the user that the entered year cannot be before 1900.
                    InValidYearDOBMessage = "Date of birth year must be at least after 1900";

                    //Add the InValidYearDOBMessage to the validationResults.
                    validationResults.Add(new ValidationResult(InValidYearDOBMessage));
                }

                //Create a string DOB by combining all the input DOB values.
                string inputDOB = RegistrationRequest.YearDOB.ToString() + "-" + RegistrationRequest.MonthDOB.ToString() + "-" + RegistrationRequest.DayDOB.ToString();

                //Declare a newDOB datetime, this is used as the return value of the attempted parse below.
                DateTime newDOB;

                //Attempt to parse the string DOB to a valid datetime.
                ValidDOB = DateTime.TryParse(inputDOB, out newDOB);

                //The parsed DOB is not a valid datetime.
                if (!ValidDOB)
                {
                    //Add a new ValidationResult to the validationResults informing the user that the entered date is not valid.
                    validationResults.Add(new ValidationResult("New date of birth must contain a valid day, month, and year"));
                }

                //The RegistrationRequest model is valid.
                if (isRegistrationRequestValid)
                {
                    //The entered year is invalid.
                    if (InValidYearDOB)
                    {
                        //Add an error to the ModelState to inform the user that they have entered a year that is in the future or too far in the past.
                        ModelState.AddModelError(string.Empty, InValidYearDOBMessage);
                    }

                    //The entered DOB is not valid.
                    if (!ValidDOB)
                    {
                        //Add an error to the ModelState to inform the user that they have not entered a valid date.
                        ModelState.AddModelError(string.Empty, "New date of birth must contain a valid day, month, and year");
                    }

                    //Either the DOB or the year are invalid.
                    if (!ValidDOB || InValidYearDOB)
                    {
                        //Return the Page();
                        return Page();
                    }

                    //Set the RegistrationRequest model in session so that it can be accessed again after the post.
                    HttpContext.Session.SetInSession("RegistrationRequest", RegistrationRequest);

                    //Get a new verification code by calling the GenerateToken method in the _verificationTokenService.
                    int verficationToken = await _verificationTokenService.GenerateToken();

                    //Declare new Response to store the reponse from the email service and populate by calling the SendVerificationCode method.
                    Response emailResponse = await SendVerificationCode(RegistrationRequest.Email, verficationToken);

                    //The email has successfully been sent.
                    if (emailResponse.IsSuccessStatusCode)
                    {
                        //Set the VerificationCodeSent property to true as this will be needed to show the textbox for the user to input the code they received.
                        VerificationCodeSent = true;

                        //Set the sent verification code in TempData to be user for matching later.
                        TempData["VerificationToken"] = verficationToken;
                        TempData["VerificationCodeSent"] = VerificationCodeSent;

                        //Keep TempData incase it is needed again.
                        TempData.Keep();
                    }
                    //The email has not been sent successfully.
                    else
                    {
                        //Set the VerificationCodeSent property to false to ensure the enter login details section remains visible.
                        VerificationCodeSent = false;

                        //Keep TempData incase it is needed again.
                        TempData.Keep();

                        //Add an error to the ModelState to inform the user that the email was not sent.
                        ModelState.AddModelError(string.Empty, "There was a problem sending the verification code");

                        //Return the Page.
                        return Page();
                    }
                }
                //The RegistrationRequest model is not valid.
                else
                {
                    //Loop over each validationResult in the returned validationResults
                    foreach (ValidationResult validationResult in validationResults)
                    {
                        //Add an error to the ModelState to inform the user of en validation errors.
                        ModelState.AddModelError(string.Empty, validationResult.ErrorMessage);
                    }

                    //Keep TempData incase it is needed again.
                    TempData.Keep();

                    //Return the Page.
                    return Page();
                }

                return Page();
            }
            //User has not entered any information.
            else
            {
                //Add an error to the ModelState to inform the user that they have not changed any information.
                ModelState.AddModelError(string.Empty, "You have not completed all required fields");

                //Return the Page();
                return Page();
            }
        }

        //Method Summary:
        //This method executes when the "Register" button is clicked in the "Enter your verification code" section.
        //When executed the RegistrationRequest model is validated and if valid the user is registered.
        public async Task<IActionResult> OnPostRegister()
        {
            //Set the RegistrationRequest to the "RegistrationRequest" session object.
            RegistrationRequest = HttpContext.Session.GetFromSession<RegistrationRequest>("RegistrationRequest");

            //Set the VerificationCodeSent property value to the value stored in TempData.
            VerificationCodeSent = Boolean.Parse(TempData["VerificationCodeSent"].ToString());

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
                        //Set VerificationCodeSent property to true to ensure verification code input is shown.
                        VerificationCodeSent = true;

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
        //This method is executed when the "Request verification code" button is clicked in the "Enter your details below" section.
        //When executed this method attempts to send a verification code email to the user and returns the response from the _emailService.
        public async Task<Response> SendVerificationCode(string emailAddress, int verficationToken)
        {
            //Declare a new EmailAddress object and assign the users email address as the value.
            EmailAddress to = new EmailAddress(emailAddress);

            //Call the SendVerificationCodeEmail in the _emailService and return the response.
            return await _emailService.SendVerificationCodeEmail(to, verficationToken);
        }

        //Method Summary:
        //This method is executed when the "Register" button is clicked.
        //When executed this method attempts to send an email to the user and returns the response from the _emailService.
        public async Task<Response> SendRegistrationEmail(string emailAddress)
        {
            //Set the subject of the email explaining that the user has deleted their account.
            string subject = "DFI Fault Reporting: Fault Report Submitted";

            //Declare a new EmailAddress object and assign the users email address as the value.
            EmailAddress to = new EmailAddress(emailAddress);

            //Set textContent to empty string as it will not be used here.
            string textContent = string.Empty;

            //Set the htmlContent to a message explaining to the user that their account has been successfully deleted.
            string htmlContent = "<p>Hello,</p><p>Thank you registering to use the DFI Fault Reporting application.</p>" +
                "<p>You can now use the application to report faults on the road network and submit compensation claims.</p>";

            //Set the attachment to null as it will not be used here.
            SendGrid.Helpers.Mail.Attachment? attachment = null;

            //Call the SendEmail in the _emailService and return the response.
            return await _emailService.SendEmail(subject, to, textContent, htmlContent, attachment);
        }

        //Method Summary:
        //This method is executed when the "Register" button is clicked.
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

        //Method Summary:
        //This method is executed when the "Cancel" button on the verification code section is clicked.
        //When executed this method clears the changes made by the user to ensure a fresh start and displayed the register section.
        public async Task<IActionResult> OnPostCancelPersonalDetailsUpdate()
        {
            //Set the ValidDOB property to true, this is needed for validation.
            ValidDOB = true;

            //Set the InValidYearDOB property to false, this is needed for validation.
            InValidYearDOB = false;

            //Clear the RegistrationRequest model to ensure fresh start.
            ModelState.Clear();
            RegistrationRequest.Email = string.Empty;
            RegistrationRequest.Password = string.Empty;
            RegistrationRequest.ConfirmPassword = string.Empty;
            RegistrationRequest.Prefix = string.Empty;
            RegistrationRequest.FirstName = string.Empty;
            RegistrationRequest.LastName = string.Empty;
            RegistrationRequest.DayDOB = null;
            RegistrationRequest.MonthDOB = null;
            RegistrationRequest.YearDOB = null;
            RegistrationRequest.ContactNumber = string.Empty;
            RegistrationRequest.Postcode = string.Empty;
            RegistrationRequest.AddressLine1 = string.Empty;
            RegistrationRequest.AddressLine2 = string.Empty;
            RegistrationRequest.AddressLine3 = string.Empty;

            //Return the Page();
            return Page();
        }
        #endregion Register
    }
}
