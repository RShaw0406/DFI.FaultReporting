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
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Models.FaultReports;
using System.Security.Claims;

namespace DFI.FaultReporting.Public.Pages.Account.Register
{
    public class Step2Model : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<Step2Model> _logger;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISettingsService _settingsService;
        private readonly IEmailService _emailService;
        private readonly IVerificationTokenService _verificationTokenService;
        private readonly IContractorService _contractorService;

        //Inject dependencies in constructor.
        public Step2Model(ILogger<Step2Model> logger, IUserService userService, IHttpContextAccessor httpContextAccessor, ISettingsService settingsService, IEmailService emailService,
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

        //Declare Step2InputModel property, this is needed when registering on step 2.
        [BindProperty]
        public Step2InputModel Step2Input { get; set; }

        //Declare isContractorEmail property, this is needed for storing whether the user is a contractor or not, so the date of birth fields can be hidden.
        [BindProperty]
        public bool isContractorEmail { get; set; }

        //Declare ValidDOB property, this is needed for validating the input DOB when inputting personal details.
        public bool ValidDOB { get; set; }

        //Declare InValidYearDOB property, this is needed for validating the input year when inputting personal details.
        public bool InValidYearDOB { get; set; }

        //Declare InValidYearDOBMessage property, this is needed for storing the specific error message when inputting personal details year.
        public string InValidYearDOBMessage = "";

        //Declare Step2InputModel class, this is needed when registering on step 2.
        public class Step2InputModel
        {
            [Required]
            [DisplayName("Title")]
            [RegularExpression(@"^[a-zA-Z''-'\s]{1,8}$", ErrorMessage = "Title must not contain special characters or numbers")]
            [StringLength(8, ErrorMessage = "Title must not be more than 8 characters")]
            public string? Prefix { get; set; }

            [Required]
            [DisplayName("First name")]
            [RegularExpression(@"^[a-zA-Z''-'\s]{1,125}$", ErrorMessage = "First name must not contain special characters or numbers")]
            [StringLength(125, ErrorMessage = "First name must not be more than 125 characters")]
            public string? FirstName { get; set; }

            [Required]
            [DisplayName("Last name")]
            [RegularExpression(@"^[a-zA-Z''-'\s]{1,125}$", ErrorMessage = "Last name must not contain special characters or numbers")]
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
        }
        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is executed when the page loads and the session values are retrieved.
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

            //User has previously input step2 details and has clicked the back button on step3.
            if (sessionRegistrationRequest != null)
            {
                //Populate Step2Input model with session values.
                Step2Input = new Step2InputModel();
                Step2Input.Prefix = sessionRegistrationRequest.Prefix;
                Step2Input.FirstName = sessionRegistrationRequest.FirstName;
                Step2Input.LastName = sessionRegistrationRequest.LastName;
                Step2Input.DayDOB = sessionRegistrationRequest.DayDOB;
                Step2Input.MonthDOB = sessionRegistrationRequest.MonthDOB;
                Step2Input.YearDOB = sessionRegistrationRequest.YearDOB;
            }

            //Set the ValidDOB property to true, this is needed for validation.
            ValidDOB = true;

            //Set the InValidYearDOB property to false, this is needed for validation.
            InValidYearDOB = false;

            //Return the page.
            return Page();
        }
        #endregion Page Load

        #region Step2
        //Method Summary:
        //This method is executed when the "Next" button is clicked.
        //When executed the personal details are added to the session registration request, the user is then redirected to Step3 page.
        public async Task<IActionResult> OnPostNext()
        {
            //Initialise a new ValidationContext to be used to validate the Step2Input model only.
            ValidationContext validationContext = new ValidationContext(Step2Input);

            //Create a collection to store the returned Step2Input model validation results.
            ICollection<ValidationResult> validationResults = new List<ValidationResult>();

            //Carry out validation check on the Step2Input model.
            bool isStep2InputValid = Validator.TryValidateObject(Step2Input, validationContext, validationResults, true);

            //Get the isContractorEmail from the session.
            isContractorEmail = HttpContext.Session.GetFromSession<bool>("ContractorEmail");

            //The email is not a contractor email so user had to enter DOB.
            if (!isContractorEmail)
            {
                //The entered year value is greater than the current year.
                if (Step2Input.YearDOB > DateTime.Now.Year)
                {
                    //Set the InValidYearDOB to true.
                    InValidYearDOB = true;

                    //Set the InValidYearDOBMessage to an error message informing the user that the entered year cannot be in the future.
                    InValidYearDOBMessage = "Date of birth year cannot be in the future";

                    //Add the InValidYearDOBMessage to the validationResults.
                    validationResults.Add(new ValidationResult(InValidYearDOBMessage));
                }

                //The entered year is far in the past.
                if (Step2Input.YearDOB < 1900)
                {
                    //Set the InValidYearDOB to true.
                    InValidYearDOB = true;

                    //Set the InValidYearDOBMessage to an error message informing the user that the entered year cannot be before 1900.
                    InValidYearDOBMessage = "Date of birth year must be at least after 1900";

                    //Add the InValidYearDOBMessage to the validationResults.
                    validationResults.Add(new ValidationResult(InValidYearDOBMessage));
                }

                //Create a string DOB by combining all the input DOB values.
                string inputDOB = Step2Input.YearDOB.ToString() + "-" + Step2Input.MonthDOB.ToString() + "-" + Step2Input.DayDOB.ToString();

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
            }

            //The isStep2InputValid model is valid.
            if (isStep2InputValid)
            {
                //The email is not a contractor email so user had to enter DOB.
                if (!isContractorEmail)
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
                }

                //Get the registration request from "RegistrationRequest" object stored in session.
                RegistrationRequest? sessionRegistrationRequest = HttpContext.Session.GetFromSession<RegistrationRequest>("RegistrationRequest");

                //Add the personal details to the session registration request.
                sessionRegistrationRequest.Prefix = Step2Input.Prefix;
                sessionRegistrationRequest.FirstName = Step2Input.FirstName;
                sessionRegistrationRequest.LastName = Step2Input.LastName;

                //The email is not a contractor email so user had to enter DOB.
                if (!isContractorEmail)
                {
                    sessionRegistrationRequest.DayDOB = Step2Input.DayDOB;
                    sessionRegistrationRequest.MonthDOB = Step2Input.MonthDOB;
                    sessionRegistrationRequest.YearDOB = Step2Input.YearDOB;
                }
                //The email is a contractor email so the DOB is not needed.
                else
                {
                    sessionRegistrationRequest.DayDOB = null;
                    sessionRegistrationRequest.MonthDOB = null;
                    sessionRegistrationRequest.YearDOB = null;
                }

                //Set the RegistrationRequest in session, needed for displaying details if user returns to this step.
                HttpContext.Session.SetInSession("RegistrationRequest", sessionRegistrationRequest);

                //Redirect to Step3.
                return Redirect("/Account/Register/Step3");
            }
            //The isStep2InputValid model is not valid.
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
        //When executed the user is redirected to Step1.
        public async Task<IActionResult> OnPostBack()
        {
            return Redirect("/Account/Register/Step1");
        }
        #endregion
    }
}
