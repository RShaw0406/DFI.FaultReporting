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

        //Declare VerificationCodeModel class, this is needed when inputting sent verification code when updating account details.
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

            //User has input the correct verification code.
            if (isVerificationCodeValid)
            {
                //Get JWT Token Claim from HttpContext Users Claims.
                Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

                //Assign value of JWT Token Claim to jwtToken string, needed for passing to API in header.
                string? jwtToken = jwtTokenClaim.Value;

                //User has changed their email.
                if (AccountDetailsInput.NewEmail != null)
                {
                    //Assign the CurrentUsers email to the AccountDetailsInput email.
                    CurrentUser.Email = AccountDetailsInput.NewEmail;
                }

                //User has changed their password.
                if (AccountDetailsInput.NewPassword != null)
                {
                    //Generate a new password salt bytes.
                    byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);

                    //Has the new password using the salt.
                    string passwordHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                        password: AccountDetailsInput.NewPassword!,
                        salt: salt,
                        prf: KeyDerivationPrf.HMACSHA256,
                        iterationCount: 100000,
                        numBytesRequested: 256 / 8));

                    //Set the CurrentUser password to the hashed password.
                    CurrentUser.Password = passwordHash;

                    //Set the CurrentUser salt to the generated salt, must convert to base64 string to enable the API to consume request.
                    //PassSalt needs to be stored to enable passowrd verification when logging in.
                    CurrentUser.PasswordSalt = Convert.ToBase64String(salt);
                }

                //Update the CurrentUser by calling the UpdateUser method form the _userService.
                User updatedUser = await _userService.UpdateUser(CurrentUser, jwtToken);

                //User has been successfully updated.
                if (updatedUser != null)
                {
                    //Clear the current Session to ensure fresh start.
                    HttpContext.Session.Clear();

                    //Reset the "User" session object so that any changes are reflected on screen.
                    HttpContext.Session.SetInSession("User", CurrentUser);

                    //Repopulate the CurrentUser class property so that any changes are reflected in class property values.
                    CurrentUser = HttpContext.Session.GetFromSession<User>("User");

                    //Create a claimsIdentity to store the updated HttpContext User claims, this is needed to ensure that claims are updated to reflect ay changes.
                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);

                    //Add the new email address to the claimsIdentity to ensure it is included instead of the old email address.
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, CurrentUser.Email));

                    //Loop over each of the claims in the HttpContext users claims.
                    foreach (Claim claim in HttpContext.User.Claims)
                    {
                        //The claim is not of type "Name", this is needed to ensure that the old "Name" claim containing the old email address is not added.
                        if (claim.Type != ClaimTypes.Name)
                        {
                            //Add the claim to the claimsIdentity.
                            claimsIdentity.AddClaim(claim);
                        }                  
                    }

                    //Create a ClaimsPrinicipal to store the claimsIdentity, this is needed for re-logging the user into the application.
                    ClaimsPrincipal currentUser = new ClaimsPrincipal();

                    //Add the claimsIdentity to the ClaimsPrinicipal
                    currentUser.AddIdentity(claimsIdentity);

                    //Get the expires claim, this is needed to ensure that the re-login session expires at the same time as the previous.
                    Claim expiresClaim = currentUser.FindFirst("exp");

                    //Convert the expires claim value to a long so it can be converted to a datetime.
                    long ticks = long.Parse(expiresClaim.Value);

                    //Convert the expires claims long value to a datetime.
                    DateTime? Expires = DateTimeOffset.FromUnixTimeSeconds(ticks).LocalDateTime;

                    //Initialise a new AuthenticationProperties instance, this is needed when re-logging the user into the application.
                    AuthenticationProperties authenticationProperties = new AuthenticationProperties();

                    //Set the expires of the AuthenticationProperties to the expires value from the expires claim of the claimsIdentity.
                    authenticationProperties.ExpiresUtc = Expires;

                    //Sign the user out.
                    await HttpContext.SignOutAsync();

                    //Set the HttpContext user to the current user.
                    HttpContext.User = currentUser;

                    //Sign the user in, this is needed to ensure that the new email address is included in the claims which will mean it can be used throughout the rest of the application.
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, currentUser, authenticationProperties);

                    //Set the VerificationCodeSent property to false to ensure the veirification code input section is no longer shown.
                    VerificationCodeSent = false;

                    //Clear the AccountDetailsInput model to ensure fresh start.
                    AccountDetailsInput.NewEmail = string.Empty;
                    AccountDetailsInput.NewPassword = string.Empty;
                    AccountDetailsInput.ConfirmPassword = string.Empty;

                    //Clear TempData to ensure fresh start.
                    TempData.Clear();

                    //Set the UpdateSuccess property to true so the success message is shown onscreen.
                    UpdateSuccess = true;

                    //Return the Page.
                    return Page();
                }
                //User was not successfully updated.
                else
                {
                    //Keep TempData to ensure values can be reused.
                    TempData.Keep();

                    //Add an error to the ModelState to inform the user that their account details have not been updated.
                    ModelState.AddModelError(string.Empty, "There was a problem updating your account details.");

                    //Return the Page.
                    return Page();
                }
            }
            //User has entered an incorrect verification code.
            else
            {
                //Keep TempData to ensure values can be reused.
                TempData.Keep();

                //Loop over each validationResult in the returned validationResults
                foreach (ValidationResult validationResult in validationResults)
                {
                    //Add an error to the ModelState to inform the user that the code they entered is not valid/does not match the sent code.
                    ModelState.AddModelError(string.Empty, validationResult.ErrorMessage);
                }

                //Return the Page.
                return Page();
            }
        }

        //Method Summary:
        //This method is executed when the "Cancel" button on the verification code input section is clicked.
        //When executed this method clears the changes made by the user to ensure a fresh start and displayed the account details.
        public async Task<IActionResult> OnPostCancelAccountDetailsUpdate()
        {
            //Set the CurrentUser to the "User" session object.
            CurrentUser = HttpContext.Session.GetFromSession<User>("User");

            //Clear the AccountDetailsInput model to ensure fresh start.
            ModelState.Clear();
            AccountDetailsInput.NewEmail = string.Empty;
            AccountDetailsInput.NewPassword = string.Empty;
            AccountDetailsInput.ConfirmPassword = string.Empty;

            //Clear TempData to ensure fresh start.
            TempData.Clear();

            //Set the UpdateSuccess property to false to ensure the success message does not show.
            UpdateSuccess = false;

            //Set the VerificationCodeSent property to false to esnure the verification code input section is not shown.
            VerificationCodeSent = false;

            //Set the ShowAccountDetails property to true to ensure that the users account details section is shown.
            ShowAccountDetails = true;

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
        #endregion Account Details

        #region Personal Details
        //Method Summary:
        //This method is excuted when the "Personal details" link is clicked in the side navigation.
        //When excuted the "Personal details" section is displayed.
        public void OnGetShowPersonalDetails()
        {
            //Set the CurrentUser property by getting the "User" object stored in session.
            CurrentUser = HttpContext.Session.GetFromSession<User>("User");

            //Set the ShowPersonalDetails property to true.
            ShowPersonalDetails = true;

            //Get the CurrentUsers DOB.
            DateTime dob = Convert.ToDateTime(CurrentUser.DOB);

            //Get the Day value from the CurrentUsers DOB, this is needed to display onscreen.
            DayDOB = (int)dob.Day;

            //Get the Month value from the CurrentUsers DOB, this is needed to display onscreen.
            MonthDOB = (int)dob.Month;

            //Get the Year value from the CurrentUsers DOB, this is needed to display onscreen.
            YearDOB = (int)dob.Year;

            //Set the ValidDOB property to true, this is needed for validation.
            ValidDOB = true;

            //Set the InValidYearDOB property to false, this is needed for validation.
            InValidYearDOB = false;

            //Clear TempDate to ensure fresh start.
            TempData.Clear();
        }

        //Method Summary:
        //This method executes when the "Update" button is clicked in the "Personal details" section.
        //When executed the PersonalDetailsInput model is validated and if valid the CurrentUser is updated.
        public async Task<IActionResult> OnPostUpdatePersonalDetails()
        {
            //Set the ShowPersonalDetails property to true.
            ShowPersonalDetails = true;

            //Set the CurrentUser property by getting the "User" object stored in session.
            CurrentUser = HttpContext.Session.GetFromSession<User>("User");

            //Get the CurrentUsers DOB.
            DateTime dob = Convert.ToDateTime(CurrentUser.DOB);

            //Get the Day value from the CurrentUsers DOB, this is needed to display onscreen.
            DayDOB = (int)dob.Day;

            //Get the Month value from the CurrentUsers DOB, this is needed to display onscreen.
            MonthDOB = (int)dob.Month;

            //Get the Year value from the CurrentUsers DOB, this is needed to display onscreen.
            YearDOB = (int)dob.Year;

            //Set the ValidDOB property to true, this is needed for validation.
            ValidDOB = true;

            //Initialise a new ValidationContext to be used to validate the PersonalDetailsInput model only.
            ValidationContext validationContext = new ValidationContext(PersonalDetailsInput);

            //Create a collection to store the returned PersonalDetailsInput model validation results.
            ICollection<ValidationResult> validationResults = new List<ValidationResult>();

            //Carry out validation check on the PersonalDetailsInput model.
            bool isPersonalDetailsValid = Validator.TryValidateObject(PersonalDetailsInput, validationContext, validationResults, true);

            //User has changed at least 1 piece of information.
            if (PersonalDetailsInput.Prefix != null || PersonalDetailsInput.FirstName != null || PersonalDetailsInput.LastName != null || PersonalDetailsInput.DayDOB != null || PersonalDetailsInput.MonthDOB != null || PersonalDetailsInput.YearDOB != null)
            {
                //User has changed at least 1 part of DOB, there this date needs validated.
                if (PersonalDetailsInput.DayDOB != null || PersonalDetailsInput.MonthDOB != null || PersonalDetailsInput.YearDOB != null)
                {
                    //The entered year value is greater than the current year.
                    if (PersonalDetailsInput.YearDOB > DateTime.Now.Year)
                    {
                        //Set the InValidYearDOB to true.
                        InValidYearDOB = true;

                        //Set the InValidYearDOBMessage to an error message informing the user that the entered year cannot be in the future.
                        InValidYearDOBMessage = "New date of birth year cannot be in the future";

                        //Add the InValidYearDOBMessage to the validationResults.
                        validationResults.Add(new ValidationResult(InValidYearDOBMessage));
                    }

                    //The entered year is far in the past.
                    if (PersonalDetailsInput.YearDOB < 1900)
                    {
                        //Set the InValidYearDOB to true.
                        InValidYearDOB = true;

                        //Set the InValidYearDOBMessage to an error message informing the user that the entered year cannot be before 1900.
                        InValidYearDOBMessage = "New date of birth year must be at least after 1900";

                        //Add the InValidYearDOBMessage to the validationResults.
                        validationResults.Add(new ValidationResult(InValidYearDOBMessage));
                    }

                    //Create a string DOB by combining all the input DOB values.
                    string inputDOB = PersonalDetailsInput.YearDOB.ToString() + "-" + PersonalDetailsInput.MonthDOB.ToString() + "-" + PersonalDetailsInput.DayDOB.ToString();

                    //Declare a newDOB datetime, this is used as the return value of the attempted parse below.
                    DateTime newDOB;

                    //Attempt to parse the string DOB to a valid datetime.
                    ValidDOB = DateTime.TryParse(inputDOB, out newDOB);

                    //The parsed DOB is a valid datetime.
                    if (ValidDOB)
                    {
                        //Set the PersonalDetailsInput to the new DOB.
                        PersonalDetailsInput.DOB = newDOB;
                    }
                    //The parsed DOB is not a valid datetime.
                    else
                    {
                        //Add a new ValidationResult to the validationResults informing the user that the entered date is not valid.
                        validationResults.Add(new ValidationResult("New date of birth must contain a valid day, month, and year"));
                    }
                }

                //The PersonalDetailsInput model is valid.
                if (isPersonalDetailsValid)
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
                        //Set the PersonalDetailsInput model DOB to null to ensure fresh start.
                        PersonalDetailsInput.DOB = null;

                        //Add an error to the ModelState to inform the user that they have not entered a valid date.
                        ModelState.AddModelError(string.Empty, "New date of birth must contain day, month, and year");
                    }

                    //Either the DOB or the year are invalid.
                    if (!ValidDOB || InValidYearDOB)
                    {
                        //Return the Page();
                        return Page();
                    }

                    //The user has changed their DOB.
                    if (PersonalDetailsInput.DOB != null ) 
                    {
                        //Set the CurrentUser DOB to the new DOB.
                        CurrentUser.DOB = PersonalDetailsInput.DOB;
                    }

                    //The user has changed their Prefix.
                    if (PersonalDetailsInput.Prefix != null)
                    {
                        //Set the CurrentUser Prefix to the new Prefix.
                        CurrentUser.Prefix = PersonalDetailsInput.Prefix;
                    }

                    //The user has changed their FirstName.
                    if (PersonalDetailsInput.FirstName != null)
                    {
                        //Set the CurrentUser FirstName to the new FirstName.
                        CurrentUser.FirstName = PersonalDetailsInput.FirstName;
                    }

                    //The user has changed their LastName.
                    if (PersonalDetailsInput.LastName != null)
                    {
                        //Set the CurrentUser LastName to the new LastName.
                        CurrentUser.LastName = PersonalDetailsInput.LastName;
                    }


                    //Get JWT Token Claim from HttpContext Users Claims.
                    Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

                    //Assign value of JWT Token Claim to jwtToken string, needed for passing to API in header.
                    string? jwtToken = jwtTokenClaim.Value;

                    //Update the CurrentUser by calling the UpdateUser method form the _userService.
                    User updatedUser = await _userService.UpdateUser(CurrentUser, jwtToken);

                    //User has been successfully updated.
                    if (updatedUser != null) 
                    {
                        //Clear the current Session to ensure fresh start.
                        HttpContext.Session.Clear();

                        //Reset the "User" session object so that any changes are reflected on screen.
                        HttpContext.Session.SetInSession("User", CurrentUser);

                        //Set the CurrentUser property by getting the "User" object stored in session.
                        CurrentUser = HttpContext.Session.GetFromSession<User>("User");

                        //Get the CurrentUsers DOB.
                        dob = Convert.ToDateTime(CurrentUser.DOB);

                        //Get the Day value from the CurrentUsers DOB, this is needed to display onscreen.
                        DayDOB = (int)dob.Day;

                        //Get the Month value from the CurrentUsers DOB, this is needed to display onscreen.
                        MonthDOB = (int)dob.Month;

                        //Get the Year value from the CurrentUsers DOB, this is needed to display onscreen.
                        YearDOB = (int)dob.Year;

                        //Clear the PersonalDetailsInput model to ensure fresh start.
                        ModelState.Clear();
                        PersonalDetailsInput.Prefix = string.Empty;
                        PersonalDetailsInput.FirstName = string.Empty;
                        PersonalDetailsInput.LastName = string.Empty;
                        PersonalDetailsInput.DayDOB = null;
                        PersonalDetailsInput.MonthDOB = null;
                        PersonalDetailsInput.YearDOB = null;
                        PersonalDetailsInput.DOB = null;

                        //Set the UpdateSuccess property to true so the success message is shown onscreen.
                        UpdateSuccess = true;

                        //Return the Page.
                        return Page();
                    }
                    //User was not successfully updated.
                    else
                    {
                        //Keep TempData to ensure values can be reused.
                        TempData.Keep();

                        //Add an error to the ModelState to inform the user that their personal details have not been updated.
                        ModelState.AddModelError(string.Empty, "There was a problem updating your personal details.");

                        //Return the Page.
                        return Page();
                    }
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
            //User has not changed any information.
            else
            {
                //Add an error to the ModelState to inform the user that they have not changed any information.
                ModelState.AddModelError(string.Empty, "You have not changed any personal details");

                //Return the Page();
                return Page();
            }
        }

        //Method Summary:
        //This method is executed when the "Cancel" button on the personal details section is clicked.
        //When executed this method clears the changes made by the user to ensure a fresh start and displayed the personal details.
        public async Task<IActionResult> OnPostCancelPersonalDetailsUpdate()
        {
            //Set the CurrentUser property by getting the "User" object stored in session.
            CurrentUser = HttpContext.Session.GetFromSession<User>("User");

            //Get the CurrentUsers DOB.
            DateTime dob = Convert.ToDateTime(CurrentUser.DOB);

            //Get the Day value from the CurrentUsers DOB, this is needed to display onscreen.
            DayDOB = (int)dob.Day;

            //Get the Month value from the CurrentUsers DOB, this is needed to display onscreen.
            MonthDOB = (int)dob.Month;

            //Get the Year value from the CurrentUsers DOB, this is needed to display onscreen.
            YearDOB = (int)dob.Year;

            //Set the ValidDOB property to true, this is needed for validation.
            ValidDOB = true;

            //Clear the PersonalDetailsInput model to ensure fresh start.
            ModelState.Clear();
            PersonalDetailsInput.Prefix = string.Empty;
            PersonalDetailsInput.FirstName = string.Empty;
            PersonalDetailsInput.LastName = string.Empty;
            PersonalDetailsInput.DayDOB = null;
            PersonalDetailsInput.MonthDOB = null;
            PersonalDetailsInput.YearDOB = null;
            PersonalDetailsInput.DOB = null;

            //Set the UpdateSuccess property to false to ensure the success message does not show.
            UpdateSuccess = false;

            //Set the ShowPersonalDetails property to true.
            ShowPersonalDetails = true;

            //Return the Page();
            return Page();
        }
        #endregion Personal Details

        #region Contact Details
        //Method Summary:
        //This method is excuted when the "Contact details" link is clicked in the side navigation.
        //When excuted the "Contact details" section is displayed.
        public void OnGetShowContactDetails()
        {
            //Set the CurrentUser property by getting the "User" object stored in session.
            CurrentUser = HttpContext.Session.GetFromSession<User>("User");

            //Set the ShowContactDetails property to true.
            ShowContactDetails = true;

            //Clear TempData to ensure fresh start.
            TempData.Clear();
        }

        //Method Summary:
        //This method executes when the "Update" button is clicked in the "Contact details" section.
        //When executed the ContactDetailsInput model is validated and if valid the CurrentUser is updated.
        public async Task<IActionResult> OnPostUpdateContactDetails()
        {
            //Set the CurrentUser property by getting the "User" object stored in session.
            CurrentUser = HttpContext.Session.GetFromSession<User>("User");

            //Set the ShowContactDetails property to true.
            ShowContactDetails = true;

            //Initialise a new ValidationContext to be used to validate the ContactDetailsInput model only.
            ValidationContext validationContext = new ValidationContext(ContactDetailsInput);

            //Create a collection to store the returned ContactDetailsInput model validation results.
            ICollection<ValidationResult> validationResults = new List<ValidationResult>();

            //Carry out validation check on the ContactDetailsInput model.
            bool isContactDetailsValid = Validator.TryValidateObject(ContactDetailsInput, validationContext, validationResults, true);

            //The user has changed their contact number.
            if (ContactDetailsInput.ContactNumber != null)
            {
                //The ContactDetailsInput model is valid.
                if (isContactDetailsValid)
                {
                    //Set the CurrentUsers ContactNumber to the new ContactNumber
                    CurrentUser.ContactNumber = ContactDetailsInput.ContactNumber;

                    //Get JWT Token Claim from HttpContext Users Claims.
                    Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

                    //Assign value of JWT Token Claim to jwtToken string, needed for passing to API in header.
                    string? jwtToken = jwtTokenClaim.Value;

                    //Update the CurrentUser by calling the UpdateUser method form the _userService.
                    User updatedUser = await _userService.UpdateUser(CurrentUser, jwtToken);

                    //User has been successfully updated.
                    if (updatedUser != null)
                    {
                        //Clear the current Session to ensure fresh start.
                        HttpContext.Session.Clear();

                        //Reset the "User" session object so that any changes are reflected on screen.
                        HttpContext.Session.SetInSession("User", CurrentUser);

                        //Set the CurrentUser property by getting the "User" object stored in session.
                        CurrentUser = HttpContext.Session.GetFromSession<User>("User");

                        //Clear the ContactDetailsInput model to ensure fresh start.
                        ModelState.Clear();
                        ContactDetailsInput.ContactNumber = string.Empty;

                        //Set the UpdateSuccess property to true so the success message is shown onscreen.
                        UpdateSuccess = true;

                        //Return the Page.
                        return Page();
                    }
                    //User was not successfully updated.
                    else
                    {
                        //Keep TempData to ensure values can be reused.
                        TempData.Keep();

                        //Add an error to the ModelState to inform the user that their contact details have not been updated.
                        ModelState.AddModelError(string.Empty, "There was a problem updating your contact details.");

                        //Return the Page.
                        return Page();
                    }
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
            //The user has not changed their contact details.
            else
            {
                //Add an error to the ModelState to inform the user that they have not changed any information.
                ModelState.AddModelError(string.Empty, "You have not changed any contact details");

                //Return the Page();
                return Page();
            }
        }

        //Method Summary:
        //This method is executed when the "Cancel" button on the contact details section is clicked.
        //When executed this method clears the changes made by the user to ensure a fresh start and displayed the contact details.
        public async Task<IActionResult> OnPostCancelContactDetailsUpdate()
        {
            //Set the CurrentUser property by getting the "User" object stored in session.
            CurrentUser = HttpContext.Session.GetFromSession<User>("User");

            //Clear the ContactDetailsInput model to ensure fresh start.
            ModelState.Clear();
            ContactDetailsInput.ContactNumber = string.Empty;

            //Set the UpdateSuccess property to false to ensure the success message does not show.
            UpdateSuccess = false;

            //Set the ShowContactDetails property to true.
            ShowContactDetails = true;

            //Return the Page();
            return Page();
        }
        #endregion Contact Details

        #region Address Details
        //Method Summary:
        //This method is excuted when the "Address details" link is clicked in the side navigation.
        //When excuted the "Address details" section is displayed.
        public void OnGetShowAddressDetails()
        {
            //Set the CurrentUser property by getting the "User" object stored in session.
            CurrentUser = HttpContext.Session.GetFromSession<User>("User");

            //Set the ShowAddressDetails property to true.
            ShowAddressDetails = true;

            //Clear TempData to ensure fresh start.
            TempData.Clear();
        }

        //Method Summary:
        //This method executes when the "Update" button is clicked in the "Address details" section.
        //When executed the AddressDetailsInput model is validated and if valid the CurrentUser is updated.
        public async Task<IActionResult> OnPostUpdateAddressDetails()
        {
            //Set the CurrentUser property by getting the "User" object stored in session.
            CurrentUser = HttpContext.Session.GetFromSession<User>("User");

            //Set the ShowAddressDetails property to true.
            ShowAddressDetails = true;

            //Initialise a new ValidationContext to be used to validate the AddressDetailsInput model only.
            ValidationContext validationContext = new ValidationContext(AddressDetailsInput);

            //Create a collection to store the returned AddressDetailsInput model validation results.
            ICollection<ValidationResult> validationResults = new List<ValidationResult>();

            //Carry out validation check on the AddressDetailsInput model.
            bool isAddressDetailsValid = Validator.TryValidateObject(AddressDetailsInput, validationContext, validationResults, true);

            //User has changed at least 1 piece of information.
            if (AddressDetailsInput.Postcode != null || AddressDetailsInput.AddressLine1 != null || AddressDetailsInput.AddressLine2 != null || AddressDetailsInput.AddressLine3 != null)
            {
                //The AddressDetailsInput model is valid.
                if (isAddressDetailsValid)
                {
                    //User has changed their Postcode.
                    if (AddressDetailsInput.Postcode != null)
                    {
                        //Set the CurrentUser Postcode to the new Postcode.
                        CurrentUser.Postcode = AddressDetailsInput.Postcode;
                    }

                    //User has changed their AddressLine1.
                    if (AddressDetailsInput.AddressLine1 != null)
                    {
                        //Set the CurrentUser AddressLine1 to the new AddressLine1.
                        CurrentUser.AddressLine1 = AddressDetailsInput.AddressLine1;
                    }

                    //User has changed their AddressLine2.
                    if (AddressDetailsInput.AddressLine2 != null)
                    {
                        //Set the CurrentUser AddressLine2 to the new AddressLine2.
                        CurrentUser.AddressLine2 = AddressDetailsInput.AddressLine2;
                    }

                    //User has changed their AddressLine3.
                    if (AddressDetailsInput.AddressLine3 != null)
                    {
                        //Set the CurrentUser AddressLine3 to the new AddressLine3.
                        CurrentUser.AddressLine3 = AddressDetailsInput.AddressLine3;
                    }

                    //Get JWT Token Claim from HttpContext Users Claims.
                    Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

                    //Assign value of JWT Token Claim to jwtToken string, needed for passing to API in header.
                    string? jwtToken = jwtTokenClaim.Value;

                    //Update the CurrentUser by calling the UpdateUser method form the _userService.
                    User updatedUser = await _userService.UpdateUser(CurrentUser, jwtToken);

                    //User has been successfully updated.
                    if (updatedUser != null)
                    {

                    }
                    //User was not successfully updated.
                    else
                    {
                        //Keep TempData to ensure values can be reused.
                        TempData.Keep();

                        //Add an error to the ModelState to inform the user that their address details have not been updated.
                        ModelState.AddModelError(string.Empty, "There was a problem updating your address details.");

                        //Return the Page.
                        return Page();
                    }

                    //Clear the current Session to ensure fresh start.
                    HttpContext.Session.Clear();

                    //Reset the "User" session object so that any changes are reflected on screen.
                    HttpContext.Session.SetInSession("User", CurrentUser);

                    //Set the CurrentUser property by getting the "User" object stored in session.
                    CurrentUser = HttpContext.Session.GetFromSession<User>("User");

                    //Clear the AddressDetailsInput model to ensure fresh start.
                    ModelState.Clear();
                    AddressDetailsInput.Postcode = string.Empty;
                    AddressDetailsInput.AddressLine1 = string.Empty;
                    AddressDetailsInput.AddressLine2 = string.Empty;
                    AddressDetailsInput.AddressLine3 = null;

                    //Set the UpdateSuccess property to true so the success message is shown onscreen.
                    UpdateSuccess = true;

                    //Return the Page.
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
            //User has not changed any information.
            else
            {
                //Add an error to the ModelState to inform the user that they have not changed any information.
                ModelState.AddModelError(string.Empty, "You have not changed any address details");

                //Return the Page;
                return Page();
            }
        }

        //Method Summary:
        //This method is executed when the "Cancel" button on the address details section is clicked.
        //When executed this method clears the changes made by the user to ensure a fresh start and displayed the address details.
        public async Task<IActionResult> OnPostCancelAddressDetailsUpdate()
        {
            //Set the CurrentUser property by getting the "User" object stored in session.
            CurrentUser = HttpContext.Session.GetFromSession<User>("User");

            //Clear the AddressDetailsInput model to ensure fresh start.
            ModelState.Clear();
            AddressDetailsInput.Postcode = string.Empty;
            AddressDetailsInput.AddressLine1 = string.Empty;
            AddressDetailsInput.AddressLine2 = string.Empty;
            AddressDetailsInput.AddressLine3 = null;

            //Set the UpdateSuccess property to false to ensure the success message does not show.
            UpdateSuccess = false;

            //Set the ShowAddressDetails property to true.
            ShowAddressDetails = true;

            //Return the Page;
            return Page();
        }
        #endregion Address Details

        #region Delete Account
        //Method Summary:
        //This method is excuted when the "Delete account" link is clicked in the side navigation.
        //When excuted the "Delete account" section is displayed.
        public void OnGetShowDeleteDetails()
        {
            //Set the CurrentUser property by getting the "User" object stored in session.
            CurrentUser = HttpContext.Session.GetFromSession<User>("User");

            //Set the ShowDeleteDetails property to true.
            ShowDeleteDetails = true;

            //Set the ShowDeleteDetailsSure property to false.
            ShowDeleteDetailsSure = false;

            //Clear TempData to ensure fresh start.
            TempData.Clear();
        }

        //Method Summary:
        //This method executes when the "Delete account" button is clicked in the "Delete account" section.
        //When executed the "Are you sure you want to delete your account" section is shown.
        public async Task<IActionResult> OnPostDeleteAccount()
        {
            //Set the CurrentUser property by getting the "User" object stored in session.
            CurrentUser = HttpContext.Session.GetFromSession<User>("User");

            //Set the ShowDeleteDetails property to true.
            ShowDeleteDetails = true;

            //Set the ShowDeleteDetailsSure property to false.
            ShowDeleteDetailsSure = true;

            //return the Page.
            return Page();
        }

        //Method Summary:
        //This method executes when the "Delete account" button is clicked in the "Are you sure you want to delete your account" section.
        //When executed the users identity details are nulled in the DB and a confirmation of account deletion is sent to the user.
        public async Task<IActionResult> OnPostDeleteAccountSure()
        {
            //Set the CurrentUser property by getting the "User" object stored in session.
            CurrentUser = HttpContext.Session.GetFromSession<User>("User");

            //Get JWT Token Claim from HttpContext Users Claims.
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

            //Assign value of JWT Token Claim to jwtToken string, needed for passing to API in header.
            string? jwtToken = jwtTokenClaim.Value;

            //Attempt to delete the users identity details by calling the DeleteUser method in the _userService.
            int deletedUserID = await _userService.DeleteUser(CurrentUser.ID, jwtToken);

            //The users identity details have been successfully deleted.
            if (deletedUserID != 0)
            {
                //Declare new Response to store the reponse from the email service and populate by calling the SendAccountDeletionEmail method.
                Response emailResponse = await SendAccountDeletionEmail(CurrentUser.Email);

                //Log the current user out of the application.
                await HttpContext.SignOutAsync();

                _logger.LogInformation("User deleted account and signed out.");

                //Clear the current session to ensure fresh start.
                HttpContext.Session.Clear();

                //Clear TempData to ensure fresh start.
                TempData.Clear();

                //Redirect to the Index/Home page.
                return Redirect("/Index");
            }
            //The users identity details have not been deleted.
            else
            {
                //Add an error to the ModelState to inform the user that their account cannot be deleted.
                ModelState.AddModelError(string.Empty, "There was a problem deleting your account.");

                //Return the Page.
                return Page();
            }
        }

        //Method Summary:
        //This method is executed when the "Cancel" button on the "Are you sure you want to delete your account" section is clicked.
        //When executed this method clears the changes made by the user to ensure a fresh start and display the delete account section.
        public async Task<IActionResult> OnPostCancelDeleteAccount()
        {
            //Set the CurrentUser to the "User" session object.
            CurrentUser = HttpContext.Session.GetFromSession<User>("User");


            //Set the ShowDeleteDetails property to true.
            ShowDeleteDetails = true;

            //Set the ShowDeleteDetailsSure property to false.
            ShowDeleteDetailsSure = false;

            //Return the Page.
            return Page();
        }

        //Method Summary:
        //This method is executed when the "Delete account" button is clicked in the "Are you sure you want to delete your account" section.
        //When executed this method attempts to send an email to the user and returns the response from the _emailService.
        public async Task<Response> SendAccountDeletionEmail(string emailAddress)
        {
            //Set the subject of the email explaining that the user has deleted their account.
            string subject = "DFI Fault Reporting: Account Deleted";

            //Declare a new EmailAddress object and assign the users email address as the value.
            EmailAddress to = new EmailAddress(emailAddress);

            //Set textContent to empty string as it will not be used here.
            string textContent = string.Empty;

            //Set the htmlContent to a message explaining to the user that their account has been successfully deleted.
            string htmlContent = "<p>Hello,</p><p>Your account has been successfully deleted.</p><p>Thank you for using the service.</p>";

            //Set the attachment to null as it will not be used here.
            Attachment? attachment = null;

            //Call the SendEmail in the _emailService and return the response.
            return await _emailService.SendEmail(subject, to, textContent, htmlContent, attachment);
        }
        #endregion Delete Account
    }
}