using DFI.FaultReporting.Common.SessionStorage;
using DFI.FaultReporting.Interfaces.Admin;
using DFI.FaultReporting.Interfaces.Claims;
using DFI.FaultReporting.Interfaces.Files;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Models.Claims;
using DFI.FaultReporting.Models.FaultReports;
using DFI.FaultReporting.Models.Files;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Admin;
using DFI.FaultReporting.Services.Interfaces.Claims;
using DFI.FaultReporting.Services.Interfaces.Emails;
using DFI.FaultReporting.Services.Interfaces.Files;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Tokens;
using DFI.FaultReporting.Services.Interfaces.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Security.Claims;

namespace DFI.FaultReporting.Public.Pages.Claims.SubmitClaim
{
    public class Step8Model : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<Step8Model> _logger;
        private readonly IUserService _userService;
        private readonly IClaimService _claimService;
        private readonly IClaimFileService _claimFileService;
        private readonly IClaimPhotoService _claimPhotoService;
        private readonly IClaimTypeService _claimTypeService;
        private readonly IFileValidationService _fileValidationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISettingsService _settingsService;
        private readonly IEmailService _emailService;
        private readonly IVerificationTokenService _verificationTokenService;
        private readonly IWitnessService _witnessService;
        private readonly ILegalRepService _legalRepService;

        //Inject dependencies in constructor.
        public Step8Model(ILogger<Step8Model> logger, IUserService userService, IClaimService claimService, IClaimFileService claimFileService,
            IHttpContextAccessor httpContextAccessor, ISettingsService settingsService, IEmailService emailService, IClaimTypeService claimTypeService,
            IVerificationTokenService verificationTokenService, IFileValidationService fileValidationService, IClaimPhotoService claimPhotoService, 
            IWitnessService witnessService, ILegalRepService legalRepService)
        {
            _logger = logger;
            _userService = userService;
            _claimService = claimService;
            _claimFileService = claimFileService;
            _httpContextAccessor = httpContextAccessor;
            _settingsService = settingsService;
            _emailService = emailService;
            _verificationTokenService = verificationTokenService;
            _fileValidationService = fileValidationService;
            _claimPhotoService = claimPhotoService;
            _claimTypeService = claimTypeService;
            _witnessService = witnessService;
            _legalRepService = legalRepService;
        }
        #endregion Dependency Injection


        #region Properties
        public User CurrentUser { get; set; }

        public Models.Claims.Claim Claim { get; set; }

        public ClaimType ClaimType { get; set; }

        public Witness Witness { get; set; }

        public LegalRep LegalRep { get; set; }

        public List<ClaimPhoto> ClaimPhotos { get; set; }

        public List<ClaimFile> ClaimFiles { get; set; }

        public int Day { get; set; }

        public int Month { get; set; }

