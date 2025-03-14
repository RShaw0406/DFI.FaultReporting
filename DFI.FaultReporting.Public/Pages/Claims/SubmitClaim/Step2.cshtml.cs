using DFI.FaultReporting.Interfaces.Admin;
using DFI.FaultReporting.Interfaces.Claims;
using DFI.FaultReporting.Interfaces.FaultReports;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Emails;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Tokens;
using DFI.FaultReporting.Services.Interfaces.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using DFI.FaultReporting.Models.FaultReports;
using System.Security.Claims;
using DFI.FaultReporting.Common.SessionStorage;
using static DFI.FaultReporting.Public.Pages.Claims.SubmitClaim.Step1Model;
using DFI.FaultReporting.Models.Claims;

namespace DFI.FaultReporting.Public.Pages.Claims.SubmitClaim
{
    public class Step2Model : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<Step2Model> _logger;
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
        public Step2Model(ILogger<Step2Model> logger, IUserService userService, IClaimService claimService, IClaimTypeService claimTypeService, 
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
        public Fault Fault { get; set; }

        [BindProperty]
        public List<Fault> Faults { get; set; }

        [BindProperty]
        public Step2InputModel Step2Input { get; set; }

        public class Step2InputModel
        {
            [DisplayName("Fault")]
            public int? FaultID { get; set; } = null;

            [DisplayName("Lat")]
            [Required(ErrorMessage = "You must select a location or a fault")]
            public string? IncidentLocationLatitude { get; set; }

            [DisplayName("Long")]
            public string? IncidentLocationLongitude { get; set; }
        }

        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is executed when the page loads and required page properties are set.
        public async Task<IActionResult> OnGetAsync()
        {
            //The contexts current user exists.
            if (_httpContextAccessor.HttpContext.User != null)
            {
                //The contexts current user has been authenticated and has user role.
                if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true && _httpContextAccessor.HttpContext.User.IsInRole("User"))
                {
                    await PopulateProperties();

                    //Store the Faults object in session, needed for selectin faults on map.
                    HttpContext.Session.SetInSession("Faults", Faults);

                    //Get the claim from "Claim" object stored in session.
                    Models.Claims.Claim? sessionClaim = HttpContext.Session.GetFromSession<Models.Claims.Claim>("Claim");

                    //User has previously input step2 and has clicked the back button on step3.
                    if (sessionClaim != null)
                    {
                        //Populate Step2Input model with session values.
                        Step2Input = new Step2InputModel();

                        if (sessionClaim.FaultID != null)
                        {
                            Step2Input.FaultID = sessionClaim.FaultID;
                        }
                        Step2Input.IncidentLocationLatitude = sessionClaim.IncidentLocationLatitude;
                        Step2Input.IncidentLocationLongitude = sessionClaim.IncidentLocationLongitude;
                    }

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
            ValidationContext validationContext = new ValidationContext(Step2Input);
            ICollection<ValidationResult> validationResults = new List<ValidationResult>();
            bool isStep2InputValid = Validator.TryValidateObject(Step2Input, validationContext, validationResults, true);

            if (isStep2InputValid)
            {
                //Get the claim from "Claim" object stored in session.
                Models.Claims.Claim? sessionClaim = HttpContext.Session.GetFromSession<Models.Claims.Claim>("Claim");

                if (Step2Input.FaultID != null)
                {
                    sessionClaim.FaultID = Step2Input.FaultID;

                    HttpContext.Session.SetInSession("HasFault", true);
                }
                else
                {
                    HttpContext.Session.SetInSession("HasFault", false);
                }

                sessionClaim.IncidentLocationLatitude = Step2Input.IncidentLocationLatitude;
                sessionClaim.IncidentLocationLongitude = Step2Input.IncidentLocationLongitude;

                //Store the claim object in session.
                HttpContext.Session.SetInSession("Claim", sessionClaim);

                //Redirect to Step3 page.
                return Redirect("/Claims/SubmitClaim/Step3");
            }
            else
            {
                //Display each of the validation errors.
                foreach (ValidationResult validationResult in validationResults)
                {
                    ModelState.AddModelError(string.Empty, validationResult.ErrorMessage);
                    ModelState.AddModelError("Step1ClaimInput.ClaimTypeID", validationResult.ErrorMessage);
                }

                await PopulateProperties();

                TempData.Keep();

                return Page();
            }
        }


        //Method Summary:
        //This method is executed when the back button is clicked.
        //When executed the user is redirected to Step1 page.
        public async Task<IActionResult> OnPostBack()
        {
            return Redirect("/Claims/SubmitClaim/Step1");
        }
        #endregion Page Buttons

        #region Data
        //Method Summary:
        //This method is excuted when the a post occurs.
        //When excuted, it populates the page properties.
        public async Task PopulateProperties()
        {
            Faults = await _faultService.GetFaults();
        }
        #endregion Data
    }
}
