using DFI.FaultReporting.Interfaces.Admin;
using DFI.FaultReporting.Interfaces.Claims;
using DFI.FaultReporting.Interfaces.FaultReports;
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
    public class Step4Model : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<Step4Model> _logger;
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
        public Step4Model(ILogger<Step4Model> logger, IUserService userService, IClaimService claimService, IClaimTypeService claimTypeService,
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
        public Step4InputModel Step4Input { get; set; }

        public class Step4InputModel
        {
            [DisplayName("Description of injury")]
            public string? InjuryDescription { get; set; }

            [DisplayName("Description of damage")]
            public string? DamageDescription { get; set; }

            [DisplayName("Description of claim")]
            public string? DamageClaimDescription { get; set; }
        }

        public bool isInjuryClaim { get; set; }

        public bool isDamageClaim { get; set; }
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
                        //Populate Step4Input model with session values.
                        Step4Input = new Step4InputModel();
                        Step4Input.InjuryDescription = sessionClaim.InjuryDescription;
                        Step4Input.DamageDescription = sessionClaim.DamageDescription;
                        Step4Input.DamageClaimDescription = sessionClaim.DamageClaimDescription;
                    }

                    if (sessionClaim.ClaimTypeID == 8)
                    {
                        isInjuryClaim = true;
                    }
                    else
                    {
                        isDamageClaim = true;
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
        #endregion

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

            //Initialise a new ValidationContext to be used to validate the Step4Input model only.
            ValidationContext validationContext = new ValidationContext(Step4Input);
            ICollection<ValidationResult> validationResults = new List<ValidationResult>();
            bool isStep4InputValid = Validator.TryValidateObject(Step4Input, validationContext, validationResults, true);
         
            if (isStep4InputValid)
            {              
                //Get the claim from "Claim" object stored in session.
                Models.Claims.Claim? sessionClaim = HttpContext.Session.GetFromSession<Models.Claims.Claim>("Claim");

                sessionClaim.InjuryDescription = Step4Input.InjuryDescription;
                sessionClaim.DamageDescription = Step4Input.DamageDescription;
                sessionClaim.DamageClaimDescription = Step4Input.DamageClaimDescription;

                //Store the claim object in session.
                HttpContext.Session.SetInSession("Claim", sessionClaim);

                //Redirect to Step3 page.
                return Redirect("/Claims/SubmitClaim/Step5");
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
        //When executed the user is redirected to Step3 page.
        public async Task<IActionResult> OnPostBack()
        {
            return Redirect("/Claims/SubmitClaim/Step3");
        }
        #endregion Page Buttons
    }
}
