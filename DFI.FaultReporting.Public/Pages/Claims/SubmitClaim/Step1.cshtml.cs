using DFI.FaultReporting.Interfaces.Admin;
using DFI.FaultReporting.Interfaces.Claims;
using DFI.FaultReporting.Interfaces.FaultReports;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Claims;
using DFI.FaultReporting.Services.Interfaces.Emails;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Tokens;
using DFI.FaultReporting.Services.Interfaces.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using DFI.FaultReporting.Models.Claims;
using DFI.FaultReporting.Common.SessionStorage;
using DFI.FaultReporting.Models.FaultReports;
using DFI.FaultReporting.Services.Admin;
using DFI.FaultReporting.Services.FaultReports;
using DFI.FaultReporting.Services.Users;
using DocumentFormat.OpenXml.Bibliography;

namespace DFI.FaultReporting.Public.Pages.Claims.SubmitClaim
{
    public class Step1Model : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<Step1Model> _logger;
        private readonly IUserService _userService;
        private readonly IClaimService _claimService;
        private readonly IClaimTypeService _claimTypeService;
        private readonly ILegalRepService _legalRepService;
        private readonly ISettingsService _settingsService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        //Inject dependencies in constructor.
        public Step1Model(ILogger<Step1Model> logger, IUserService userService, IClaimService claimService, IClaimTypeService claimTypeService, 
            ISettingsService settingsService, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _userService = userService;
            _claimService = claimService;
            _claimTypeService = claimTypeService;
            _settingsService = settingsService;
            _httpContextAccessor = httpContextAccessor;
        }
        #endregion Dependency Injection

        #region Properties
        public User CurrentUser { get; set; }

        public List<ClaimType> ClaimTypes { get; set; }

        public IEnumerable<SelectListItem> ClaimTypesList { get; set; }

        [BindProperty]
        public Step1ClaimInputModel Step1ClaimInput { get; set; }

        public class Step1ClaimInputModel
        {
            [DisplayName("Claim type")]
            [Required(ErrorMessage = "You must select a claim type")]
            public int? ClaimTypeID { get; set; } = null;
        }

        [BindProperty]
        public LegalRep? LegalRep { get; set; }

        [BindProperty]
        public Step1LegalRepQuestionInputModel Step1LegalRepQuestionInput { get; set; }

        public class Step1LegalRepQuestionInputModel
        {
            [DisplayName("Are you a legal representative for the claimant?")]
            [Required(ErrorMessage = "You must select an option")]
            public int YesNo { get; set; } = 1;
        }

        [BindProperty]
        public bool ShowLegalRepInput { get; set; } = false;

        [BindProperty]
        public bool SaveSuccess { get; set; } = false;

        #endregion Properties


