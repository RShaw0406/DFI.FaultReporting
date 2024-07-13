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
        private readonly IEmailService _emailService;
        private readonly IVerificationTokenService _verificationTokenService;

        //Inject dependencies in constructor.
        public DetailsModel(ILogger<DetailsModel> logger, IStaffService staffService, IHttpContextAccessor httpContextAccessor, IEmailService emailService,
            IVerificationTokenService verificationTokenService)
        {
            _logger = logger;
            _staffService = staffService;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
            _verificationTokenService = verificationTokenService;
        }
        #endregion Dependency Injection

        #region Properties
        public Staff CurrentStaff { get; set; }

        [BindProperty]
        public AccountDetailsInputModel AccountDetailsInput { get; set; }

        [BindProperty]
        public VerificationCodeModel VerificationCodeInput { get; set; }

        [BindProperty]
        public PersonalDetailsInputModel PersonalDetailsInput { get; set; }

        public bool ShowAccountDetails { get; set; }

        public bool ShowPersonalDetails { get; set; }

        public bool VerificationCodeSent { get; set; }

        public bool UpdateSuccess { get; set; }

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

        public class VerificationCodeModel
        {
            public string? EmailVerificationCode { get; set; }

            [Required]
            [DisplayName("Verification code")]
            [Compare("EmailVerificationCode", ErrorMessage = "The verification codes do not match")]
            public string? VerificationCode { get; set; }
        }

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
            //The contexts current user exists.
            if (_httpContextAccessor.HttpContext.User != null)
            {
                //The contexts current user has been authenticated
                if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true)
                {
                    //Clear session and temp data to ensure fresh start.
                    HttpContext.Session.Clear();
                    TempData.Clear();

                    //Get the ID and JWT token from the contexts current user, needed for populating CurrentStaff property from DB.
                    string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                    Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
                    string? jwtToken = jwtTokenClaim.Value;

                    //Set the CurrentStaff property by calling the GetStaff method in the _staffService and set in session.
                    CurrentStaff = await _staffService.GetStaff(Convert.ToInt32(userID), jwtToken);
                    HttpContext.Session.SetInSession("Staff", CurrentStaff);

                    //Ensure UpdateSuccess message is not showing.
                    UpdateSuccess = false;

                    //Set ShowAccountDetails, this is the default view a user has.
                    ShowAccountDetails = true;

                    return Page();
                }
                else
                {
                    return Redirect("/NoPermission");
                }
            }
            else
            {
                return Redirect("/NoPermission");
            }
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
                //Validate the AccountDetailsInput model only.
                ValidationContext validationContext = new ValidationContext(AccountDetailsInput);
                ICollection<ValidationResult> validationResults = new List<ValidationResult>();
                bool isAccountDetailsValid = Validator.TryValidateObject(AccountDetailsInput, validationContext, validationResults, true);

                //The AccountDetailsInput model is valid.
                if (isAccountDetailsValid)
                {
                    //Generate a verification code by calling the GenerateToken() method from the _verificationTokenService.
                    int verificationToken = await _verificationTokenService.GenerateToken();

                    string emailAddress = string.Empty;

                    //The user has changed their email address.
                    if (AccountDetailsInput.NewEmail != null)
                    {
                        //Set the emailAddress string to the new email address.
                        emailAddress = AccountDetailsInput.NewEmail;
                    }
                    else
                    {
                        //Set the emailAddress string to the staff current email address.
                        emailAddress = CurrentStaff.Email;
                    }

                    //Attempt to send the verification code email to the user.
                    Response emailResponse = await SendVerificationCode(emailAddress, verificationToken);

                    if (emailResponse.IsSuccessStatusCode)
                    {
                        //Set the VerificationCodeSent property to true as this will be needed to show the textbox for the user to input the code they received.
                        VerificationCodeSent = true;

                        //Store the verificationToken in TempData as it will be needed to check against the code input by the user.
                        TempData["VerificationToken"] = verificationToken;
                        //Store the VerificationCodeSent property value in TempData as it will be needed to ensure the code input textboxe remains visible if an error occurs.
                        TempData["VerificationCodeSent"] = VerificationCodeSent;
                        TempData.Keep();
                    }
                    else
                    {
                        //Set the VerificationCodeSent property to false to ensure the "Account details" section remains visible.
                        VerificationCodeSent = false;

                        TempData.Clear();

                        ModelState.AddModelError(string.Empty, "There was a problem sending the verification code");

                        return Page();
                    }
                }
                else
                {
                    //Set the VerificationCodeSent property to false to ensure the "Account details" section remains visible.
                    VerificationCodeSent = false;

                    TempData.Clear();

                    //Display each of the validation errors.
                    foreach (ValidationResult validationResult in validationResults)
                    {
                        ModelState.AddModelError(string.Empty, validationResult.ErrorMessage);
                    }
                    return Page();
                }
            }
            else
            {
                //Set the VerificationCodeSent property to false to ensure the "Account details" section remains visible.
                VerificationCodeSent = false;

                TempData.Clear();

                ModelState.AddModelError(string.Empty, "You have not changed any account details");

                return Page();
            }

            TempData.Keep();

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

            //Validate the VerificationCodeInput model only.
            ValidationContext validationContext = new ValidationContext(VerificationCodeInput);
            ICollection<ValidationResult> validationResults = new List<ValidationResult>();
            bool isVerificationCodeValid = Validator.TryValidateObject(VerificationCodeInput, validationContext, validationResults, true);

            //User has input the correct verification code.
            if (isVerificationCodeValid)
            {
                //Get JWT Token Claim from HttpContext Users Claims.
                Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
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
                    //Reset the "Staff" session object so that any changes are reflected on screen.
                    HttpContext.Session.Clear();
                    HttpContext.Session.SetInSession("Staff", CurrentStaff);

                    //Repopulate the CurrentStaff class property so that any changes are reflected in class property values.
                    CurrentStaff = HttpContext.Session.GetFromSession<Staff>("Staff");

                    //Create a claimsIdentity to store the updated HttpContext User claims, this is needed to ensure that claims are updated to reflect ay changes.
                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, CurrentStaff.Email));

                    //Add the new email claim to the claimsIdentity.
                    foreach (Claim claim in HttpContext.User.Claims)
                    {
                        if (claim.Type != ClaimTypes.Name)
                        {
                            claimsIdentity.AddClaim(claim);
                        }
                    }

                    //Create a ClaimsPrinicipal to store the claimsIdentity, this is needed for re-logging the user into the application.
                    ClaimsPrincipal currentUser = new ClaimsPrincipal();
                    currentUser.AddIdentity(claimsIdentity);

                    //Get the expires claim, this is needed to ensure that the re-login session expires at the same time as the previous.
                    Claim expiresClaim = currentUser.FindFirst("exp");
                    long ticks = long.Parse(expiresClaim.Value);
                    DateTime? Expires = DateTimeOffset.FromUnixTimeSeconds(ticks).LocalDateTime;

                    //Initialise a new AuthenticationProperties instance, this is needed when re-logging the user into the application.
                    AuthenticationProperties authenticationProperties = new AuthenticationProperties();
                    authenticationProperties.ExpiresUtc = Expires;

                    //Sign the user out.
                    await HttpContext.SignOutAsync();

                    //Sign the user in, this is needed to ensure that the new email address is included in the claims which will mean it can be used throughout the rest of the application.
                    HttpContext.User = currentUser;
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, currentUser, authenticationProperties);

                    //Set the VerificationCodeSent property to false to ensure the veirification code input section is no longer shown.
                    VerificationCodeSent = false;

                    //Clear data to ensure fresh start.
                    AccountDetailsInput.NewEmail = string.Empty;
                    AccountDetailsInput.NewPassword = string.Empty;
                    AccountDetailsInput.ConfirmPassword = string.Empty;
                    TempData.Clear();

                    //Set the UpdateSuccess property to true so the success message is shown onscreen.
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

                //Display each of the validation errors.
                foreach (ValidationResult validationResult in validationResults)
                {
                    ModelState.AddModelError(string.Empty, validationResult.ErrorMessage);
                }

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

            //Clear the AccountDetailsInput model data to ensure fresh start.
            ModelState.Clear();
            AccountDetailsInput.NewEmail = string.Empty;
            AccountDetailsInput.NewPassword = string.Empty;
            AccountDetailsInput.ConfirmPassword = string.Empty;
            TempData.Clear();

            //Set the UpdateSuccess property to false to ensure the success message does not show.
            UpdateSuccess = false;

            //Set the VerificationCodeSent property to false to esnure the verification code input section is not shown.
            VerificationCodeSent = false;

            //Set the ShowAccountDetails property to true to ensure that the users account details section is shown.
            ShowAccountDetails = true;

            return Page();
        }


        //Method Summary:
        //This method is executed when the "Request verification code" button is clicked in the account details section.
        //When executed this method attempts to send a verification code email to the user and returns the response from the _emailService.
        public async Task<Response> SendVerificationCode(string emailAddress, int verficationToken)
        {
            //Attempt to send the verification code email to the user.
            EmailAddress to = new EmailAddress(emailAddress);
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

            //Validate the PersonalDetailsInput model only.
            ValidationContext validationContext = new ValidationContext(PersonalDetailsInput);
            ICollection<ValidationResult> validationResults = new List<ValidationResult>();
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
                    string? jwtToken = jwtTokenClaim.Value;

                    //Update the CurrentStaff by calling the UpdateStaff method form the _staffService.
                    Staff updatedStaff = await _staffService.UpdateStaff(CurrentStaff, jwtToken);

                    //Staff has been successfully updated.
                    if (updatedStaff != null)
                    {
                        //Reset the "Staff" session object so that any changes are reflected on screen.
                        HttpContext.Session.Clear();
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

                        return Page();
                    }
                    else
                    {
                        TempData.Keep();

                        ModelState.AddModelError(string.Empty, "There was a problem updating your personal details.");

                        return Page();
                    }
                }
                else
                {
                    //Display each of the validation errors.
                    foreach (ValidationResult validationResult in validationResults)
                    {
                        ModelState.AddModelError(string.Empty, validationResult.ErrorMessage);
                    }

                    return Page();
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "You have not changed any personal details");

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

            return Page();
        }
        #endregion Personal Details
    }
}
