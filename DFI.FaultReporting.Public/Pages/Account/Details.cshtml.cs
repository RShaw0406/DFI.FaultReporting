using Azure.Core;
using DFI.FaultReporting.Common.SessionStorage;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Emails;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Tokens;
using DFI.FaultReporting.Services.Interfaces.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Linq;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Cryptography;

namespace DFI.FaultReporting.Public.Pages.Account
{
    public class DetailsModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<DetailsModel> _logger;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISettingsService _settingsService;
        private readonly IEmailService _emailService;
        private readonly IVerificationTokenService _verificationTokenService;

        //Inject dependencies in constructor.
        public DetailsModel(ILogger<DetailsModel> logger, IUserService userService, IHttpContextAccessor httpContextAccessor, ISettingsService settingsService, IEmailService emailService, 
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
        //Declare CurrentUser property, this is needed when calling the _userService.
        public User CurrentUser { get; set; }

        //Declare AccountDetailsInput property, this is needed when updating account details.
        [BindProperty]
        public AccountDetailsInputModel AccountDetailsInput { get; set; }

        //Declare VerificationCodeInput property, this is needed when inputting sent verification code when updating account details.
        [BindProperty]
        public VerificationCodeModel VerificationCodeInput { get; set; }

        //Declare PersonalDetailsInput property, this is needed when updating personal details.
        [BindProperty]
        public PersonalDetailsInputModel PersonalDetailsInput { get; set; }

        //Declare ContactDetailsInput property, this is needed when updating contact details.
        [BindProperty]
        public ContactDetailsInputModel ContactDetailsInput { get; set; }

        //Declare AddressDetailsInput property, this is needed when updating address details.
        [BindProperty]
        public AddressDetailsInputModel AddressDetailsInput { get; set; }

        //Declare ShowAccountDetails property, this is needed for displaying account details section. 
        public bool ShowAccountDetails { get; set; }

        //Declare ShowPersonalDetails property, this is needed for displaying personal details section.
        public bool ShowPersonalDetails { get; set; }

        //Declare ShowContactDetails property, this is needed for displaying contact details section.
        public bool ShowContactDetails { get; set; }

        //Declare ShowAddressDetails property, this is needed for displaying address details section.
        public bool ShowAddressDetails { get; set; }

        //Declare ShowDeleteDetails property, this is needed for displaying delete account section.
        public bool ShowDeleteDetails { get; set; }

        //Declare ShowDeleteDetailsSure property, this is needed for displaying the delete account check section.
        public bool ShowDeleteDetailsSure { get; set; }

        //Declare VerificationCodeSent property, this is needed for displaying the section for inputting the verification code.
        public bool VerificationCodeSent { get; set; }

        //Declare UpdateSuccess property, this is needed for displaying the updated success message.
        public bool UpdateSuccess { get; set; }

        //Declare ValidDOB property, this is needed for validating the input DOB when updating personal details.
        public bool ValidDOB { get; set; }

        //Declare InValidYearDOB property, this is needed for validating the input year when updating personal details.
        public bool InValidYearDOB { get; set; }

        //Declare InValidYearDOBMessage property, this is needed for storing the specific error message when upating personal details year.
        public string InValidYearDOBMessage = "";

        //Declare DayDOB property, this is needed for storing the day value from users DOB.
        [DisplayName("Day")]
        public int DayDOB { get; set; }

        //Declare MonthDOB property, this is needed for storing the month value from users DOB.
        [DisplayName("Month")]
        public int MonthDOB { get; set; }

        //Declare YearDOB property, this is needed for storing the year value from users DOB.
        [DisplayName("Year")]
        public int YearDOB { get; set; }

        //Declare AccountDetailsInputModel class, this is needed for updating account details.
        public class AccountDetailsInputModel
        {
            [DisplayName("New email address")]
            [DataType(DataType.EmailAddress, ErrorMessage = "You must enter a valid email address")]
            public string? NewEmail { get; set; }

            [DisplayName("New password")]
            // Password should contain the following:
            // At least 1 number
            // At least 1 special character
            // At least 1 uppercase letter
            // At least 1 lowercase letter
            // At least 8 characters in total
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[#$^+=!*()@%&]).{8,}$", ErrorMessage = "Password does not meet all requirements")]
            [DataType(DataType.Password)]
            public string? NewPassword { get; set; }

            [Display(Name = "Confirm new password")]
            [DataType(DataType.Password)]
            [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match")]
            public string? ConfirmPassword { get; set; }
        }

        //Declare VerificationCodeModel class, this is needed when inputting setn verification code when updating account details.
        public class VerificationCodeModel
        {
            public string? EmailVerificationCode { get; set; }

            [Required]
            [DisplayName("Verification code")]
            [Compare("EmailVerificationCode", ErrorMessage = "The verification codes do not match")]
            public string? VerificationCode { get; set; }
        }

        //Declare PersonalDetailsInputModel class, this is needed when updating personal details.
        public class PersonalDetailsInputModel
        {
            [DisplayName("New title")]
            [RegularExpression(@"^[a-zA-Z''-'\s]{1,8}$", ErrorMessage = "New title must not contain special characters or numbers")]
            [StringLength(8, ErrorMessage = "Prefix name must not be more than 8 characters")]
            public string? Prefix { get; set; }

            [DisplayName("New first name")]
            [RegularExpression(@"^[a-zA-Z''-'\s]{1,125}$", ErrorMessage = "New first name must not contain special characters or numbers")]
            [StringLength(125, ErrorMessage = "First name must not be more than 125 characters")]
            public string? FirstName { get; set; }

            [DisplayName("New last name")]
            [RegularExpression(@"^[a-zA-Z''-'\s]{1,125}$", ErrorMessage = "New last name must not contain special characters or numbers")]
            [StringLength(125, ErrorMessage = "Last name must not be more than 125 characters")]
            public string? LastName { get; set; }

            [DisplayName("Day")]
            [Range(1, 31, ErrorMessage = "New date of birth day must be between {1} and {2}")]
            public int? DayDOB { get; set; }

            [DisplayName("Month")]
            [Range(1, 12, ErrorMessage = "New date of birth month must be between {1} and {2}")]
            public int? MonthDOB { get; set; }

            [DisplayName("Year")]
            public int? YearDOB { get; set; }

            [DisplayName("New date of birth")]
            [DataType(DataType.Date)]
            public DateTime? DOB { get; set; }
        }

        //Declare ContactDetailsInputModel class, this is needed when updating contact details.
        public class ContactDetailsInputModel
        {
            [DisplayName("New contact number")]
            [RegularExpression(@"^(?:(?:\(?(?:0(?:0|11)\)?[\s-]?\(?|\+)44\)?[\s-]?(?:\(?0\)?[\s-]?)?)|(?:\(?0))(?:(?:\d{5}\)?[\s-]?\d{4,5})|(?:\d{4}\)?[\s-]?(?:\d{5}|\d{3}[\s-]?\d{3}))|(?:\d{3}\)?[\s-]?\d{3}[\s-]?\d{3,4})|(?:\d{2}\)?[\s-]?\d{4}[\s-]?\d{4}))(?:[\s-]?(?:x|ext\.?|\#)\d{3,4})?$", ErrorMessage = "You must enter a valid new contact number")]
            [DataType(DataType.PhoneNumber, ErrorMessage = "You must enter a valid new contact number")]
            public string? ContactNumber { get; set; }
        }

        //Declare AddressDetailsInputModel class, this is needed when updating contact details.
        public class AddressDetailsInputModel
        {
            [DisplayName("New address line 1")]
            [RegularExpression(@"^[a-zA-Z0-9''-'\s]{1,100}$", ErrorMessage = "new address line 1 must not contain special characters")]
            [StringLength(100, ErrorMessage = "new address line 1 must not be more than 100 characters")]
            public string? AddressLine1 { get; set; }

            [DisplayName("New address line 2")]
            [RegularExpression(@"^[a-zA-Z0-9''-'\s]{1,100}$", ErrorMessage = "New address line 2 must not contain special characters")]
            [StringLength(100, ErrorMessage = "New address line 2 must not be more than 100 characters")]
            public string? AddressLine2 { get; set; }

            [DisplayName("New address line 3")]
            [RegularExpression(@"^[a-zA-Z0-9''-'\s]{1,100}$", ErrorMessage = "New address line 3 must not contain special characters")]
            [StringLength(100, ErrorMessage = "New address line 3 must not be more than 100 characters")]
            public string? AddressLine3 { get; set; }

            [DisplayName("New postcode")]
            [RegularExpression(@"^(([Bb][Tt][0-9]{1,2})\s?[0-9][A-Za-z]{2})$", ErrorMessage = "You must enter a valid Northern Ireland postcode")]
            public string? Postcode { get; set; }
        }
        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is executed when the page loads and is used for setting the CurrentUser in session.
        public async Task<IActionResult> OnGetAsync()
        {
            //Clear session to ensure fresh start.
            HttpContext.Session.Clear();

            //Clear TempData to ensure fresh start.
            TempData.Clear();

            //Ensure UpdateSuccess message is not showing.
            UpdateSuccess = false;

            //The contexts current user exists.
            if (_httpContextAccessor.HttpContext.User != null)
            {
                //The contexts current user has been authenticated
                if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true)
                {
                    //Get the ID from the contexts current user, needed for populating CurrentUser property from DB.
                    string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

                    //Get the JWT token claim from the contexts current user, needed for populating CurrentUser property from DB.
                    Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

                    //Set the jwtToken string to the JWT token claims value, needed for populating CurrentUser property from DB.
                    string? jwtToken = jwtTokenClaim.Value;

                    //Set the CurrentUser property by calling the GetUser method in the _userService.
                    CurrentUser = await _userService.GetUser(Convert.ToInt32(userID), jwtToken);

                    //Set the CurrentUser in session, needed for displaying details.
                    HttpContext.Session.SetInSession("User", CurrentUser);

                    //Set ShowAccountDetails, this is the default view a user has.
                    ShowAccountDetails = true;
                }
            }

            //Return the page.
            return Page();
        }
        #endregion Page Load

        #region Account Details
        //Method Summary:
        //This method is excuted when the "Account details" link is clicked in the side navigation.
        //When excuted the "Account details" section is displayed.
        public void OnGetShowAccountDetails()
        {
            //Set the CurrentUser property by getting the "User" object stored in session.
            CurrentUser = HttpContext.Session.GetFromSession<User>("User");

            //Set the ShowAccountDetails property to true.
            ShowAccountDetails = true;

            //Clear all TempData to ensure a fresh start.
            TempData.Clear();
        }

        //Method Summary:
        //This method is executed when the "Request verification code" button is clicked in the "Account details" section.
        //When executed the AccountDetailsInput model is validated and if valid an email containing a generated 6 digit code is sent to the user.
        public async Task<IActionResult> OnPostRequestVerificationCode()
        {
            //Set the ShowAccountDetails property to true so that the section remains visible after the post.
            ShowAccountDetails = true;

            //Set the CurrentUser property by getting the "User" object stored in session so that the property remains populated after the post.
            CurrentUser = HttpContext.Session.GetFromSession<User>("User");

            //Set the AccountDetailsInput model in session so that it can be accessed again after the post.
            HttpContext.Session.SetInSession("AccountDetailsInputModel", AccountDetailsInput);

            //The "NewEmail" or the "NewPassword" and "ConfirmPassword" properties of the AccountDetailsInput model are not null.
            //The user has changed at least 1 piece of information.
            if (AccountDetailsInput.NewEmail != null || AccountDetailsInput.NewPassword != null && AccountDetailsInput.ConfirmPassword != null)
            {
                //Initialise a new ValidationContext to be used to validate the AccountDetailsInput model only.
                ValidationContext validationContext = new ValidationContext(AccountDetailsInput);

                //Create a collection to store the returned AccountDetailsInput model validation results.
                ICollection<ValidationResult> validationResults = new List<ValidationResult>();

                //Carry out validation check on the AccountDetailsInput model.
                bool isAccountDetailsValid = Validator.TryValidateObject(AccountDetailsInput, validationContext, validationResults, true);

                //The AccountDetailsInput model is valid.
                if (isAccountDetailsValid)
                {
                    //Generate a verification code by calling the GenerateToken() method from the _verificationTokenService.
                    int verificationToken = await _verificationTokenService.GenerateToken();

                    //Declare a new string to store the email address to be used for sending the verification code.
                    string? emailAddress = string.Empty;

                    //The user has changed their email address.
                    if (AccountDetailsInput.NewEmail != null)
                    {
                        //Set the emailAddress string to the new email address.
                        emailAddress = AccountDetailsInput.NewEmail;
                    }
                    //The user has not changed their email address.
                    else
                    {
                        //Set the emailAddress string to the users current email address.
                        emailAddress = CurrentUser.Email;
                    }

                    //Declare new Response to store the reponse from the email service and populate by calling the SendVerificationCode method.
                    Response emailResponse = await SendVerificationCode(emailAddress, verificationToken);

                    //The email has successfully been sent.
                    if (emailResponse.IsSuccessStatusCode)
                    {
                        //Set the VerificationCodeSent property to true as this will be needed to show the textbox for the user to input the code they received.
                        VerificationCodeSent = true;

                        //Store the verificationToken in TempData as it will be needed to check against the code input by the user.
                        TempData["VerificationToken"] = verificationToken;

                        //Store the VerificationCodeSent property value in TempData as it will be needed to ensure the code input textboxe remains visible if an error occurs.
                        TempData["VerificationCodeSent"] = VerificationCodeSent;

                        //Keep all TempData
                        TempData.Keep();
                    }
                    //The email has not been sent successfully.
                    else
                    {
                        //Set the VerificationCodeSent property to false to ensure the "Account details" section remains visible.
                        VerificationCodeSent = false;

                        //Clear all TempData to ensure a fresh start.
                        TempData.Clear();

                        //Add an error to the ModelState to inform the user that the email was not sent.
                        ModelState.AddModelError(string.Empty, "There was a problem sending the verification code");

                        //Return the Page.
                        return Page();
                    }
                }
                //The AccountDetailsInput model is not valid.
                else
                {
                    //Set the VerificationCodeSent property to false to ensure the "Account details" section remains visible.
                    VerificationCodeSent = false;

                    //Clear all TempData to ensure a fresh start.
                    TempData.Clear();

                    //Loop over each validationResult in the returned validationResults
                    foreach (ValidationResult validationResult in validationResults)
                    {
                        //Add an error to the ModelState to inform the user of en validation errors.
                        ModelState.AddModelError(string.Empty, validationResult.ErrorMessage);
                    }

                    //Return the Page.
                    return Page();
                }
            }
            //The user has not changed any information.
            else
            {
                //Set the VerificationCodeSent property to false to ensure the "Account details" section remains visible.
                VerificationCodeSent = false;

                //Clear all TempData to ensure a fresh start.
                TempData.Clear();

                //Add an error to the ModelState to inform the user that they have not changed any information.
                ModelState.AddModelError(string.Empty, "You have not changed any account details");

                //Return the Page();
                return Page();
            }

            //If the method get this far something has gone wrong.

            //Keep all TempData so that it can be reused if needed.
            TempData.Keep();

            //Return the Page.
            return Page();
        }

        //Method Summary:
        //This method executes when the "Update" button is clicked in the "Account details" section.
        //When executed the VerificationCodeInput model is validated and if valid the CurrentUser is updated.
        public async Task<IActionResult> OnPostUpdateAccountDetails()
        {
            //Set VerificationCodeSent to true, this is needed to ensure verification code input remains displayed if an error occurs.
            VerificationCodeSent = true;

            //Set ShowAccountDetails to true, this is needed to ensure the "Account details" section remains displayed.
            ShowAccountDetails = true;

            //Set the CurrentUser property to the "User" session object.
            CurrentUser = HttpContext.Session.GetFromSession<User>("User");

            //Set the AccountDetailsInput to the "AccountDetailsInputModel" session object.
            AccountDetailsInput = HttpContext.Session.GetFromSession<AccountDetailsInputModel>("AccountDetailsInputModel");

            //Get the sent verification code from TempData, needed for validating whether codes match. 
            VerificationCodeInput.EmailVerificationCode = TempData["VerificationToken"].ToString();

            //Initialise a new ValidationContext to be used to validate the VerificationCodeInput model only.
            ValidationContext validationContext = new ValidationContext(VerificationCodeInput);

            //Create a collection to store the returned VerificationCodeInput model validation results.
            ICollection<ValidationResult> validationResults = new List<ValidationResult>();

            //Carry out validation check on the VerificationCodeInput model.
            bool isVerificationCodeValid = Validator.TryValidateObject(VerificationCodeInput, validationContext, validationResults, true);

            //The VerificationCodeInput model is valid.
            if (isVerificationCodeValid)
            {
                Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

                string? jwtToken = jwtTokenClaim.Value;

                if (AccountDetailsInput.NewEmail != null)
                {
                    CurrentUser.Email = AccountDetailsInput.NewEmail;
                }

                if (AccountDetailsInput.NewPassword != null)
                {
                    byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);

                    string passwordHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                        password: AccountDetailsInput.NewPassword!,
                        salt: salt,
                        prf: KeyDerivationPrf.HMACSHA256,
                        iterationCount: 100000,
                        numBytesRequested: 256 / 8));

                    CurrentUser.Password = passwordHash;
                    CurrentUser.PasswordSalt = Convert.ToBase64String(salt);
                }

                User updatedUser = await _userService.UpdateUser(CurrentUser, jwtToken);

                if (updatedUser != null)
                {
                    HttpContext.Session.SetInSession("User", CurrentUser);

                    CurrentUser = HttpContext.Session.GetFromSession<User>("User");

                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, CurrentUser.Email));

