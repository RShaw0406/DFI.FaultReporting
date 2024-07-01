using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Emails;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Tokens;
using DFI.FaultReporting.Services.Interfaces.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using DFI.FaultReporting.Common.SessionStorage;
using DFI.FaultReporting.Services.Users;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using SendGrid.Helpers.Mail;
using System.Security.Cryptography;
using SendGrid;

namespace DFI.FaultReporting.Admin.Pages.Account
{
    public class DetailsModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<DetailsModel> _logger;
        private readonly IStaffService _staffService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISettingsService _settingsService;
        private readonly IEmailService _emailService;
        private readonly IVerificationTokenService _verificationTokenService;

        //Inject dependencies in constructor.
        public DetailsModel(ILogger<DetailsModel> logger, IStaffService staffService, IHttpContextAccessor httpContextAccessor, ISettingsService settingsService, IEmailService emailService,
            IVerificationTokenService verificationTokenService)
        {
            _logger = logger;
            _staffService = staffService;
            _httpContextAccessor = httpContextAccessor;
            _settingsService = settingsService;
            _emailService = emailService;
            _verificationTokenService = verificationTokenService;
        }
        #endregion Dependency Injection

        #region Properties
        //Declare CurrentStaff property, this is needed when calling the _staffService.
        public Staff CurrentStaff { get; set; }

        //Declare AccountDetailsInput property, this is needed when updating account details.
        [BindProperty]
        public AccountDetailsInputModel AccountDetailsInput { get; set; }

        //Declare VerificationCodeInput property, this is needed when inputting sent verification code when updating account details.
        [BindProperty]
        public VerificationCodeModel VerificationCodeInput { get; set; }

        //Declare PersonalDetailsInput property, this is needed when updating personal details.
        [BindProperty]
        public PersonalDetailsInputModel PersonalDetailsInput { get; set; }

        //Declare ShowAccountDetails property, this is needed for displaying account details section. 
        public bool ShowAccountDetails { get; set; }

        //Declare ShowPersonalDetails property, this is needed for displaying personal details section.
        public bool ShowPersonalDetails { get; set; }

        //Declare VerificationCodeSent property, this is needed for displaying the section for inputting the verification code.
        public bool VerificationCodeSent { get; set; }

        //Declare UpdateSuccess property, this is needed for displaying the updated success message.
        public bool UpdateSuccess { get; set; }

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
            [StringLength(8, ErrorMessage = "New title must not be more than 8 characters")]
            public string? Prefix { get; set; }

            [DisplayName("New first name")]
            [RegularExpression(@"^[a-zA-Z''-'\s]{1,125}$", ErrorMessage = "New first name must not contain special characters or numbers")]
            [StringLength(125, ErrorMessage = "New first name must not be more than 125 characters")]
            public string? FirstName { get; set; }

            [DisplayName("New last name")]
            [RegularExpression(@"^[a-zA-Z''-'\s]{1,125}$", ErrorMessage = "New last name must not contain special characters or numbers")]
            [StringLength(125, ErrorMessage = "New last name must not be more than 125 characters")]
            public string? LastName { get; set; }
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
                    //Get the ID from the contexts current user, needed for populating CurrentStaff property from DB.
                    string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

                    //Get the JWT token claim from the contexts current user, needed for populating CurrentStaff property from DB.
                    Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

                    //Set the jwtToken string to the JWT token claims value, needed for populating CurrentStaff property from DB.
                    string? jwtToken = jwtTokenClaim.Value;

                    //Set the CurrentStaff property by calling the GetStaff method in the _staffService.
                    CurrentStaff = await _staffService.GetStaff(Convert.ToInt32(userID), jwtToken);

                    //Set the CurrentStaff in session, needed for displaying details.
                    HttpContext.Session.SetInSession("Staff", CurrentStaff);

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
            //Set the CurrentStaff property by getting the "Staff" object stored in session.
            CurrentStaff = HttpContext.Session.GetFromSession<Staff>("Staff");

            //Set the ShowAccountDetails property to true.
            ShowAccountDetails = true;