        public int Year { get; set; }
        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is executed when the page loads.
        //When executed the claim, claimFiles and claimPhotos are retrieved from session storage.
        public async Task<IActionResult> OnGetAsync()
        {
            //The contexts current user exists.
            if (_httpContextAccessor.HttpContext.User != null)
            {
                //The contexts current user has been authenticated and has user role.
                if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true && _httpContextAccessor.HttpContext.User.IsInRole("User"))
                {
                    Claim = HttpContext.Session.GetFromSession<Models.Claims.Claim>("Claim");
                    ClaimFiles = HttpContext.Session.GetFromSession<List<ClaimFile>>("ClaimFiles");
                    ClaimPhotos = HttpContext.Session.GetFromSession<List<ClaimPhoto>>("ClaimPhotos");
                    LegalRep = HttpContext.Session.GetFromSession<LegalRep>("LegalRep");
                    Witness = HttpContext.Session.GetFromSession<Witness>("Witness");

                    System.Security.Claims.Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
                    string? jwtToken = jwtTokenClaim.Value;
                    ClaimType = await _claimTypeService.GetClaimType(Claim.ClaimTypeID, jwtToken);

                    Day = Claim.IncidentDate.Day;
                    Month = Claim.IncidentDate.Month;
                    Year = Claim.IncidentDate.Year;

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

        #region Submit Claim    
        //Method Summary:
        //This method is executed when the submit button is clicked.
        //When executed the session claim, witness, legalrep and claim photos/files list are inserted to the DB and the user is redirected to the SubmittedClaim page.
        public async Task<IActionResult> OnPostSubmitClaim()
        {
            //Get the ID from the contexts current user, needed for populating CurrentUser property from DB.
            string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            //Get the JWT token claim from the contexts current user, needed for populating CurrentUser property from DB.
            System.Security.Claims.Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

            //Set the jwtToken string to the JWT token claims value, needed for populating CurrentUser property from DB.
            string? jwtToken = jwtTokenClaim.Value;

            //Set the CurrentUser property by calling the GetUser method in the _userService.
            CurrentUser = await _userService.GetUser(Convert.ToInt32(userID), jwtToken);

            Claim = HttpContext.Session.GetFromSession<Models.Claims.Claim>("Claim");
            ClaimFiles = HttpContext.Session.GetFromSession<List<ClaimFile>>("ClaimFiles");
            ClaimPhotos = HttpContext.Session.GetFromSession<List<ClaimPhoto>>("ClaimPhotos");
            LegalRep = HttpContext.Session.GetFromSession<LegalRep>("LegalRep");
            Witness = HttpContext.Session.GetFromSession<Witness>("Witness");

            Claim.ClaimStatusID = 8;
            Claim.UserID = CurrentUser.ID;

            //Insert session claim to DB.
            Models.Claims.Claim? insertedClaim = await _claimService.CreateClaim(Claim, jwtToken);

            if (insertedClaim != null)
            {
                if (ClaimFiles != null)
                {
                    foreach (ClaimFile claimFile in ClaimFiles)
                    {
                        claimFile.ClaimID = insertedClaim.ID;

                        //Insert session claim files to DB.
                        await _claimFileService.CreateClaimFile(claimFile, jwtToken);
                    }
                }

                if (ClaimPhotos != null)
                {
                    foreach (ClaimPhoto claimPhoto in ClaimPhotos)
                    {
                        claimPhoto.ClaimID = insertedClaim.ID;

                        //Insert session claim photos to DB.
                        await _claimPhotoService.CreateClaimPhoto(claimPhoto, jwtToken);
                    }
                }

                if (LegalRep != null)
                {
                    LegalRep.ClaimID = insertedClaim.ID;

                    //Insert session legal rep to DB.
                    await _legalRepService.CreateLegalRep(LegalRep, jwtToken);
                }

                if (Witness != null)
                {
                    Witness.ClaimID = insertedClaim.ID;

                    //Insert session witness to DB.
                    await _witnessService.CreateWitness(Witness, jwtToken);
                }

                //Send an email to the user.
                Response emailResponse = await SendSubmittedClaimEmail(CurrentUser.Email);

                return Redirect("./SubmittedClaim");
            }
            else
            {
                //Redirect user to error page.
                return Redirect("/Error");
            }

        }
        #endregion Submit Claim

        #region Email

        //Method Summary:
        //This method is executed when the "Submit" button is clicked.
        //When executed this method attempts to send an email to the user and returns the response from the _emailService.
        public async Task<Response> SendSubmittedClaimEmail(string emailAddress)
        {
            //Get the JWT token claim from the contexts current user, needed for populating CurrentUser property from DB.
            System.Security.Claims.Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

            //Set the jwtToken string to the JWT token claims value, needed for populating CurrentUser property from DB.
            string? jwtToken = jwtTokenClaim.Value;

            Claim = HttpContext.Session.GetFromSession<Models.Claims.Claim>("Claim");
            ClaimFiles = HttpContext.Session.GetFromSession<List<ClaimFile>>("ClaimFiles");
            ClaimPhotos = HttpContext.Session.GetFromSession<List<ClaimPhoto>>("ClaimPhotos");
            LegalRep = HttpContext.Session.GetFromSession<LegalRep>("LegalRep");
            Witness = HttpContext.Session.GetFromSession<Witness>("Witness");

            //Construct email.
            string subject = "DFI Fault Reporting: Compensation Claim Submitted";
            EmailAddress to = new EmailAddress(emailAddress);
            string textContent = string.Empty;
            string htmlContent = "<p>Hello,</p><p>Thank you for submitting a compensation claim.</p>" +
                "<p>DFI staff will now review your claim and you will receive update emails as the status of the claim progresses.</p>" +
                "<p>You can track the progress of your claim using the DFI Fault Reporting application.";
            SendGrid.Helpers.Mail.Attachment? attachment = null;
            return await _emailService.SendEmail(subject, to, textContent, htmlContent, attachment);
        }
        #endregion Email

        #region Page Buttons
        //Method Summary:
        //This method is executed when the back button is clicked.
        //When executed the user is redirected to Step7 page.
        public async Task<IActionResult> OnPostBack()
        {
            return Redirect("/Claims/SubmitClaim/Step7");
        }
        #endregion Page Buttons
    }
}
