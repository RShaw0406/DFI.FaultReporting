using DFI.FaultReporting.Interfaces.Admin;
using DFI.FaultReporting.Interfaces.FaultReports;
using DFI.FaultReporting.Public.Pages.Account;
using DFI.FaultReporting.Services.Interfaces.Emails;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Tokens;
using DFI.FaultReporting.Services.Interfaces.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Models.FaultReports;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Common.SessionStorage;
using System.Security.Claims;
using System.Diagnostics;

namespace DFI.FaultReporting.Public.Pages.Faults.ReportFault
{
    public class Step1Model : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<Step1Model> _logger;
        private readonly IUserService _userService;
        private readonly IFaultService _faultService;
        private readonly IFaultTypeService _faultTypeService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISettingsService _settingsService;
        private readonly IEmailService _emailService;
        private readonly IVerificationTokenService _verificationTokenService;

        //Inject dependencies in constructor.
        public Step1Model(ILogger<Step1Model> logger, IUserService userService, IFaultService faultService, IFaultTypeService faultTypeService, 
            IHttpContextAccessor httpContextAccessor, ISettingsService settingsService, IEmailService emailService,
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
        //Declare CurrentUser property, this is needed when calling the _userService.
        public User CurrentUser { get; set; }

        //Declare Step1InputModel property, this is needed when reporting a fault on step 1.
        [BindProperty]
        public Step1InputModel Step1Input { get; set; }

        //Declare FaultTypes property, this is needed for populating fault types dropdown list.
        public List<FaultType> FaultTypes { get; set; }

        //Declare FaultTypesList property, this is needed for populating fault types dropdown list.
        public IEnumerable<SelectListItem> FaultTypesList { get; set; }

        //Declare Step1InputModel class, this is needed when reporting a fault on step 1.
        public class Step1InputModel
        {
            [DisplayName("Fault type")]
            [Required(ErrorMessage = "You must enter a fault type")]
            public int FaultTypeID { get; set; }

            [DisplayName("Lat")]
            [Required(ErrorMessage = "You must select a location on the map")]
            public string? Latitude { get; set; }

            [DisplayName("Long")]
            public string? Longitude { get; set; }

            [DisplayName("Road number")]
            [StringLength(10, ErrorMessage = "Road number must not be more than 10 characters")]
            public string? RoadNumber { get; set; }

            [DisplayName("Road")]
            [Required(ErrorMessage = "You must enter a road")]
            [StringLength(250, ErrorMessage = "Road must not be more than 250 characters")]
            public string? RoadName { get; set; }

            [DisplayName("Road town")]
            [StringLength(100, ErrorMessage = "Road town must not be more than 100 characters")]
            public string? RoadTown { get; set; }

            [DisplayName("Road county")]
            [StringLength(50, ErrorMessage = "Road county must not be more than 50 characters")]
            public string? RoadCounty { get; set; }
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
                    //Get the fault from "Fault" object stored in session.
                    Fault? sessionFault = HttpContext.Session.GetFromSession<Fault>("Fault");

                    //User has previously input step1 details and has clicked the back button on step2.
                    if (sessionFault != null)
                    {
                        //Populate Step1Input model with session values.
                        Step1Input = new Step1InputModel();
                        Step1Input.Latitude = sessionFault.Latitude;
                        Step1Input.Longitude = sessionFault.Longitude;
                        Step1Input.RoadNumber = sessionFault.RoadNumber;
                        Step1Input.RoadName = sessionFault.RoadName;
                        Step1Input.RoadTown = sessionFault.RoadTown;
                        Step1Input.RoadCounty = sessionFault.RoadCounty;
                        Step1Input.FaultTypeID = sessionFault.FaultTypeID;
                    }

                    //Get fault types for dropdown
                    FaultTypes = await GetFaultTypes();
                    //Populate fault types dropdown.
                    FaultTypesList = FaultTypes.Select(ft => new SelectListItem()
                    {
                        Text = ft.FaultTypeDescription,
                        Value = ft.ID.ToString()
                    });

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

        #region Step1
        //Method Summary:
        //This method is executed when the "Next" button is clicked.
        //When executed a new Fault object is created and stored in session, the user is then redirected to Step2 page.
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

            //Initialise a new ValidationContext to be used to validate the Step1Input model only.
            ValidationContext validationContext = new ValidationContext(Step1Input);

            //Create a collection to store the returned Step1Input model validation results.
            ICollection<ValidationResult> validationResults = new List<ValidationResult>();

            //Carry out validation check on the Step1Input model.
            bool isStep1InputValid = Validator.TryValidateObject(Step1Input, validationContext, validationResults, true);

            //The isStep1InputValid model is valid.
            if (isStep1InputValid)
            {
                //Create new fault
                Fault newFault = new Fault();
                newFault.Latitude = Step1Input.Latitude;
                newFault.Longitude = Step1Input.Longitude;
                newFault.RoadNumber = Step1Input.RoadNumber;
                newFault.RoadName = Step1Input.RoadName;
                newFault.RoadTown = Step1Input.RoadTown;
                newFault.RoadCounty = Step1Input.RoadCounty;
                newFault.FaultTypeID = Step1Input.FaultTypeID;
                if (Step1Input.RoadNumber != null) {
                    if (Step1Input.RoadNumber.Contains("M"))
                    {
                        newFault.FaultPriorityID = 4;
                    }
                    else if (Step1Input.RoadNumber.Contains("A"))
                    {
                        newFault.FaultPriorityID = 7;
                    }
                    else if (Step1Input.RoadNumber.Contains("B"))
                    {
                        newFault.FaultPriorityID = 8;
                    }
                    else
                    {
                        newFault.FaultPriorityID = 9;
                    }
                }
                newFault.FaultStatusID = 1;
                newFault.InputBy = CurrentUser.Email;
                newFault.InputOn = DateTime.Now;
                newFault.Active = true;

                //Set the newFault in session, needed for displaying details if user returns to this step.
                HttpContext.Session.SetInSession("Fault", newFault);

                //Redirect user to step2
                return Redirect("/Faults/ReportFault/Step2");

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

                //Get fault types for dropdown.
                FaultTypes = await GetFaultTypes();

                //Populate fault types dropdown.
                FaultTypesList = FaultTypes.Select(ft => new SelectListItem()
                {
                    Text = ft.FaultTypeDescription,
                    Value = ft.ID.ToString()
                });

                //Return the Page.
                return Page();
            }
        }

        //Method Summary:
        //This method is executed when the back button is clicked.
        //When executed the user is redirected to ReportFault page.
        public async Task<IActionResult> OnPostBack()
        {
            return Redirect("/Faults/ReportFault/ReportFault");
        }
        #endregion Step1

        #region Fault Types
        //Method Summary:
        //This method is executed when the page loads and when an error occurs.
        //When executed this method returns a list of fault types to populate the fault types dropdown list.
        public async Task<List<FaultType>> GetFaultTypes()
        {
            //Get fault types by calling the GetFaultTypes method from the _faultTypeService.
            List<FaultType> faultTypes = await _faultTypeService.GetFaultTypes();

            //Return fault types.
            return faultTypes;
        }
        #endregion Fault Types
    }
}
