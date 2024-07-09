using DFI.FaultReporting.JWT.Requests;
using DFI.FaultReporting.Services.Interfaces.Admin;
using DFI.FaultReporting.Services.Interfaces.Emails;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Tokens;
using DFI.FaultReporting.Services.Interfaces.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using SendGrid;
using SendGrid.Helpers.Mail;
using DFI.FaultReporting.Common.SessionStorage;

namespace DFI.FaultReporting.Public.Pages.Account.Register
{
    public class Step4Model : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<Step4Model> _logger;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISettingsService _settingsService;
        private readonly IEmailService _emailService;
        private readonly IVerificationTokenService _verificationTokenService;
        private readonly IContractorService _contractorService;

        //Inject dependencies in constructor.
        public Step4Model(ILogger<Step4Model> logger, IUserService userService, IHttpContextAccessor httpContextAccessor, ISettingsService settingsService, IEmailService emailService,
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

        #region Verification Code
        //Method Summary:
        //This method is excuted when the "Request verification code" button is clicked.
        //When executed a verification code is generated and sent to the user.
        public async Task<IActionResult> OnPostRequestVerificationCode()
        {
            //Get a new verification code by calling the GenerateToken method in the _verificationTokenService.
            int verficationToken = await _verificationTokenService.GenerateToken();

            //Get the registration request from "RegistrationRequest" object stored in session.
            RegistrationRequest? sessionRegistrationRequest = HttpContext.Session.GetFromSession<RegistrationRequest>("RegistrationRequest");

            //Declare new Response to store the reponse from the email service and populate by calling the SendVerificationCode method.
            Response emailResponse = await SendVerificationCode(sessionRegistrationRequest.Email, verficationToken);

            //The email has successfully been sent.
            if (emailResponse.IsSuccessStatusCode)
            {
                //Add the verification code to the TempData, this is used to verify the user when they enter the code.
                TempData["VerificationToken"] = verficationToken;

                //Keep TempData incase it is needed again.
                TempData.Keep();

                //Redirect to Step5.
                return Redirect("/Account/Register/Step5");
            }
            //The email has not been sent successfully.
            else
            {
                //Keep TempData incase it is needed again.
                TempData.Keep();

                //Add an error to the ModelState to inform the user that the email was not sent.
                ModelState.AddModelError(string.Empty, "There was a problem sending the verification code");

                //Return the Page.
                return Page();
            }
        }

        //Method Summary:
        //This method is executed when the "Request verification code" button is clicked.
        //When executed this method attempts to send a verification code email to the user and returns the response from the _emailService.
        public async Task<Response> SendVerificationCode(string emailAddress, int verficationToken)
        {
            //Declare a new EmailAddress object and assign the users email address as the value.
            EmailAddress to = new EmailAddress(emailAddress);

            //Call the SendVerificationCodeEmail in the _emailService and return the response.
            return await _emailService.SendVerificationCodeEmail(to, verficationToken);
        }

        //Method Summary:
        //This method is executed when the back button is clicked.
        //When executed the user is redirected to Step3.
        public async Task<IActionResult> OnPostBack()
        {
            return Redirect("/Account/Register/Step3");
        }
        #endregion Verification Code
    }
}