            //Clear all TempData to ensure a fresh start.
            TempData.Clear();
        }

        //Method Summary:
        //This method is executed when the "Request verification code" button is clicked in the "Account details" section.
        //When executed the AccountDetailsInput model is validated and if valid an email containing a generated 6 digit code is sent to the staff.
        public async Task<IActionResult> OnPostRequestVerificationCode()
        {
            //Set the ShowAccountDetails property to true so that the section remains visible after the post.
            ShowAccountDetails = true;

            //Set the CurrentStaff property by getting the "Staff" object stored in session so that the property remains populated after the post.
            CurrentStaff = HttpContext.Session.GetFromSession<Staff>("Staff");

            //Set the AccountDetailsInput model in session so that it can be accessed again after the post.
            HttpContext.Session.SetInSession("AccountDetailsInputModel", AccountDetailsInput);

            //The "NewEmail" or the "NewPassword" and "ConfirmPassword" properties of the AccountDetailsInput model are not null.
            //The staff has changed at least 1 piece of information.
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
                        //Set the emailAddress string to the staff current email address.
                        emailAddress = CurrentStaff.Email;
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
        //When executed the VerificationCodeInput model is validated and if valid the CurrentStaff is updated.
        public async Task<IActionResult> OnPostUpdateAccountDetails()
        {
            //Set VerificationCodeSent to true, this is needed to ensure verification code input remains displayed if an error occurs.
            VerificationCodeSent = true;

            //Set ShowAccountDetails to true, this is needed to ensure the "Account details" section remains displayed.
            ShowAccountDetails = true;

            //Set the CurrentStaff property to the "Staff" session object.
            CurrentStaff = HttpContext.Session.GetFromSession<Staff>("Staff");

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
                    //Assign the CurrentStaff email to the AccountDetailsInput email.
                    CurrentStaff.Email = AccountDetailsInput.NewEmail;
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

                    //Set the CurrentStaff password to the hashed password.
                    CurrentStaff.Password = passwordHash;

                    //Set the CurrentStaff salt to the generated salt, must convert to base64 string to enable the API to consume request.
                    //PassSalt needs to be stored to enable passowrd verification when logging in.
                    CurrentStaff.PasswordSalt = Convert.ToBase64String(salt);
                }

                //Update the CurrentStaff by calling the UpdateStaff method form the _staffService.
                Staff updatedStaff = await _staffService.UpdateStaff(CurrentStaff, jwtToken);

                //User has been successfully updated.
                if (updatedStaff != null)
                {
                    //Clear the current Session to ensure fresh start.
                    HttpContext.Session.Clear();

                    //Reset the "Staff" session object so that any changes are reflected on screen.
                    HttpContext.Session.SetInSession("Staff", CurrentStaff);

                    //Repopulate the CurrentStaff class property so that any changes are reflected in class property values.
                    CurrentStaff = HttpContext.Session.GetFromSession<Staff>("Staff");

                    //Create a claimsIdentity to store the updated HttpContext User claims, this is needed to ensure that claims are updated to reflect ay changes.
                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);

                    //Add the new email address to the claimsIdentity to ensure it is included instead of the old email address.
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, CurrentStaff.Email));

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
        //When executed this method clears the changes made by the staff to ensure a fresh start and displayed the account details.
        public async Task<IActionResult> OnPostCancelAccountDetailsUpdate()
        {
            //Set the CurrentStaff to the "Staff" session object.
            CurrentStaff = HttpContext.Session.GetFromSession<Staff>("Staff");

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
            //Set the CurrentStaff property by getting the "Staff" object stored in session.
            CurrentStaff = HttpContext.Session.GetFromSession<Staff>("Staff");

            //Set the ShowPersonalDetails property to true.
            ShowPersonalDetails = true;

            //Clear TempDate to ensure fresh start.
            TempData.Clear();
        }

        //Method Summary:
        //This method executes when the "Update" button is clicked in the "Personal details" section.
        //When executed the PersonalDetailsInput model is validated and if valid the CurrentStaff is updated.
        public async Task<IActionResult> OnPostUpdatePersonalDetails()
        {
            //Set the ShowPersonalDetails property to true.
            ShowPersonalDetails = true;

            //Set the CurrentStaff property by getting the "Staff" object stored in session.
            CurrentStaff = HttpContext.Session.GetFromSession<Staff>("Staff");

            //Initialise a new ValidationContext to be used to validate the PersonalDetailsInput model only.
            ValidationContext validationContext = new ValidationContext(PersonalDetailsInput);

            //Create a collection to store the returned PersonalDetailsInput model validation results.
            ICollection<ValidationResult> validationResults = new List<ValidationResult>();

            //Carry out validation check on the PersonalDetailsInput model.
            bool isPersonalDetailsValid = Validator.TryValidateObject(PersonalDetailsInput, validationContext, validationResults, true);

            //User has changed at least 1 piece of information.
            if (PersonalDetailsInput.Prefix != null || PersonalDetailsInput.FirstName != null || PersonalDetailsInput.LastName != null)
            {              
                //The PersonalDetailsInput model is valid.
                if (isPersonalDetailsValid)
                {
                    //The staff has changed their Prefix.
                    if (PersonalDetailsInput.Prefix != null)
                    {
                        //Set the CurrentStaff Prefix to the new Prefix.
                        CurrentStaff.Prefix = PersonalDetailsInput.Prefix;
                    }

                    //The staff has changed their FirstName.
                    if (PersonalDetailsInput.FirstName != null)
                    {
                        //Set the CurrentStaff FirstName to the new FirstName.
                        CurrentStaff.FirstName = PersonalDetailsInput.FirstName;
                    }

                    //The staff has changed their LastName.
                    if (PersonalDetailsInput.LastName != null)
                    {
                        //Set the CurrentStaff LastName to the new LastName.
                        CurrentStaff.LastName = PersonalDetailsInput.LastName;
                    }


                    //Get JWT Token Claim from HttpContext Users Claims.
                    Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

                    //Assign value of JWT Token Claim to jwtToken string, needed for passing to API in header.
                    string? jwtToken = jwtTokenClaim.Value;

                    //Update the CurrentStaff by calling the UpdateStaff method form the _staffService.
                    Staff updatedStaff = await _staffService.UpdateStaff(CurrentStaff, jwtToken);

                    //Staff has been successfully updated.
                    if (updatedStaff != null)
                    {
                        //Clear the current Session to ensure fresh start.
                        HttpContext.Session.Clear();

                        //Reset the "Staff" session object so that any changes are reflected on screen.
                        HttpContext.Session.SetInSession("Staff", CurrentStaff);

                        //Set the CurrentStaff property by getting the "Staff" object stored in session.
                        CurrentStaff = HttpContext.Session.GetFromSession<Staff>("Staff");


                        //Clear the PersonalDetailsInput model to ensure fresh start.
                        ModelState.Clear();
                        PersonalDetailsInput.Prefix = string.Empty;
                        PersonalDetailsInput.FirstName = string.Empty;
                        PersonalDetailsInput.LastName = string.Empty;

                        //Set the UpdateSuccess property to true so the success message is shown onscreen.
                        UpdateSuccess = true;

                        //Return the Page.
                        return Page();
                    }
                    //Staff was not successfully updated.
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
        //When executed this method clears the changes made by the staff to ensure a fresh start and displayed the personal details.
        public async Task<IActionResult> OnPostCancelPersonalDetailsUpdate()
        {
            //Set the CurrentStaff property by getting the "Staff" object stored in session.
            CurrentStaff = HttpContext.Session.GetFromSession<Staff>("Staff");

            //Clear the PersonalDetailsInput model to ensure fresh start.
            ModelState.Clear();
            PersonalDetailsInput.Prefix = string.Empty;
            PersonalDetailsInput.FirstName = string.Empty;
            PersonalDetailsInput.LastName = string.Empty;

            //Set the UpdateSuccess property to false to ensure the success message does not show.
            UpdateSuccess = false;

            //Set the ShowPersonalDetails property to true.
            ShowPersonalDetails = true;

            //Return the Page();
            return Page();
        }
        #endregion Personal Details
    }
}
