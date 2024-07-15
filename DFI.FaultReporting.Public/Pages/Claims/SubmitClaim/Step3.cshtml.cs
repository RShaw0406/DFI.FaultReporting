using DFI.FaultReporting.Interfaces.Admin;
using DFI.FaultReporting.Interfaces.Claims;
using DFI.FaultReporting.Interfaces.FaultReports;
using DFI.FaultReporting.Models.FaultReports;
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

namespace DFI.FaultReporting.Public.Pages.Claims.SubmitClaim
{
    public class Step3Model : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<Step3Model> _logger;
        private readonly IUserService _userService;
        private readonly IClaimService _claimService;
        private readonly IClaimTypeService _claimTypeService;
        private readonly IFaultService _faultService;
        private readonly IFaultTypeService _faultTypeService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISettingsService _settingsService;
        private readonly IEmailService _emailService;
        private readonly IVerificationTokenService _verificationTokenService;

        //Inject dependencies in constructor.
        public Step3Model(ILogger<Step3Model> logger, IUserService userService, IClaimService claimService, IClaimTypeService claimTypeService,
            IFaultService faultService, IFaultTypeService faultTypeService, IHttpContextAccessor httpContextAccessor, ISettingsService settingsService, IEmailService emailService,
            IVerificationTokenService verificationTokenService)
        {
            _logger = logger;
            _userService = userService;
            _faultService = faultService;
            _faultTypeService = faultTypeService;
            _httpContextAccessor = httpContextAccessor;
            _settingsService = settingsService;
            _emailService = emailService;
            _verificationTokenService = verificationTokenService;
        }
        #endregion Dependency Injection

        #region Properties
        public User CurrentUser { get; set; }

        [BindProperty]
        public Models.Claims.Claim Claim { get; set; }

        [BindProperty]
        public Step3InputModel Step3Input { get; set; }

        public class Step3InputModel
        {
            [DisplayName("Description of incident")]
            [Required(ErrorMessage = "You must enter a description of the incident")]
            public string? IncidentDescription { get; set; }

            [DisplayName("Description of incident location")]
            [Required(ErrorMessage = "You must enter a description of the incident location")]
            public string? IncidentLocationDescription { get; set; }

            [DisplayName("Day")]
            [Range(1, 31, ErrorMessage = "Incident date day must be between {1} and {2}")]
            public int? Day { get; set; } = null;

            [DisplayName("Month")]
            [Range(1, 12, ErrorMessage = "Incident date month must be between {1} and {2}")]
            public int? Month { get; set; } = null;

            [DisplayName("Year")]
            public int? Year { get; set; } = null;

            [DisplayName("Date of incident")]
            [DataType(DataType.Date)]
            public DateTime? IncidentDate { get; set; }
        }

        public bool ValidDate { get; set; }

        public bool InValidYear { get; set; }


