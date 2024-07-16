using DFI.FaultReporting.Admin.Pages.Faults;
using DFI.FaultReporting.Interfaces.Admin;
using DFI.FaultReporting.Interfaces.Claims;
using DFI.FaultReporting.Interfaces.FaultReports;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Models.Claims;
using DFI.FaultReporting.Models.FaultReports;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Admin;
using DFI.FaultReporting.Services.FaultReports;
using DFI.FaultReporting.Services.Interfaces.Admin;
using DFI.FaultReporting.Services.Interfaces.Emails;
using DFI.FaultReporting.Services.Interfaces.Pagination;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Users;
using DFI.FaultReporting.Services.Users;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SendGrid.Helpers.Mail;
using System.ComponentModel;

namespace DFI.FaultReporting.Admin.Pages.Claims
{
    public class EditClaimModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<EditClaimModel> _logger;
        private readonly IStaffService _staffService;
        private readonly IClaimService _claimService;
        private readonly IClaimTypeService _claimTypeService;
        private readonly IClaimStatusService _claimStatusService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPagerService _pagerService;
        private readonly ISettingsService _settingsService;
        private readonly IEmailService _emailService;

        //Inject dependencies in constructor.
        public EditClaimModel(ILogger<EditClaimModel> logger, IStaffService staffService, IClaimService claimService, IClaimTypeService claimTypeService, IClaimStatusService claimStatusService,
            IHttpContextAccessor httpContextAccessor, IPagerService pagerService, ISettingsService settingsService, IEmailService emailService)
        {
            _logger = logger;
            _staffService = staffService;
            _claimService = claimService;
            _claimTypeService = claimTypeService;
            _claimStatusService = claimStatusService;
            _httpContextAccessor = httpContextAccessor;
            _pagerService = pagerService;
            _settingsService = settingsService;
            _emailService = emailService;
        }
        #endregion Dependency Injection

        #region Properties
        public Staff CurrentStaff { get; set; }

        [BindProperty]
        public Claim Claim { get; set; }

        public List<ClaimType> ClaimTypes { get; set; }

        [BindProperty]
        public IEnumerable<SelectListItem> ClaimTypesList { get; set; }

        public List<ClaimStatus> ClaimStatuses { get; set; }

        [BindProperty]
        public IEnumerable<SelectListItem> ClaimStatusList { get; set; }

        public bool UpdateSuccess { get; set; }
        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is called when the page is loaded.
        //It checks if the current user is authenticated and if so, it gets the claim details from the DB.
        public async Task<IActionResult> OnGetAsync(int? ID)
        {
            //The contexts current user exists.
            if (_httpContextAccessor.HttpContext.User != null)
            {
                //The contexts current user has been authenticated and has admin role.
                if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true && HttpContext.User.IsInRole("StaffReadWrite"))
                {
                    //Clear session to ensure fresh start.
                    HttpContext.Session.Clear();

                    await PopulateProperties();

                    //Get Claim from the DB.
                    System.Security.Claims.Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
                    string? jwtToken = jwtTokenClaim.Value;
                    Claim = await _claimService.GetClaim((int)ID, jwtToken);

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

        #region Edit Fault
        //Method Summary:
        //This method is executed when the update button is clicked.
        //When executed, it updates the fault in the DB.
        public async Task<IActionResult> OnPostAsync()
        {
            await PopulateProperties();

            //Modelstate is not valid.
            if (!ModelState.IsValid)
            {
                //Display each of the model state errors.
                foreach (var item in ModelState)
                {
                    if (item.Value.Errors.Count > 0)
                    {
                        foreach (var error in item.Value.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.ErrorMessage);
                        }
                    }
                }

                return Page();
            }
            //Modelstate is valid.
            else
            {
                //Update claim in the DB.
                System.Security.Claims.Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
                string? jwtToken = jwtTokenClaim.Value;
                Claim updatedClaim = await _claimService.UpdateClaim(Claim, jwtToken);

                //Claim was updated.
                if (updatedClaim != null)
                {
                    UpdateSuccess = true;

                    //Send email to user who submitted the claim.
                    SendGrid.Response response = await SendClaimUpdateEmail(updatedClaim.InputBy);

                    return Page();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Claim could not be updated");

                    TempData.Keep();

                    return Page();
                }
            }
        }
        #endregion Edit Fault

        #region Email
        //Method Summary:
        //This method is executed when the  "Update" button is clicked.
        //When executed this method attempts to send an email to user who submitted the claim and returns the response from the _emailService.
        public async Task<SendGrid.Response> SendClaimUpdateEmail(string emailAddress)
        {
            //Get the status, priority and type of the fault, this is needed for sending info in the email.
            System.Security.Claims.Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
            string? jwtToken = jwtTokenClaim.Value;

            ClaimStatus claimStatus = await _claimStatusService.GetClaimStatus(Claim.ClaimStatusID, jwtToken);
            ClaimType claimType = await _claimTypeService.GetClaimType(Claim.ClaimTypeID, jwtToken);

            //Construct the email.
            string subject = "DFI Fault Reporting Administration: Submitted compensation claim status has changed";
            EmailAddress to = new EmailAddress(emailAddress);
            string textContent = string.Empty;
            string htmlContent = "<p>Hello,</p><p>The status of your compensation claim has changed.</p>" +
                "<p><strong>Reported fault details:</strong></p>" +
                claimType.ClaimTypeDescription + "<br/>" +
                "<p><strong>Status has been changed to:</strong></p>" +
                claimStatus.ClaimStatusDescription + "<br/><br/>" +
                "You can continue to track the progress of your submitted claim using the DFI Fault Reporting application.";
            SendGrid.Helpers.Mail.Attachment? attachment = null;

            //Call the SendEmail in the _emailService and return the response.
            return await _emailService.SendEmail(subject, to, textContent, htmlContent, attachment);
        }
        #endregion Email

        #region Data
        //Method Summary:
        //This method is excuted when the a post occurs.
        //When excuted, it populates the page properties.
        public async Task PopulateProperties()
        {
            //Get the current user ID and JWT token.
            string? userID = _httpContextAccessor.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;
            System.Security.Claims.Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
            string? jwtToken = jwtTokenClaim.Value;
            CurrentStaff = await _staffService.GetStaff(Convert.ToInt32(userID), jwtToken);

            ClaimTypes = await _claimTypeService.GetClaimTypes(jwtToken);
            ClaimTypesList = ClaimTypes.Select(ct => new SelectListItem
            {
                Value = ct.ID.ToString(),
                Text = ct.ClaimTypeDescription
            });

            ClaimStatuses = await _claimStatusService.GetClaimStatuses(jwtToken);
            ClaimStatusList = ClaimStatuses.Select(cs => new SelectListItem
            {
                Value = cs.ID.ToString(),
                Text = cs.ClaimStatusDescription
            });
        }
        #endregion Data
    }
}
