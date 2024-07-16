using DFI.FaultReporting.Services.Interfaces.Emails;
using DFI.FaultReporting.Services.Interfaces.Roles;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Tokens;
using DFI.FaultReporting.Services.Interfaces.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SendGrid.Helpers.Mail;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using SendGrid;

namespace DFI.FaultReporting.Admin.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly IStaffService _staffService;
        private IRoleService _roleService;
        private readonly ILogger<ResetPasswordModel> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISettingsService _settingsService;
        private readonly IEmailService _emailService;
        private readonly IVerificationTokenService _verificationTokenService;

        //Inject dependencies in constructor.
        public ResetPasswordModel(IStaffService staffService, IRoleService roleService, ILogger<ResetPasswordModel> logger, IHttpContextAccessor httpContextAccessor,
            ISettingsService settingsService, IEmailService emailService, IVerificationTokenService verificationTokenService)
        {
            _staffService = staffService;
            _roleService = roleService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _settingsService = settingsService;
            _emailService = emailService;
            _verificationTokenService = verificationTokenService;
        }
        #endregion Dependency Injection

        #region Properties
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
        }

        public class VerificationCodeModel
        {
            [Required]
            [DisplayName("Verification code")]
            public string? VerificationCode { get; set; }
        }

        [BindProperty]
        public bool Verified { get; set; }

        [BindProperty]
        public bool PasswordReset { get; set; }

        [BindProperty]
        public bool EmailExists { get; set; }

        [BindProperty]
        public PasswordInputModel passwordInput { get; set; }

        public class PasswordInputModel
        {
            [Required]
            [DisplayName("Password")]
            // Password should contain the following:
            // At least 1 number
            // At least 1 special character
            // At least 1 uppercase letter
            // At least 1 lowercase letter
            // At least 8 characters in total
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[#$^+=!*()@%&]).{8,}$", ErrorMessage = "Password does not meet all requirements")]
            [DataType(DataType.Password)]
            public string? Password { get; set; }

            [Required]
            [Display(Name = "Confirm password")]
            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match")]
            public string? ConfirmPassword { get; set; }
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

        #region Request Verification Code
        //Method Summary:
        //This method is excuted when the "Request verification code" button is clicked on the "Enter your login details" section.
        //When excuted the "Enter your verification code" section is displayed.
        public async Task<IActionResult> OnPostRequestVerificationCode()
        {
            //User has entered both their email and password.
            if (loginInput.Email != null)
            {
                EmailExists = await _staffService.CheckEmail(loginInput.Email);

                if (EmailExists == false)
                {
                    ModelState.AddModelError(string.Empty, "Email does not exist");
                    return Page();
                }
                else
                {
                    //Get a new verification code by calling the GenerateToken method in the _verificationTokenService.
                    int verficationToken = await _verificationTokenService.GenerateToken();

                    //Set the verificationCodeSent property to true as this will be needed to show the textbox for the user to input the code they received.
                    verificationCodeSent = true;

                    //Set the sent verification code in TempData to be user for matching later.
                    TempData["LoginEmail"] = loginInput.Email;
                    TempData["VerificationToken"] = verficationToken;
                    TempData["VerificationCodeSent"] = verificationCodeSent;

                    //Keep TempData incase it is needed again.
                    TempData.Keep();

                    //Declare new Response to store the reponse from the email service and populate by calling the SendVerificationCode method.
                    Response emailResponse = await SendVerificationCode(loginInput.Email, verficationToken);

                    //The email has successfully been sent.
                    if (emailResponse.IsSuccessStatusCode)
                    {
                        //Set the verificationCodeSent property to true as this will be needed to show the textbox for the user to input the code they received.
                        verificationCodeSent = true;

                        //Set the sent verification code in TempData to be user for matching later.
                        TempData["VerificationToken"] = verficationToken;
                        TempData["VerificationCodeSent"] = verificationCodeSent;

                        //Keep TempData incase it is needed again.
                        TempData.Keep();
                    }
                    //The email has not been sent successfully.
                    else
                    {
                        //Set the verificationCodeSent property to false to ensure the enter login details section remains visible.
                        verificationCodeSent = false;

                        //Keep TempData incase it is needed again.
                        TempData.Keep();

                        //Add an error to the ModelState to inform the user that the email was not sent.
                        ModelState.AddModelError(string.Empty, "There was a problem sending the verification code");

                        //Return the Page.
                        return Page();
                    }
                }
            }

            //Keep TempData incase it is needed again.
            TempData.Keep();

            //Return the Page.
            return Page();
        }
        #endregion Request Verification Code

        #region Verify Code
        //Method Summary:
        //This method executes when the "Verify" button is clicked in the "Enter your verification code" section.
        //When executed the VerificationCodeInput model is validated and if valid the CurrentUser logged in.
        public async Task<IActionResult> OnPostVerify()
        {
            //Set the verificationCodeSent property value to the value stored in TempData.
            verificationCodeSent = Boolean.Parse(TempData["VerificationCodeSent"].ToString());

            //User has entered a verification code.
            if (verificationCodeInput.VerificationCode != null)
            {
                //The entered verification code matches the sent verification code.
                if (verificationCodeInput.VerificationCode == TempData["VerificationToken"].ToString())
                {
                    //Set the Verified property to true
                    Verified = true;
                    TempData["Verified"] = Verified;
                    TempData.Keep();
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
        #endregion Verify Code

        #region Reset Password
        //Method Summary:
        //This method is executed when the "Reset Password" button is clicked in the "Enter your new password" section.
        //When executed the PasswordInput model is validated and if valid the users password is reset.
        public async Task<IActionResult> OnPostResetPassword()
        {
            //Set the Verified property value to the value stored in TempData.
            Verified = Boolean.Parse(TempData["Verified"].ToString());

            verificationCodeSent = Boolean.Parse(TempData["VerificationCodeSent"].ToString());

            //Initialise a new ValidationContext to be used to validate the passwordInput model only.
            ValidationContext validationContext = new ValidationContext(passwordInput);

            //Create a collection to store the returned Step1Input model validation results.
            ICollection<ValidationResult> validationResults = new List<ValidationResult>();

            //Carry out validation check on the passwordInput model.
            bool isPasswordInputValid = Validator.TryValidateObject(passwordInput, validationContext, validationResults, true);

            //The passwordInput model is valid.
            if (isPasswordInputValid)
            {
                //The user has been verified.
                if (Verified == true)
                {
                    //Get the email address from TempData.
                    string email = TempData["LoginEmail"].ToString();

                    //Get the password from the passwordInput model.
                    string password = passwordInput.Password;

                    //Call the ResetPassword method in the _userService and return the response.
                    bool PasswordReset = await _staffService.ResetPassword(email, password);

                    //The password has been reset successfully.
                    if (PasswordReset)
                    {
                        TempData.Clear();

                        return RedirectToPage("/Account/Login");
                    }
                    //The password has not been reset successfully.
                    else
                    {
                        //Keep TempData incase its needed again.
                        TempData.Keep();

                        //Add an error to the ModelState to inform the user that the password was not reset.
                        ModelState.AddModelError(string.Empty, "There was a problem resetting the password");

                        //Return the Page.
                        return Page();
                    }
                }
                //The user has not been verified.
                else
                {
                    //Keep TempData incase its needed again.
                    TempData.Keep();

                    //Add an error to the ModelState to inform the user that they need to verify their account.
                    ModelState.AddModelError(string.Empty, "You need to verify your account");

                    //Return the Page.
                    return Page();
                }
            }
            //The passwordInput model is not valid.
            else
            {
                //Keep TempData incase its needed again.
                TempData.Keep();

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
        #endregion Reset Password

        #region Email
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
        #endregion Emai
    }
}