        #region Page Load
        //Method Summary:
        //This method is executed when the page loads and is used for populating the claim types dropdown list.
        public async Task<IActionResult> OnGetAsync()
        {
            //The contexts current user exists.
            if (_httpContextAccessor.HttpContext.User != null)
            {
                //The contexts current user has been authenticated and has user role.
                if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true && _httpContextAccessor.HttpContext.User.IsInRole("User"))
                {
                    await PopulateProperties();

                    //Get the claim from "Claim" object stored in session.
                    Claim? sessionClaim = HttpContext.Session.GetFromSession<Claim>("Claim");

                    //User has previously input step1 claimtype and has clicked the back button on step2.
                    if (sessionClaim != null)
                    {
                        //Populate Step1ClaimInput model with session values.
                        Step1ClaimInput = new Step1ClaimInputModel();
                        Step1ClaimInput.ClaimTypeID = sessionClaim.ClaimTypeID;
                    }

                    //Get the legal rep question from "Step1LegalRepQuestionInput" object stored in session.
                    Step1LegalRepQuestionInputModel? sessionYesNo = HttpContext.Session.GetFromSession<Step1LegalRepQuestionInputModel>("Step1LegalRepQuestionInput");

                    //User has previously input step1 legal rep question and has clicked the back button on step2.
                    if (sessionYesNo != null)
                    {
                        //Populate Step1LegalRepQuestionInput model with session values.
                        Step1LegalRepQuestionInput = new Step1LegalRepQuestionInputModel();
                        Step1LegalRepQuestionInput.YesNo = sessionYesNo.YesNo;

                        //Hide or show the legal rep input based on the yes no value.
                        if (Step1LegalRepQuestionInput.YesNo == 2)
                        {
                            ShowLegalRepInput = true;
                        }
                        else
                        {
                            ShowLegalRepInput = false;
                        }
                    }
                    else
                    {
                        //Populate Step1LegalRepQuestionInput model with default values.
                        Step1LegalRepQuestionInput = new Step1LegalRepQuestionInputModel();
                    }

                    //Get the legal rep from "LegalRep" object stored in session.
                    LegalRep? sessionLegalRep = HttpContext.Session.GetFromSession<LegalRep>("LegalRep");

                    //User has previously input legal rep and has clicked the back button on step2.
                    if (sessionLegalRep != null)
                    {
                        //Populate LegalRep model with session values.
                        LegalRep = sessionLegalRep;
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

        #region Claim Type Selected
        //Method Summary:
        //This method is executed when the claim type is selected.
        //When executed the selected claim type ID is stored in temp data.
        public async Task<IActionResult> OnPostClaimTypeSelected()
        {
            await PopulateProperties();

            //User has selected an answer to the legal rep question.
            if (TempData["LegalRepYesNo"] != null)
            {
                Step1LegalRepQuestionInput.YesNo = (int)TempData["LegalRepYesNo"];

                //User has selected yes to the legal rep question.
                if (Step1LegalRepQuestionInput.YesNo == 2)
                {
                    ShowLegalRepInput = true;

                    //Get the legal rep from "LegalRep" object stored in session.
                    LegalRep? sessionLegalRep = HttpContext.Session.GetFromSession<LegalRep>("LegalRep");
                    if (sessionLegalRep != null)
                    {
                        LegalRep = sessionLegalRep;
                    }
                }
                else
                {
                    ShowLegalRepInput = false;
                }
            }

            //Store the selected claim type ID in temp data.
            TempData["ClaimTypeID"] = Step1ClaimInput.ClaimTypeID;
            TempData.Keep();

            return Page();
        }
        #endregion Claim Type Selected

        #region Legal Rep Question Selected
        //Method Summary:
        //This method is executed when the legal rep question is selected.
        //When executed the selected yes no value is stored in temp data.
        public async Task<IActionResult> OnPostLegalRepYesNo()
        {
            await PopulateProperties();

            //User has selected a claim type.
            if (TempData["ClaimTypeID"] != null)
            {
                Step1ClaimInput.ClaimTypeID = (int)TempData["ClaimTypeID"];
            }

            //Store the yes no value in temp data.
            TempData["LegalRepYesNo"] = Step1LegalRepQuestionInput.YesNo;
            TempData.Keep();

            //Get the legal rep from "LegalRep" object stored in session.
            LegalRep? sessionLegalRep = HttpContext.Session.GetFromSession<LegalRep>("LegalRep");

            //The user has selected yes to the legal rep question.
            if (Step1LegalRepQuestionInput.YesNo == 2)
            {
                ShowLegalRepInput = true;

                //The legal rep object exists in session.
                if (sessionLegalRep != null)
                {
                    LegalRep = sessionLegalRep;
                }
            }
            //The user has selected no to the legal rep question.
            else
            {
                ShowLegalRepInput = false;

                //The legal rep object exists in session.
                if (sessionLegalRep != null)
                {
                    LegalRep = null;

                    //Remove the legal rep object from session.
                    HttpContext.Session.Remove("LegalRep");
                }
            }

            TempData.Keep();

            return Page();
        }
        #endregion Legal Rep Question Selected

        #region Legal Rep Input
        //Method Summary:
        //This method is executed when the legal rep input is submitted.
        //When executed the legal rep object is created and stored in session.
        public async Task<IActionResult> OnPostCreateLegalRep()
        {
            //Ensure the legal rep input is shown.
            ShowLegalRepInput = true;

            //User has selected a claim type.
            if (TempData["ClaimTypeID"] != null)
            {
                Step1ClaimInput.ClaimTypeID = (int)TempData["ClaimTypeID"];
            }

            //User has selected a legal rep question answer.
            if (TempData["LegalRepYesNo"] != null)
            {
                Step1LegalRepQuestionInput.YesNo = (int)TempData["LegalRepYesNo"];
            }

            await PopulateProperties();

            TempData.Keep();

            //Get the ID from the contexts current user, needed for populating CurrentUser property from DB.
            System.Security.Claims.Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
            string? jwtToken = jwtTokenClaim.Value;

            //Set the CurrentUser property by calling the GetUser method in the _userService.
            string? userID = _httpContextAccessor.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;
            CurrentUser = await _userService.GetUser(Convert.ToInt32(userID), jwtToken);

            //Populate the LegalRep object with non form values.
            LegalRep.ClaimID = 0;
            LegalRep.InputBy = CurrentUser.Email;
            LegalRep.InputOn = DateTime.Now;
            LegalRep.Active = true;

            //Initialise a new ValidationContext to be used to validate the LegalRep model only.
            ValidationContext validationContext = new ValidationContext(LegalRep);
            ICollection<ValidationResult> validationResults = new List<ValidationResult>();
            bool isLegalRepValid = Validator.TryValidateObject(LegalRep, validationContext, validationResults, true);

            //The isLegalRepValid model is valid.
            if (isLegalRepValid)
            {
                //Create a new LegalRep object and populate it with the values from the LegalRep model.
                LegalRep legalRep = new LegalRep();
                legalRep.ClaimID = 0;
                legalRep.Title = LegalRep.Title;
                legalRep.FirstName = LegalRep.FirstName;
                legalRep.LastName = LegalRep.LastName;
                legalRep.CompanyName = LegalRep.CompanyName;
                legalRep.Postcode = LegalRep.Postcode;
                legalRep.AddressLine1 = LegalRep.AddressLine1;
                legalRep.AddressLine2 = LegalRep.AddressLine2;
                legalRep.AddressLine3 = LegalRep.AddressLine3;
                legalRep.InputBy = CurrentUser.Email;
                legalRep.InputOn = DateTime.Now;
                legalRep.Active = true;

                //Store the legal rep object in session.
                HttpContext.Session.SetInSession("LegalRep", legalRep);

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

        #endregion Legal Rep Input

        #region Page Buttons
        //Method Summary:
        //This method is executed when the "Next" button is clicked.
        //When executed a new Claim object is created and stored in session, the user is then redirected to Step2 page.
        public async Task<IActionResult> OnPostNext()
        {
            //User has selected a claim type.
            if (TempData["ClaimTypeID"] != null)
            {
                Step1ClaimInput.ClaimTypeID = (int)TempData["ClaimTypeID"];
            }

            //User has selected a legal rep question answer.
            if (TempData["LegalRepYesNo"] != null)
            {
                Step1LegalRepQuestionInput.YesNo = (int)TempData["LegalRepYesNo"];

                //Hide or show the legal rep input based on the yes no value.
                if (Step1LegalRepQuestionInput.YesNo == 2)
                {
                    ShowLegalRepInput = true;
                }
                else
                {
                    ShowLegalRepInput = false;
                }
            }

            //Get the ID from the contexts current user, needed for populating CurrentUser property from DB.
            System.Security.Claims.Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
            string? jwtToken = jwtTokenClaim.Value;

            //Set the CurrentUser property by calling the GetUser method in the _userService.
            string? userID = _httpContextAccessor.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;
            CurrentUser = await _userService.GetUser(Convert.ToInt32(userID), jwtToken);

            //Initialise a new ValidationContext to be used to validate the Step1ClaimInput model only.
            ValidationContext validationContext = new ValidationContext(Step1ClaimInput);
            ICollection<ValidationResult> validationResults = new List<ValidationResult>();
            bool isStep1ClaimInputValid = Validator.TryValidateObject(Step1ClaimInput, validationContext, validationResults, true);

            //The isStep1ClaimInputValid model is valid.
            if (isStep1ClaimInputValid)
            {
                //Initialise a new ValidationContext to be used to validate the Step1LegalRepQuestionInput model only.
                validationContext = new ValidationContext(Step1LegalRepQuestionInput);
                validationResults = new List<ValidationResult>();
                bool isStep1LegalRepQuestionInputValid = Validator.TryValidateObject(Step1LegalRepQuestionInput, validationContext, validationResults, true);

                //The isStep1LegalRepQuestionInputValid model is valid.
                if (isStep1LegalRepQuestionInputValid)
                {
                    //The user has selected yes to the legal rep question.
                    if (Step1LegalRepQuestionInput.YesNo == 2)
                    {
                        //Get the legal rep object from session.
                        LegalRep? sessionLegalRep = HttpContext.Session.GetFromSession<LegalRep>("LegalRep");

                        if (sessionLegalRep != null)
                        {
                            //Set the LegalRep property to the sessionLegalRep object.
                            LegalRep = sessionLegalRep;

                            //Initialise a new ValidationContext to be used to validate the LegalRep model only.
                            validationContext = new ValidationContext(LegalRep);
                            validationResults = new List<ValidationResult>();
                            bool isLegalRepValid = Validator.TryValidateObject(LegalRep, validationContext, validationResults, true);

                            //The isLegalRepValid model is valid.
                            if (isLegalRepValid)
                            {
                                //Store the legal rep object in session.
                                HttpContext.Session.SetInSession("LegalRep", LegalRep);
                            }
                            else
                            {
                                //Display each of the validation errors.
                                foreach (ValidationResult validationResult in validationResults)
                                {
                                    ModelState.AddModelError(string.Empty, validationResult.ErrorMessage);
                                }

                                await PopulateProperties();

                                TempData.Keep();

                                return Page();
                            }
                        }
                        else
                        {
                            //Initialise a new LegalRep object, this is needed for the validation process.
                            LegalRep = new LegalRep();

                            //Initialise a new ValidationContext to be used to validate the LegalRep model only.
                            validationContext = new ValidationContext(LegalRep);
                            validationResults = new List<ValidationResult>();
                            bool isLegalRepValid = Validator.TryValidateObject(LegalRep, validationContext, validationResults, true);

                            //The isLegalRepValid model is valid.
                            if (isLegalRepValid)
                            {
                                //Store the legal rep object in session.
                                HttpContext.Session.SetInSession("LegalRep", LegalRep);
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "You have not saved any legal representative details.");

                                //Display each of the validation errors.
                                foreach (ValidationResult validationResult in validationResults)
                                {
                                    ModelState.AddModelError(validationResult.MemberNames.ToString(), validationResult.ErrorMessage);
                                }

                                //Set the LegalRep object to null.
                                LegalRep = null;

                                await PopulateProperties();

                                TempData.Keep();

                                return Page();
                            }
                        }                  
                    }

                    //Create a new Claim object and populate it with the values from the Step1ClaimInput model.
                    Claim claim = new Claim();
                    claim.ClaimTypeID = (int)TempData["ClaimTypeID"];
                    claim.UserID = CurrentUser.ID;
                    claim.InputBy = CurrentUser.Email;
                    claim.InputOn = DateTime.Now;

                    //Get the claim from "Claim" object stored in session.
                    Claim? sessionClaim = HttpContext.Session.GetFromSession<Claim>("Claim");
                    if (sessionClaim != null)
                    {
                        if (sessionClaim.IncidentLocationLatitude != null && sessionClaim.IncidentLocationLongitude != null)
                        {
                            claim.IncidentLocationLatitude = sessionClaim.IncidentLocationLatitude;
                            claim.IncidentLocationLongitude = sessionClaim.IncidentLocationLongitude;
                        }

                        if (sessionClaim.FaultID != null)
                        {
                            claim.FaultID = sessionClaim.FaultID;
                        }

                        if (sessionClaim.IncidentDate != null)
                        {
                            claim.IncidentDate = sessionClaim.IncidentDate;
                        }

                        if (sessionClaim.IncidentDescription != null)
                        {
                            claim.IncidentDescription = sessionClaim.IncidentDescription;
                        }

                        if (sessionClaim.IncidentLocationDescription != null)
                        {
                            claim.IncidentLocationDescription = sessionClaim.IncidentLocationDescription;
                        }

                        if (sessionClaim.InjuryDescription != null)
                        {
                            claim.InjuryDescription = sessionClaim.InjuryDescription;
                        }

                        if (sessionClaim.DamageDescription != null)
                        {
                            claim.DamageDescription = sessionClaim.DamageDescription;
                        }

                        if (sessionClaim.DamageClaimDescription != null)
                        {
                            claim.DamageClaimDescription = sessionClaim.DamageClaimDescription;
                        }
                    }

                    //Store the claim object in session.
                    HttpContext.Session.SetInSession("Claim", claim);

                    //Store the legal rep question answer object in session.
                    HttpContext.Session.SetInSession("Step1LegalRepQuestionInput", Step1LegalRepQuestionInput);

                    //Redirect to Step2 page.
                    return Redirect("/Claims/SubmitClaim/Step2");
                }
                else
                {
                    //Display each of the validation errors.
                    foreach (ValidationResult validationResult in validationResults)
                    {
                        ModelState.AddModelError(string.Empty, validationResult.ErrorMessage);
                        ModelState.AddModelError("Step1LegalRepQuestionInput.YesNo", validationResult.ErrorMessage);
                    }

                    await PopulateProperties();

                    TempData.Keep();

                    return Page();
                }
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
        //When executed the user is redirected to SubmitClaim page.
        public async Task<IActionResult> OnPostBack()
        {
            return Redirect("/Claims/SubmitClaim/SubmitClaim");
        }
        #endregion Page Buttons

        #region Data
        //Method Summary:
        //This method is excuted when the a post occurs.
        //When excuted, it populates the page properties.
        public async Task PopulateProperties()
        {
            System.Security.Claims.Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
            string? jwtToken = jwtTokenClaim.Value;
            ClaimTypes = await _claimTypeService.GetClaimTypes(jwtToken);

            ClaimTypesList = ClaimTypes.Select(ct => new SelectListItem()
            {
                Text = ct.ClaimTypeDescription,
                Value = ct.ID.ToString()
            });
        }
        #endregion Data
    }
}
