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
using DFI.FaultReporting.Common.SessionStorage;
using DFI.FaultReporting.Models.Files;
using System.Security.Claims;

namespace DFI.FaultReporting.Public.Pages.Account.Register
{
    public class Step3Model : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<Step3Model> _logger;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISettingsService _settingsService;
        private readonly IEmailService _emailService;
        private readonly IVerificationTokenService _verificationTokenService;
        private readonly IContractorService _contractorService;

        //Inject dependencies in constructor.
        public Step3Model(ILogger<Step3Model> logger, IUserService userService, IHttpContextAccessor httpContextAccessor, ISettingsService settingsService, IEmailService emailService,
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

        //Declare Step3InputModel property, this is needed when registering on step 3.
        [BindProperty]
        public Step3InputModel Step3Input { get; set; }

        //Declare isContractorEmail property, this is needed for storing whether the user is a contractor or not, so the date of birth fields can be hidden.
        [BindProperty]
        public bool isContractorEmail { get; set; }

        //Declare Step3InputModel class, this is needed when registering on step 3.
        public class Step3InputModel
        {
            [Required]
            [DisplayName("Postcode")]
            [RegularExpression(@"^(([Bb][Tt][0-9]{1,2})\s?[0-9][A-Za-z]{2})$", ErrorMessage = "You must enter a valid Northern Ireland postcode")]
            public string? Postcode { get; set; }

            [Required]
            [DisplayName("Address line 1")]
            [RegularExpression(@"^[a-zA-Z0-9''-'\s]{1,100}$", ErrorMessage = "Address line 1 must not contain special characters")]
            [StringLength(100, ErrorMessage = "Address line 1 must not be more than 100 characters")]
            public string? AddressLine1 { get; set; }

            [DisplayName("Address line 2")]
            [RegularExpression(@"^[a-zA-Z0-9''-'\s]{1,100}$", ErrorMessage = "Address line 2 must not contain special characters")]
            [StringLength(100, ErrorMessage = "Address line 2 must not be more than 100 characters")]
            public string? AddressLine2 { get; set; }

            [DisplayName("Address line 3")]
            [RegularExpression(@"^[a-zA-Z0-9''-'\s]{1,100}$", ErrorMessage = "Address line 3 must not contain special characters")]
            [StringLength(100, ErrorMessage = "Address line 3 must not be more than 100 characters")]
            public string? AddressLine3 { get; set; }
        }
        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is executed when the page loads.
        //When executed when the page loads and the session values are retrieved.
        public async Task<IActionResult> OnGetAsync()
        {
            //The HttpContext user is already authenticated.
            if (HttpContext.User.Identity.IsAuthenticated == true)
            {
                //Redirect to the index page.

                Redirect("./Index");
            }

            isContractorEmail = HttpContext.Session.GetFromSession<bool>("ContractorEmail");

            //Get the registration request from "RegistrationRequest" object stored in session.
            RegistrationRequest? sessionRegistrationRequest = HttpContext.Session.GetFromSession<RegistrationRequest>("RegistrationRequest");

            //User has previously input step3 details and has clicked the back button on step4.
            if (sessionRegistrationRequest != null)
            {
                //Populate Step3Input model with session values.
                Step3Input = new Step3InputModel();
                Step3Input.Postcode = sessionRegistrationRequest.Postcode;
                Step3Input.AddressLine1 = sessionRegistrationRequest.AddressLine1;
                Step3Input.AddressLine2 = sessionRegistrationRequest.AddressLine2;
                Step3Input.AddressLine3 = sessionRegistrationRequest.AddressLine3;
            }

            //Return the page.
            return Page();
        }
        #endregion Page Load

        #region Step3
        //Method Summary:
        //This method is executed when the next button is clicked.
        //When executed the user is redirected to Step4.
        public async Task<IActionResult> OnPostNext()
        {
            //Initialise a new ValidationContext to be used to validate the Step3Input model only.
            ValidationContext validationContext = new ValidationContext(Step3Input);

            //Create a collection to store the returned Step3Input model validation results.
            ICollection<ValidationResult> validationResults = new List<ValidationResult>();

            //Carry out validation check on the Step3Input model.
            bool isStep3InputValid = Validator.TryValidateObject(Step3Input, validationContext, validationResults, true);

            //Get the isContractorEmail from the session.
            isContractorEmail = HttpContext.Session.GetFromSession<bool>("ContractorEmail");

            //The isStep3InputValid model is valid.
            if (isStep3InputValid)
            {
                //Get the registration request from "RegistrationRequest" object stored in session.
                RegistrationRequest? sessionRegistrationRequest = HttpContext.Session.GetFromSession<RegistrationRequest>("RegistrationRequest");

                //Add the address details to the session registration request.
                sessionRegistrationRequest.Postcode = Step3Input.Postcode;
                sessionRegistrationRequest.AddressLine1 = Step3Input.AddressLine1;
                sessionRegistrationRequest.AddressLine2 = Step3Input.AddressLine2;
                sessionRegistrationRequest.AddressLine3 = Step3Input.AddressLine3;

                //Set the RegistrationRequest in session, needed for displaying details if user returns to this step.
                HttpContext.Session.SetInSession("RegistrationRequest", sessionRegistrationRequest);

                //Redirect to Step4.
                return Redirect("/Account/Register/Step4");
            }
            //The isStep3InputValid model is not valid.
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

        //Method Summary:
        //This method is executed when the back button is clicked.
        //When executed the user is redirected to Step2.
        public async Task<IActionResult> OnPostBack()
        {
            return Redirect("/Account/Register/Step2");
        }
        #endregion Step3
    }
}