        public string InValidYearMessage = "";
        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is executed when the page loads and required page properties are set.
        public async Task<IActionResult> OnGetAsync()
        {
            if (_httpContextAccessor.HttpContext.User != null)
            {
                //The contexts current user has been authenticated and has user role.
                if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true && _httpContextAccessor.HttpContext.User.IsInRole("User"))
                {
                    //Get the claim from "Claim" object stored in session.
                    Models.Claims.Claim? sessionClaim = HttpContext.Session.GetFromSession<Models.Claims.Claim>("Claim");

                    //User has previously input step2 and has clicked the back button on step3.
                    if (sessionClaim != null)
                    {
                        //Populate Step3Input model with session values.
                        Step3Input = new Step3InputModel();

                        Step3Input.IncidentLocationDescription = sessionClaim.IncidentLocationDescription;
                        Step3Input.IncidentDescription = sessionClaim.IncidentDescription;

                        if (sessionClaim.IncidentDate != null && sessionClaim.IncidentDate > DateTime.MinValue)
                        {
                            Step3Input.Day = sessionClaim.IncidentDate.Day;
                            Step3Input.Month = sessionClaim.IncidentDate.Month;
                            Step3Input.Year = sessionClaim.IncidentDate.Year;
                        }
                    }

                    ValidDate = true;


                    InValidYear = false;

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

        #region Page Buttons
        //Method Summary:
        //This method is executed when the "Next" button is clicked.
        //When executed a new Claim object is created and stored in session, the user is then redirected to Step3 page.
        public async Task<IActionResult> OnPostNext()
        {
            //Get the ID from the contexts current user, needed for populating CurrentUser property from DB.
            System.Security.Claims.Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
            string? jwtToken = jwtTokenClaim.Value;

            //Set the CurrentUser property by calling the GetUser method in the _userService.
            string? userID = _httpContextAccessor.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;
            CurrentUser = await _userService.GetUser(Convert.ToInt32(userID), jwtToken);

            //Initialise a new ValidationContext to be used to validate the Step2Input model only.
            ValidationContext validationContext = new ValidationContext(Step3Input);
            ICollection<ValidationResult> validationResults = new List<ValidationResult>();
            bool isStep3InputValid = Validator.TryValidateObject(Step3Input, validationContext, validationResults, true);

            //The entered year value is greater than the current year.
            if (Step3Input.Year > DateTime.Now.Year)
            {
                InValidYear = true;

                InValidYearMessage = "Incident date year cannot be in the future";

                validationResults.Add(new ValidationResult(InValidYearMessage));
            }

            //The entered year is far in the past.
            if (Step3Input.Year < 1900)
            {
                InValidYear = true;

                InValidYearMessage = "Incident date year must be at least after 1900";

                validationResults.Add(new ValidationResult(InValidYearMessage));
            }

            //Create a string date by combining all the input date values.
            string inputDate = Step3Input.Year.ToString() + "-" + Step3Input.Month.ToString() + "-" + Step3Input.Day.ToString();

            //Declare a newDate datetime, this is used as the return value of the attempted parse below.
            DateTime newDate;

            //Attempt to parse the string date to a valid datetime.
            ValidDate = DateTime.TryParse(inputDate, out newDate);

            //The parsed date is a valid datetime.
            if (ValidDate)
            {
                Step3Input.IncidentDate = newDate;
            }
            //The parsed date is not a valid datetime.
            else
            {
                validationResults.Add(new ValidationResult("Incident date must contain a valid day, month, and year"));

            }

            if (isStep3InputValid)
            {
                //The entered year is invalid.
                if (InValidYear)
                {
                    //Add an error to the ModelState to inform the user that they have entered a year that is in the future or too far in the past.
                    ModelState.AddModelError(string.Empty, InValidYearMessage);
                }

                //The entered date is not valid.
                if (!ValidDate)
                {
                    //Set the Step3Input model date to null to ensure fresh start.
                    Step3Input.IncidentDate = null;

                    //Add an error to the ModelState to inform the user that they have not entered a valid date.
                    ModelState.AddModelError(string.Empty, "Incident date must contain a valid day, month, and year");
                }

                //Either the date or the year are invalid.
                if (!ValidDate || InValidYear)
                {
                    //Return the Page();
                    return Page();
                }

                //Get the claim from "Claim" object stored in session.
                Models.Claims.Claim? sessionClaim = HttpContext.Session.GetFromSession<Models.Claims.Claim>("Claim");

                sessionClaim.IncidentDescription = Step3Input.IncidentDescription;
                sessionClaim.IncidentLocationDescription = Step3Input.IncidentLocationDescription;
                sessionClaim.IncidentDate = Convert.ToDateTime(Step3Input.IncidentDate);

                //Store the claim object in session.
                HttpContext.Session.SetInSession("Claim", sessionClaim);

                //Redirect to Step3 page.
                return Redirect("/Claims/SubmitClaim/Step4");
            }
            else
            {
                //Display each of the validation errors.
                foreach (ValidationResult validationResult in validationResults)
                {
                    ModelState.AddModelError(string.Empty, validationResult.ErrorMessage);
                    ModelState.AddModelError("Step1ClaimInput.ClaimTypeID", validationResult.ErrorMessage);
                }

                TempData.Keep();

                return Page();
            }

        }

        //Method Summary:
        //This method is executed when the back button is clicked.
        //When executed the user is redirected to Step2 page.
        public async Task<IActionResult> OnPostBack()
        {
            return Redirect("/Claims/SubmitClaim/Step2");
        }
        #endregion Page Buttons
    }
}