                    foreach (Claim claim in HttpContext.User.Claims)
                    {
                        if (claim.Type != ClaimTypes.Name)
                        {
                            claimsIdentity.AddClaim(claim);
                        }                  
                    }

                    ClaimsPrincipal currentUser = new ClaimsPrincipal();
                    currentUser.AddIdentity(claimsIdentity);

                    Claim expiresClaim = currentUser.FindFirst("exp");

                    long ticks = long.Parse(expiresClaim.Value);

                    DateTime? Expires = DateTimeOffset.FromUnixTimeSeconds(ticks).LocalDateTime;

                    AuthenticationProperties authenticationProperties = new AuthenticationProperties();

                    authenticationProperties.ExpiresUtc = Expires;

                    await HttpContext.SignOutAsync();

                    HttpContext.User = currentUser;

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, currentUser, authenticationProperties);

                    VerificationCodeSent = false;

                    AccountDetailsInput.NewEmail = string.Empty;

                    AccountDetailsInput.NewPassword = string.Empty;

                    AccountDetailsInput.ConfirmPassword = string.Empty;

                    TempData.Clear();

                    UpdateSuccess = true;

                    return Page();
                }
                else
                {
                    TempData.Keep();

                    ModelState.AddModelError(string.Empty, "There was a problem updating your account details.");

                    return Page();
                }
            }
            else
            {
                TempData.Keep();

                foreach (ValidationResult validationResult in validationResults)
                {
                    ModelState.AddModelError(string.Empty, validationResult.ErrorMessage);
                }

                return Page();
            }
        }

        public async Task<IActionResult> OnPostCancelAccountDetailsUpdate()
        {
            CurrentUser = HttpContext.Session.GetFromSession<User>("User");

            ModelState.Clear();

            ShowAccountDetails = true;

            VerificationCodeSent = false;

            AccountDetailsInput.NewEmail = string.Empty;

            AccountDetailsInput.NewPassword = string.Empty;

            AccountDetailsInput.ConfirmPassword = string.Empty;

            TempData.Clear();

            UpdateSuccess = false;

            return Page();
        }

        public async Task<Response> SendVerificationCode(string emailAddress, int verficationToken)
        {
            EmailAddress to = new EmailAddress(emailAddress);

            return await _emailService.SendVerificationCodeEmail(to, verficationToken);
        }
        #endregion Account Details

        #region Personal Details
        public void OnGetShowPersonalDetails()
        {
            CurrentUser = HttpContext.Session.GetFromSession<User>("User");

            ShowPersonalDetails = true;

            DateTime dob = Convert.ToDateTime(CurrentUser.DOB);

            DayDOB = (int)dob.Day;

            MonthDOB = (int)dob.Month;

            YearDOB = (int)dob.Year;

            ValidDOB = true;

            InValidYearDOB = false;

            TempData.Clear();
        }

        public async Task<IActionResult> OnPostUpdatePersonalDetails()
        {
            ShowPersonalDetails = true;

            CurrentUser = HttpContext.Session.GetFromSession<User>("User");

            DateTime dob = Convert.ToDateTime(CurrentUser.DOB);

            DayDOB = (int)dob.Day;

            MonthDOB = (int)dob.Month;

            YearDOB = (int)dob.Year;

            ValidDOB = true;

            //Initialise a new ValidationContext to be used to validate the PersonalDetailsInput model only.
            ValidationContext validationContext = new ValidationContext(PersonalDetailsInput);

            //Create a collection to store the returned PersonalDetailsInput model validation results.
            ICollection<ValidationResult> validationResults = new List<ValidationResult>();

            //Carry out validation check on the PersonalDetailsInput model.
            bool isPersonalDetailsValid = Validator.TryValidateObject(PersonalDetailsInput, validationContext, validationResults, true);

            if (PersonalDetailsInput.Prefix != null || PersonalDetailsInput.FirstName != null || PersonalDetailsInput.LastName != null || PersonalDetailsInput.DayDOB != null || PersonalDetailsInput.MonthDOB != null || PersonalDetailsInput.YearDOB != null)
            {
                if (PersonalDetailsInput.DayDOB != null || PersonalDetailsInput.MonthDOB != null || PersonalDetailsInput.YearDOB != null)
                {
                    if (PersonalDetailsInput.YearDOB > DateTime.Now.Year)
                    {
                        InValidYearDOB = true;

                        InValidYearDOBMessage = "New date of birth year cannot be in the future";

                        validationResults.Add(new ValidationResult(InValidYearDOBMessage));
                    }
                    else if (PersonalDetailsInput.YearDOB < 1900)
                    {
                        InValidYearDOB = true;

                        InValidYearDOBMessage = "New date of birth year must be at least after 1900";

                        validationResults.Add(new ValidationResult(InValidYearDOBMessage));
                    }

                    string inputDOB = PersonalDetailsInput.YearDOB.ToString() + "-" + PersonalDetailsInput.MonthDOB.ToString() + "-" + PersonalDetailsInput.DayDOB.ToString();

                    DateTime newDOB;

                    ValidDOB = DateTime.TryParse(inputDOB, out newDOB);

                    if (ValidDOB)
                    {
                        PersonalDetailsInput.DOB = newDOB;
                    }
                    else
                    {
                        validationResults.Add(new ValidationResult("New date of birth must contain a valid day, month, and year"));
                    }
                }

                if (isPersonalDetailsValid)
                {
                    if (InValidYearDOB)
                    {
                        //Add an error to the ModelState to inform the user that they have entered a year that is in the future.
                        ModelState.AddModelError(string.Empty, InValidYearDOBMessage);
                    }

                    if (!ValidDOB)
                    {
                        PersonalDetailsInput.DOB = null;

                        //Add an error to the ModelState to inform the user that they have not entered a valid date.
                        ModelState.AddModelError(string.Empty, "New date of birth must contain day, month, and year");
                    }

                    if (!ValidDOB || InValidYearDOB)
                    {
                        //Return the Page();
                        return Page();
                    }

                    if (PersonalDetailsInput.DOB != null ) 
                    {
                        CurrentUser.DOB = PersonalDetailsInput.DOB;
                    }

                    if (PersonalDetailsInput.Prefix != null)
                    {
                        CurrentUser.Prefix = PersonalDetailsInput.Prefix;
                    }

                    if (PersonalDetailsInput.FirstName != null)
                    {
                        CurrentUser.FirstName = PersonalDetailsInput.FirstName;
                    }

                    if (PersonalDetailsInput.LastName != null)
                    {
                        CurrentUser.LastName = PersonalDetailsInput.LastName;
                    }

                    Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

                    string? jwtToken = jwtTokenClaim.Value;

                    User updatedUser = await _userService.UpdateUser(CurrentUser, jwtToken);

                    HttpContext.Session.Clear();

                    HttpContext.Session.SetInSession("User", CurrentUser);

                    CurrentUser = HttpContext.Session.GetFromSession<User>("User");

                    dob = Convert.ToDateTime(CurrentUser.DOB);

                    DayDOB = (int)dob.Day;

                    MonthDOB = (int)dob.Month;

                    YearDOB = (int)dob.Year;

                    ModelState.Clear();

                    PersonalDetailsInput.Prefix = string.Empty;

                    PersonalDetailsInput.FirstName = string.Empty;

                    PersonalDetailsInput.LastName = string.Empty;

                    PersonalDetailsInput.DayDOB = null;

                    PersonalDetailsInput.MonthDOB = null;

                    PersonalDetailsInput.YearDOB = null;

                    PersonalDetailsInput.DOB = null;

                    UpdateSuccess = true;

                    return Page();
                }
                //The PersonalDetailsInput model is not valid.
                else
                {
                    //Loop over each validationResult in the returned validationResults
                    foreach (ValidationResult validationResult in validationResults)
                    {
                        //Add an error to the ModelState to inform the user of en validation errors.
                        ModelState.AddModelError(string.Empty, validationResult.ErrorMessage);
                    }

                    //Return the Page.
                    return Page();
                }
            }
            else
            {
                //Add an error to the ModelState to inform the user that they have not changed any information.
                ModelState.AddModelError(string.Empty, "You have not changed any account details");

                //Return the Page();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostCancelPersonalDetailsUpdate()
        {
            UpdateSuccess = false;

            CurrentUser = HttpContext.Session.GetFromSession<User>("User");

            DateTime dob = Convert.ToDateTime(CurrentUser.DOB);

            DayDOB = (int)dob.Day;

            MonthDOB = (int)dob.Month;

            YearDOB = (int)dob.Year;

            ValidDOB = true;

            ShowPersonalDetails = true;

            ModelState.Clear();

            PersonalDetailsInput.Prefix = string.Empty;

            PersonalDetailsInput.FirstName = string.Empty;

            PersonalDetailsInput.LastName = string.Empty;

            PersonalDetailsInput.DayDOB = null;

            PersonalDetailsInput.MonthDOB = null;

            PersonalDetailsInput.YearDOB = null;

            PersonalDetailsInput.DOB = null;

            return Page();
        }
        #endregion Personal Details

        #region Contact Details
        public void OnGetShowContactDetails()
        {
            CurrentUser = HttpContext.Session.GetFromSession<User>("User");

            ShowContactDetails = true;

            TempData.Clear();
        }

        public async Task<IActionResult> OnPostUpdateContactDetails()
        {
            ShowContactDetails = true;

            CurrentUser = HttpContext.Session.GetFromSession<User>("User");

            //Initialise a new ValidationContext to be used to validate the ContactDetailsInput model only.
            ValidationContext validationContext = new ValidationContext(ContactDetailsInput);

            //Create a collection to store the returned ContactDetailsInput model validation results.
            ICollection<ValidationResult> validationResults = new List<ValidationResult>();

            //Carry out validation check on the ContactDetailsInput model.
            bool isContactDetailsValid = Validator.TryValidateObject(ContactDetailsInput, validationContext, validationResults, true);

            if (ContactDetailsInput.ContactNumber != null)
            {
                if (isContactDetailsValid)
                {
                    CurrentUser.ContactNumber = ContactDetailsInput.ContactNumber;

                    Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

                    string? jwtToken = jwtTokenClaim.Value;

                    User updatedUser = await _userService.UpdateUser(CurrentUser, jwtToken);

                    HttpContext.Session.Clear();

                    HttpContext.Session.SetInSession("User", CurrentUser);

                    CurrentUser = HttpContext.Session.GetFromSession<User>("User");

                    ModelState.Clear();

                    ContactDetailsInput.ContactNumber = string.Empty;

                    UpdateSuccess = true;

                    return Page();

                }
                //The ContactDetailsInput model is not valid.
                else
                {
                    //Loop over each validationResult in the returned validationResults
                    foreach (ValidationResult validationResult in validationResults)
                    {
                        //Add an error to the ModelState to inform the user of en validation errors.
                        ModelState.AddModelError(string.Empty, validationResult.ErrorMessage);
                    }

                    //Return the Page.
                    return Page();
                }
            }
            else
            {
                //Add an error to the ModelState to inform the user that they have not changed any information.
                ModelState.AddModelError(string.Empty, "You have not changed any contact details");

                //Return the Page();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostCancelContactDetailsUpdate()
        {
            UpdateSuccess = false;

            CurrentUser = HttpContext.Session.GetFromSession<User>("User");

            ShowContactDetails = true;

            ModelState.Clear();

            ContactDetailsInput.ContactNumber = string.Empty;

            return Page();
        }
        #endregion Contact Details

        #region Address Details
        public void OnGetShowAddressDetails()
        {
            CurrentUser = HttpContext.Session.GetFromSession<User>("User");

            ShowAddressDetails = true;

            TempData.Clear();
        }

        public async Task<IActionResult> OnPostUpdateAddressDetails()
        {
            ShowAddressDetails = true;

            CurrentUser = HttpContext.Session.GetFromSession<User>("User");

            //Initialise a new ValidationContext to be used to validate the AddressDetailsInput model only.
            ValidationContext validationContext = new ValidationContext(AddressDetailsInput);

            //Create a collection to store the returned AddressDetailsInput model validation results.
            ICollection<ValidationResult> validationResults = new List<ValidationResult>();

            //Carry out validation check on the AddressDetailsInput model.
            bool isAddressDetailsValid = Validator.TryValidateObject(AddressDetailsInput, validationContext, validationResults, true);

            if (AddressDetailsInput.Postcode != null || AddressDetailsInput.AddressLine1 != null || AddressDetailsInput.AddressLine2 != null || AddressDetailsInput.AddressLine3 != null)
            {
                if (isAddressDetailsValid)
                {
                    if (AddressDetailsInput.Postcode != null)
                    {
                        CurrentUser.Postcode = AddressDetailsInput.Postcode;
                    }

                    if (AddressDetailsInput.AddressLine1 != null)
                    {
                        CurrentUser.AddressLine1 = AddressDetailsInput.AddressLine1;
                    }

                    if (AddressDetailsInput.AddressLine2 != null)
                    {
                        CurrentUser.AddressLine2 = AddressDetailsInput.AddressLine2;
                    }

                    if (AddressDetailsInput.AddressLine3 != null)
                    {
                        CurrentUser.AddressLine3 = AddressDetailsInput.AddressLine3;
                    }

                    Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

                    string? jwtToken = jwtTokenClaim.Value;

                    User updatedUser = await _userService.UpdateUser(CurrentUser, jwtToken);

                    HttpContext.Session.Clear();

                    HttpContext.Session.SetInSession("User", CurrentUser);

                    CurrentUser = HttpContext.Session.GetFromSession<User>("User");

                    ModelState.Clear();

                    AddressDetailsInput.Postcode = string.Empty;

                    AddressDetailsInput.AddressLine1 = string.Empty;

                    AddressDetailsInput.AddressLine2 = string.Empty;

                    AddressDetailsInput.AddressLine3 = null;

                    UpdateSuccess = true;

                    return Page();
                }
                //The AddressDetailsInput model is not valid.
                else
                {
                    //Loop over each validationResult in the returned validationResults
                    foreach (ValidationResult validationResult in validationResults)
                    {
                        //Add an error to the ModelState to inform the user of en validation errors.
                        ModelState.AddModelError(string.Empty, validationResult.ErrorMessage);
                    }

                    //Return the Page.
                    return Page();
                }
            }
            else
            {
                //Add an error to the ModelState to inform the user that they have not changed any information.
                ModelState.AddModelError(string.Empty, "You have not changed any address details");

                //Return the Page();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostCancelAddressDetailsUpdate()
        {
            UpdateSuccess = false;

            CurrentUser = HttpContext.Session.GetFromSession<User>("User");

            ShowAddressDetails = true;

            ModelState.Clear();

            AddressDetailsInput.Postcode = string.Empty;

            AddressDetailsInput.AddressLine1 = string.Empty;

            AddressDetailsInput.AddressLine2 = string.Empty;

            AddressDetailsInput.AddressLine3 = null;

            return Page();
        }
        #endregion Address Details

        #region Delete Account
        public void OnGetShowDeleteDetails()
        {
            CurrentUser = HttpContext.Session.GetFromSession<User>("User");

            ShowDeleteDetails = true;

            ShowDeleteDetailsSure = false;

            TempData.Clear();
        }

        public async Task<IActionResult> OnPostDeleteAccount()
        {
            CurrentUser = HttpContext.Session.GetFromSession<User>("User");

            ShowDeleteDetails = true;

            ShowDeleteDetailsSure = true;

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAccountSure()
        {
            CurrentUser = HttpContext.Session.GetFromSession<User>("User");

            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

            string? jwtToken = jwtTokenClaim.Value;

            int? deletedUserID = await _userService.DeleteUser(CurrentUser.ID, jwtToken);

            if (deletedUserID != null)
            {
                Response emailResponse = await SendAccountDeletionEmail(CurrentUser.Email);

                await HttpContext.SignOutAsync();

                _logger.LogInformation("User deleted account and signed out.");

                HttpContext.Session.Clear();

                TempData.Clear();

                return Redirect("/Index");
            }
            else
            {
                //Add an error to the ModelState to inform the user that their account cannot be deleted.
                ModelState.AddModelError(string.Empty, "There was a problem deleting your account.");

                //Return the Page();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostCancelDeleteAccount()
        {
            CurrentUser = HttpContext.Session.GetFromSession<User>("User");

            ShowDeleteDetails = true;

            ShowDeleteDetailsSure = false;

            return Page();
        }

        public async Task<Response> SendAccountDeletionEmail(string emailAddress)
        {
            string subject = "DFI Fault Reporting: Account Deleted";
            EmailAddress to = new EmailAddress(emailAddress);
            string textContent = string.Empty;
            string htmlContent = "<p>Hello,</p><p>Your account has been successfully deleted.</p><p>Thank you for using the service.</p>";
            Attachment? attachment = null;

            return await _emailService.SendEmail(subject, to, textContent, htmlContent, attachment);
        }
        #endregion Delete Account
    }
}