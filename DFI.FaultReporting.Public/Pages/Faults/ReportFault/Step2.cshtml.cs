using DFI.FaultReporting.Common.SessionStorage;
using DFI.FaultReporting.Interfaces.Admin;
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
using static DFI.FaultReporting.Public.Pages.Faults.ReportFault.Step1Model;
using DFI.FaultReporting.Models.Admin;
using Microsoft.AspNetCore.Mvc.Rendering;
using DFI.FaultReporting.Services.Admin;
using System.Security.Claims;

namespace DFI.FaultReporting.Public.Pages.Faults.ReportFault
{
    public class Step2Model : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<Step2Model> _logger;
        private readonly IUserService _userService;
        private readonly IFaultService _faultService;
        private readonly IReportService _reportService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISettingsService _settingsService;
        private readonly IEmailService _emailService;
        private readonly IVerificationTokenService _verificationTokenService;

        //Inject dependencies in constructor.
        public Step2Model(ILogger<Step2Model> logger, IUserService userService, IReportService reportService, IFaultService faultService,
            IHttpContextAccessor httpContextAccessor, ISettingsService settingsService, IEmailService emailService,
            IVerificationTokenService verificationTokenService)
        {
            _logger = logger;
            _userService = userService;
            _faultService = faultService;
            _reportService = reportService;
            _httpContextAccessor = httpContextAccessor;
            _settingsService = settingsService;
            _emailService = emailService;
            _verificationTokenService = verificationTokenService;
        }
        #endregion Dependency Injection

        #region Properties
        //Declare CurrentUser property, this is needed when calling the _userService.
        public User CurrentUser { get; set; }

        //Declare Step2InputModel property, this is needed when reporting a fault on step 2.
        [BindProperty]
        public Step2InputModel Step2Input { get; set; }

        //Declare Step2InputModel class, this is needed when reporting a fault on step 2.
        public class Step2InputModel
        {
            [DisplayName("Additional information")]
            [Required(ErrorMessage = "You must enter additional info")]
            [StringLength(1000, ErrorMessage = "Additional info must not be more than 1000 characters")]
            public string? AdditionalInfo { get; set; }
        }
        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is executed when the page loads and is used for populating the fault types dropdown list.
        public async Task<IActionResult> OnGetAsync()
        {
            //The contexts current user exists.
            if (_httpContextAccessor.HttpContext.User != null)
            {
                //The contexts current user has been authenticated.
                if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true)
                {
                    
                    //Return the page.
                    return Page();
                }
                //The contexts current user has not been authenticated.
                else
                {
                    //Redirect user to no permission.
                    return Redirect("/NoPermission");
                }
            }
            //The contexts current user does not exist.
            else
            {
                //Redirect user to no permission
                return Redirect("/NoPermission");
            }
        }
        #endregion Page Load

        #region Step2
        //Method Summary:
        //This method is executed when the "Next" button is clicked.
        //When executed a new Report object is created and inserted to the database, the fault object stored in session is also inserted to the database, the user is then redirected to Step3 page.
        public async Task<IActionResult> OnPostNext()
        {
            //Get the ID from the contexts current user, needed for populating CurrentUser property from DB.
            string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            //Get the JWT token claim from the contexts current user, needed for populating CurrentUser property from DB.
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

            //Set the jwtToken string to the JWT token claims value, needed for populating CurrentUser property from DB.
            string? jwtToken = jwtTokenClaim.Value;

            //Set the CurrentUser property by calling the GetUser method in the _userService.
            CurrentUser = await _userService.GetUser(Convert.ToInt32(userID), jwtToken);

            //Initialise a new ValidationContext to be used to validate the Step2Input model only.
            ValidationContext validationContext = new ValidationContext(Step2Input);

            //Create a collection to store the returned Step2Input model validation results.
            ICollection<ValidationResult> validationResults = new List<ValidationResult>();

            //Carry out validation check on the Step2Input model.
            bool isStep2InputValid = Validator.TryValidateObject(Step2Input, validationContext, validationResults, true);

            //The isStep2InputValid model is valid.
            if (isStep2InputValid)
            {
                //Get the fault from "Fault" object stored in session.
                Fault? sessionFault = HttpContext.Session.GetFromSession<Fault>("Fault");

                //Insert session fault to DB.
                //Fault? insertedFault = await _faultService.CreateFault(sessionFault, jwtToken);

                //Create new Report
                Report newReport = new Report();
                newReport.FaultID = 0;
                newReport.AdditionalInfo = Step2Input.AdditionalInfo;
                newReport.UserID = CurrentUser.ID;
                newReport.InputBy = CurrentUser.Email;
                newReport.InputOn = DateTime.Now;
                newReport.Active = true;

                //Report? insertedReport = await _reportService.CreateReport(newReport, jwtToken);

                //Set the insertedReport in session, needed for displaying details if user returns to this step.
                HttpContext.Session.SetInSession("Report", newReport);

                return Redirect("/Faults/ReportFault/Step3");
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
        #endregion
    }
}
