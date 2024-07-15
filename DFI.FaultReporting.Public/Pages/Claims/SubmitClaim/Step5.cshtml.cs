using DFI.FaultReporting.Interfaces.Admin;
using DFI.FaultReporting.Interfaces.Claims;
using DFI.FaultReporting.Interfaces.FaultReports;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Models.Claims;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Claims;
using DFI.FaultReporting.Services.Interfaces.Emails;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Tokens;
using DFI.FaultReporting.Services.Interfaces.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DFI.FaultReporting.Common.SessionStorage;
using static DFI.FaultReporting.Public.Pages.Claims.SubmitClaim.Step1Model;

namespace DFI.FaultReporting.Public.Pages.Claims.SubmitClaim
{
    public class Step5Model : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<Step5Model> _logger;
        private readonly IUserService _userService;
        private readonly IWitnessService _witnessService;
        private readonly IClaimService _claimService;
        private readonly IClaimTypeService _claimTypeService;
        private readonly IFaultService _faultService;
        private readonly IFaultTypeService _faultTypeService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISettingsService _settingsService;
        private readonly IEmailService _emailService;
        private readonly IVerificationTokenService _verificationTokenService;

        //Inject dependencies in constructor.
        public Step5Model(ILogger<Step5Model> logger, IUserService userService, IWitnessService witnessService, IClaimService claimService, IClaimTypeService claimTypeService,
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
        public Witness? Witness { get; set; }

        [BindProperty]
        public Step5WitnessQuestionInputModel Step5WitnessQuestionInput { get; set; }

        public class Step5WitnessQuestionInputModel
        {
            [DisplayName("Was there a witness to the incident?")]
            [Required(ErrorMessage = "You must select an option")]
            public int YesNo { get; set; } = 1;
        }

        [BindProperty]
        public bool ShowWitnessInput { get; set; } = false;

        [BindProperty]
        public bool SaveSuccess { get; set; } = false;

        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is executed when the page loads and is used for getting session values.
        public async Task<IActionResult> OnGetAsync()
        {
            //The contexts current user exists.
            if (_httpContextAccessor.HttpContext.User != null)
            {
                //The contexts current user has been authenticated and has user role.
                if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true && _httpContextAccessor.HttpContext.User.IsInRole("User"))
                {
                    //Get the witness question from "Step5WitnessQuestionInput" object stored in session.
                    Step5WitnessQuestionInputModel? sessionYesNo = HttpContext.Session.GetFromSession<Step5WitnessQuestionInputModel>("Step5WitnessQuestionInput");

                    //User has previously input witness question and has clicked the back button on step6.
                    if (sessionYesNo != null)
                    {
                        //Populate Step5WitnessQuestionInput model with session values.
                        Step5WitnessQuestionInput = new Step5WitnessQuestionInputModel();
                        Step5WitnessQuestionInput.YesNo = sessionYesNo.YesNo;

                        //Hide or show the witness input based on the yes no value.
                        if (Step5WitnessQuestionInput.YesNo == 2)
                        {
                            ShowWitnessInput = true;
                        }
                        else
                        {
                            ShowWitnessInput = false;
                        }
                    }
                    else
                    {
                        //Populate Step5WitnessQuestionInput model with default values.
                        Step5WitnessQuestionInput = new Step5WitnessQuestionInputModel();
                    }

                    //Get the witness from "Witness" object stored in session.
                    Witness? sessionWitness = HttpContext.Session.GetFromSession<Witness>("Witness");

                    //User has previously input witness and has clicked the back button on step6.
                    if (sessionWitness != null)
                    {
                        //Populate Witness model with session values.
                        Witness = sessionWitness;
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

        #region Witness Question Selected
        //Method Summary:
        //This method is executed when the witness question is selected.
        //When executed the selected yes no value is stored in temp data.
        public async Task<IActionResult> OnPostWitnessYesNo()
        {

            //Store the yes no value in temp data.
            TempData["WitnessYesNo"] = Step5WitnessQuestionInput.YesNo;
            TempData.Keep();

            //Get the witness from "Witness" object stored in session.
            Witness? sessionWitness = HttpContext.Session.GetFromSession<Witness>("Witness");

            //The user has selected yes to the witness question.
            if (Step5WitnessQuestionInput.YesNo == 2)
            {
                ShowWitnessInput = true;

                //The witness object exists in session.
                if (sessionWitness != null)
                {
                    Witness = sessionWitness;
                }
            }
            //The user has selected no to the witness question.
            else
            {
                ShowWitnessInput = false;

                //The witness object exists in session.
                if (sessionWitness != null)
                {
                    Witness = null;

                    //Remove the witness object from session.
                    HttpContext.Session.Remove("Witness");
                }
            }

            TempData.Keep();

            return Page();
        }
        #endregion Legal Rep Question Selected

        #region Witness Input
        //Method Summary:
        //This method is executed when the witness input is submitted.
        //When executed the witness object is created and stored in session.
        public async Task<IActionResult> OnPostCreateWitness()
        {
            //Ensure the wiitness input is shown.
            ShowWitnessInput = true;

            //User has selected a witness question answer.
            if (TempData["WitnessYesNo"] != null)
            {
                Step5WitnessQuestionInput.YesNo = (int)TempData["WitnessYesNo"];
            }

            TempData.Keep();

            //Get the ID from the contexts current user, needed for populating CurrentUser property from DB.
            System.Security.Claims.Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
            string? jwtToken = jwtTokenClaim.Value;

            //Set the CurrentUser property by calling the GetUser method in the _userService.
            string? userID = _httpContextAccessor.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;
            CurrentUser = await _userService.GetUser(Convert.ToInt32(userID), jwtToken);

            //Populate the Witness object with non form values.
            Witness.ClaimID = 0;
            Witness.InputBy = CurrentUser.Email;
            Witness.InputOn = DateTime.Now;
            Witness.Active = true;

            //Initialise a new ValidationContext to be used to validate the Witness model only.
            ValidationContext validationContext = new ValidationContext(Witness);
            ICollection<ValidationResult> validationResults = new List<ValidationResult>();
            bool isWitnessValid = Validator.TryValidateObject(Witness, validationContext, validationResults, true);

            //The isWitnessValid model is valid.
            if (isWitnessValid)
            {
                //Create a new Witness object and populate it with the values from the Witness model.
                Witness witness = new Witness();
                witness.ClaimID = 0;
                witness.Title = Witness.Title;
                witness.FirstName = Witness.FirstName;
                witness.LastName = Witness.LastName;
                witness.Email = Witness.Email;
                witness.ContactNumber = Witness.ContactNumber;
                witness.Postcode = Witness.Postcode;
                witness.AddressLine1 = Witness.AddressLine1;
                witness.AddressLine2 = Witness.AddressLine2;
                witness.AddressLine3 = Witness.AddressLine3;
                witness.InputBy = CurrentUser.Email;
                witness.InputOn = DateTime.Now;
                witness.Active = true;

                //Store the witness object in session.
                HttpContext.Session.SetInSession("Witness", Witness);

                TempData.Keep();

                SaveSuccess = true;

                return Page();
            }
            else
            {
                //Display each of the validation errors.
                foreach (ValidationResult validationResult in validationResults)
                {
                    ModelState.AddModelError(string.Empty, validationResult.ErrorMessage);
                }

                TempData.Keep();

                return Page();
            }
        }
        #endregion Witness Input

        #region Page Buttons
        //Method Summary:
        //This method is executed when the "Next" button is clicked.
        //When executed a new Claim object is created and stored in session, the user is then redirected to Step2 page.
        public async Task<IActionResult> OnPostNext()
        {
            //User has selected a witness question answer.
            if (TempData["WitnessYesNo"] != null)
            {
                Step5WitnessQuestionInput.YesNo = (int)TempData["WitnessYesNo"];

                //Hide or show the witness input based on the yes no value.
                if (Step5WitnessQuestionInput.YesNo == 2)
                {
                    ShowWitnessInput = true;
                }
                else
                {
                    ShowWitnessInput = false;
                }
            }

            //Get the ID from the contexts current user, needed for populating CurrentUser property from DB.
            System.Security.Claims.Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
            string? jwtToken = jwtTokenClaim.Value;

            //Set the CurrentUser property by calling the GetUser method in the _userService.
            string? userID = _httpContextAccessor.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;
            CurrentUser = await _userService.GetUser(Convert.ToInt32(userID), jwtToken);

            //Initialise a new ValidationContext to be used to validate the Step5WitnessQuestionInput model only.
            ValidationContext validationContext = new ValidationContext(Step5WitnessQuestionInput);
            ICollection<ValidationResult> validationResults = new List<ValidationResult>();
            bool isStep5WitnessQuestionInputValid = Validator.TryValidateObject(Step5WitnessQuestionInput, validationContext, validationResults, true);

            //The isStep5WitnessQuestionInputValid model is valid.
            if (isStep5WitnessQuestionInputValid)
            {
                //The user has selected yes to the witness question.
                if (Step5WitnessQuestionInput.YesNo == 2)
                {
                    //Get the witness from "Witness" object stored in session.
                    Witness? sessionWitness = HttpContext.Session.GetFromSession<Witness>("Witness");

                    if (sessionWitness != null)
                    {
                        //Set the Witness property to the sessionWitness object.
                        Witness = sessionWitness;

                        //Initialise a new ValidationContext to be used to validate the LegalRep model only.
                        validationContext = new ValidationContext(Witness);
                        validationResults = new List<ValidationResult>();
                        bool isWitnessValid = Validator.TryValidateObject(Witness, validationContext, validationResults, true);

                        //The isWitnessValid model is valid.
                        if (isWitnessValid)
                        {
                            //Store the witness object in session.
                            HttpContext.Session.SetInSession("Witness", Witness);
                        }
                        else
                        {
                            //Display each of the validation errors.
                            foreach (ValidationResult validationResult in validationResults)
                            {
                                ModelState.AddModelError(string.Empty, validationResult.ErrorMessage);
                            }

                            TempData.Keep();

                            return Page();
                        }
                    }
                    else
                    {
                        //Initialise a new Witness object, this is needed for the validation process.
                        Witness = new Witness();

                        //Initialise a new ValidationContext to be used to validate the Witness model only.
                        validationContext = new ValidationContext(Witness);
                        validationResults = new List<ValidationResult>();
                        bool isWitnessValid = Validator.TryValidateObject(Witness, validationContext, validationResults, true);

                        //The isWitnessValid model is valid.
                        if (isWitnessValid)
                        {
                            //Store the legal rep object in session.
                            HttpContext.Session.SetInSession("Witness", Witness);
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "You have not saved any witness details.");

                            //Display each of the validation errors.
                            foreach (ValidationResult validationResult in validationResults)
                            {
                                ModelState.AddModelError(validationResult.MemberNames.ToString(), validationResult.ErrorMessage);
                            }

                            //Set the Witness object to null.
                            Witness = null;

                            TempData.Keep();

                            return Page();
                        }
                    }
                }

                //Store the witness question answer object in session.
                HttpContext.Session.SetInSession("Step5WitnessQuestionInput", Step5WitnessQuestionInput);

                //Redirect to Step6 page.
                return Redirect("/Claims/SubmitClaim/Step6");
            }
            else
            {
                //Display each of the validation errors.
                foreach (ValidationResult validationResult in validationResults)
                {
                    ModelState.AddModelError(string.Empty, validationResult.ErrorMessage);
                    ModelState.AddModelError("Step5WitnessQuestionInput.YesNo", validationResult.ErrorMessage);
                }

                TempData.Keep();

                return Page();
            }
        }

        //Method Summary:
        //This method is executed when the back button is clicked.
        //When executed the user is redirected to Step4 page.
        public async Task<IActionResult> OnPostBack()
        {
            return Redirect("/Claims/SubmitClaim/Step4");
        }
        #endregion Page Buttons
    }
}
