using DFI.FaultReporting.JWT.Requests;
using DFI.FaultReporting.Services.Interfaces.Emails;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Tokens;
using DFI.FaultReporting.Services.Interfaces.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using DFI.FaultReporting.Common.SessionStorage;
using DFI.FaultReporting.Models.FaultReports;
using DFI.FaultReporting.Models.Admin;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using DFI.FaultReporting.Services.Interfaces.Admin;
using DocumentFormat.OpenXml.Spreadsheet;

namespace DFI.FaultReporting.Public.Pages.Account.Register
{
    public class Step1Model : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<Step1Model> _logger;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISettingsService _settingsService;
        private readonly IEmailService _emailService;
        private readonly IVerificationTokenService _verificationTokenService;
        private readonly IContractorService _contractorService;

        //Inject dependencies in constructor.
        public Step1Model(ILogger<Step1Model> logger, IUserService userService, IHttpContextAccessor httpContextAccessor, ISettingsService settingsService, IEmailService emailService,
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

        //Declare Step1InputModel property, this is needed when registering on step 1.
        [BindProperty]
        public Step1InputModel Step1Input { get; set; }

        //Declare Step1InputModel class, this is needed when registering on step 1.
        public class Step1InputModel
        {
            [Required]
            [DataType(DataType.EmailAddress, ErrorMessage = "You must enter a valid email address")]
            public string? Email { get; set; }

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

            [Required]
            [DisplayName("Contact number")]
            [RegularExpression(@"^(?:(?:\(?(?:0(?:0|11)\)?[\s-]?\(?|\+)44\)?[\s-]?(?:\(?0\)?[\s-]?)?)|(?:\(?0))(?:(?:\d{5}\)?[\s-]?\d{4,5})|(?:\d{4}\)?[\s-]?(?:\d{5}|\d{3}[\s-]?\d{3}))|(?:\d{3}\)?[\s-]?\d{3}[\s-]?\d{3,4})|(?:\d{2}\)?[\s-]?\d{4}[\s-]?\d{4}))(?:[\s-]?(?:x|ext\.?|\#)\d{3,4})?$", ErrorMessage = "You must enter a valid contact number")]
            [DataType(DataType.PhoneNumber, ErrorMessage = "You must enter a valid contact number")]
            public string? ContactNumber { get; set; }
        }
        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is executed when the page loads and is used for clearing TempData to ensure fresh start.
        public async Task<IActionResult> OnGetAsync()
        {

            //The HttpContext user is already authenticated.
            if (HttpContext.User.Identity.IsAuthenticated == true)
            {
                //Redirect to the index page.
               
                Redirect("./Index");
            }

            //Get the registration request from "RegistrationRequest" object stored in session.
            RegistrationRequest? sessionRegistrationRequest = HttpContext.Session.GetFromSession<RegistrationRequest>("RegistrationRequest");

            //User has previously input step1 details and has clicked the back button on step2.
            if (sessionRegistrationRequest != null)
            {
                //Populate Step1Input model with session values.
                Step1Input = new Step1InputModel();
                Step1Input.Email = sessionRegistrationRequest.Email;
                Step1Input.Password = sessionRegistrationRequest.Password;
                Step1Input.ConfirmPassword = sessionRegistrationRequest.ConfirmPassword;
                Step1Input.ContactNumber = sessionRegistrationRequest.ContactNumber;
            }

            //Return the page.
            return Page();
        }
        #endregion Page Load

        #region Step1
        //Method Summary:
        //This method is executed when the "Next" button is clicked.
        //When executed a new RegistrationRequest object is created and stored in session, the user is then redirected to Step2 page.
        public async Task<IActionResult> OnPostNext()
        {
            //Initialise a new ValidationContext to be used to validate the Step1Input model only.
            ValidationContext validationContext = new ValidationContext(Step1Input);

            //Create a collection to store the returned Step1Input model validation results.
            ICollection<ValidationResult> validationResults = new List<ValidationResult>();

            //Carry out validation check on the Step1Input model.
            bool isStep1InputValid = Validator.TryValidateObject(Step1Input, validationContext, validationResults, true);

            //The isStep1InputValid model is valid.
            if (isStep1InputValid)
            {
                //Check if the input email already exists in the database.
                bool emailExists = await _userService.CheckEmail(Step1Input.Email);

                //Email already exists in the database.
                if (emailExists)
                {
                    //Add an error to the ModelState to inform the user that the email already exists.
                    ModelState.AddModelError(string.Empty, "Email already exists");

                    //Return the Page.
                    return Page();
                }

                //Create new RegistrationRequest
                RegistrationRequest = new RegistrationRequest();
                RegistrationRequest.Email = Step1Input.Email;
                RegistrationRequest.Password = Step1Input.Password;
                RegistrationRequest.ConfirmPassword = Step1Input.ConfirmPassword;
                RegistrationRequest.ContactNumber = Step1Input.ContactNumber;

                //Get the registration request from "RegistrationRequest" object stored in session.
                RegistrationRequest? sessionRegistrationRequest = HttpContext.Session.GetFromSession<RegistrationRequest>("RegistrationRequest");

                if (sessionRegistrationRequest != null)
                {
                    RegistrationRequest.Prefix = sessionRegistrationRequest.Prefix;
                    RegistrationRequest.FirstName = sessionRegistrationRequest.FirstName;
                    RegistrationRequest.LastName = sessionRegistrationRequest.LastName;
                    RegistrationRequest.DayDOB = sessionRegistrationRequest.DayDOB;
                    RegistrationRequest.MonthDOB = sessionRegistrationRequest.MonthDOB;
                    RegistrationRequest.YearDOB = sessionRegistrationRequest.YearDOB;
                    RegistrationRequest.Postcode = sessionRegistrationRequest.Postcode;
                    RegistrationRequest.AddressLine1 = sessionRegistrationRequest.AddressLine1;
                    RegistrationRequest.AddressLine2 = sessionRegistrationRequest.AddressLine2;
                    RegistrationRequest.AddressLine3 = sessionRegistrationRequest.AddressLine3;
                }

                //Set the RegistrationRequest in session, needed for displaying details if user returns to this step.
                HttpContext.Session.SetInSession("RegistrationRequest", RegistrationRequest);

                //Check if the input email has been added to an approved contractor in the internal administation system.
                bool isContractorEmail = await _contractorService.CheckForContractor(Step1Input.Email);

                //Email is added to an approved contractor in the internal administation system.
                if (isContractorEmail)
                {
                    HttpContext.Session.SetInSession("ContractorEmail", true);
                }
                //Email is not added to an approved contractor in the internal administation system.
                else
                {
                    HttpContext.Session.SetInSession("ContractorEmail", false);
                }

                //Redirect user to step2
                return Redirect("/Account/Register/Step2");

            }
            //The isStep1InputValid model is not valid.
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
        //When executed the user is redirected to Register page.
        public async Task<IActionResult> OnPostBack()
        {
            return Redirect("/Account/Register/Register");
        }
        #endregion Step1
    }
}
