using DFI.FaultReporting.Common.Pagination;
using DFI.FaultReporting.Interfaces.Admin;
using DFI.FaultReporting.Interfaces.FaultReports;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Models.FaultReports;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Admin;
using DFI.FaultReporting.Services.Interfaces.Admin;
using DFI.FaultReporting.Services.Interfaces.Emails;
using DFI.FaultReporting.Services.Interfaces.Pagination;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SendGrid.Helpers.Mail;
using System.ComponentModel;
using System.Security.Claims;

namespace DFI.FaultReporting.Admin.Pages.Faults
{
    public class EditFaultModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<EditFaultModel> _logger;
        private readonly IStaffService _staffService;
        private readonly IFaultService _faultService;
        private readonly IFaultPriorityService _faultPriorityService;
        private readonly IFaultStatusService _faultStatusService;
        private readonly IFaultTypeService _faultTypeService;
        private readonly IReportService _reportService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPagerService _pagerService;
        private readonly ISettingsService _settingsService;
        private readonly IEmailService _emailService;

        //Inject dependencies in constructor.
        public EditFaultModel(ILogger<EditFaultModel> logger, IStaffService staffService, IFaultService faultService, IFaultTypeService faultTypeService,
            IFaultPriorityService faultPriorityService, IFaultStatusService faultStatusService, IReportService reportService,
            IHttpContextAccessor httpContextAccessor, IPagerService pagerService, ISettingsService settingsService, IEmailService emailService)
        {
            _logger = logger;
            _staffService = staffService;
            _faultService = faultService;
            _faultPriorityService = faultPriorityService;
            _faultStatusService = faultStatusService;
            _faultTypeService = faultTypeService;
            _reportService = reportService;
            _httpContextAccessor = httpContextAccessor;
            _pagerService = pagerService;
            _settingsService = settingsService;
            _emailService = emailService;
        }
        #endregion Dependency Injection

        #region Properties
        public Staff CurrentStaff { get; set; }

        [BindProperty]
        public Fault Fault { get; set; }

        [BindProperty]
        public List<FaultPriority> FaultPriorities { get; set; }

        [BindProperty]
        public IEnumerable<SelectListItem> FaultPriorityList { get; set; }

        [BindProperty]
        public List<FaultStatus> FaultStatuses { get; set; }

        [BindProperty]
        public IEnumerable<SelectListItem> FaultStatusList { get; set; }

        [BindProperty]
        public List<FaultType> FaultTypes { get; set; }

        [BindProperty]
        public IEnumerable<SelectListItem> FaultTypesList { get; set; }

        public bool UpdateSuccess { get; set; }
        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is called when the page is loaded.
        //It checks if the current user is authenticated and if so, it gets the fault details from the DB.
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

                    //Get fault from the DB.
                    Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
                    string? jwtToken = jwtTokenClaim.Value;
                    Fault = await _faultService.GetFault((int)ID, jwtToken);

                    //Store the fault status ID in TempData.
                    TempData["StatusID"] = Fault.FaultStatusID;
                    TempData.Keep();

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
                //Update fault in the DB.
                Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
                string? jwtToken = jwtTokenClaim.Value;
                Fault updatedFault = await _faultService.UpdateFault(Fault, jwtToken);

                //Fault was updated.
                if (updatedFault != null)
                {
                    UpdateSuccess = true;

                    //Check if the fault status ID has changed.
                    if (updatedFault.FaultStatusID != (int)TempData["StatusID"])
                    {
                        //Get all reports from the DB.
                        List<Report> reports = await _reportService.GetReports();

                        foreach (Report report in reports)
                        {
                            //If the fault ID from the report is the same as the updated fault ID.
                            if (report.FaultID == updatedFault.ID)
                            {
                                //Send email to user who reported the fault.
                                SendGrid.Response response = await SendFaultUpdateEmail(report.InputBy);
                            }
                        }
                    }

                    //Store the fault status ID in TempData.
                    TempData["StatusID"] = updatedFault.FaultStatusID;
                    TempData.Keep();

                    return Page();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Fault could not be updated");

                    TempData.Keep();

                    return Page();
                }
            }
        }
        #endregion Edit Fault

        #region Email
        //Method Summary:
        //This method is executed when the fault status has changed "Update" button is clicked.
        //When executed this method attempts to send an email to user who reported the fault and returns the response from the _emailService.
        public async Task<SendGrid.Response> SendFaultUpdateEmail(string emailAddress)
        {
            //Get the status, priority and type of the fault, this is needed for sending info in the email.
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
            string? jwtToken = jwtTokenClaim.Value;
            FaultStatus faultStatus = await _faultStatusService.GetFaultStatus(Fault.FaultStatusID, jwtToken);
            FaultPriority faultPriority = await _faultPriorityService.GetFaultPriority(Fault.FaultPriorityID, jwtToken);
            FaultType faultType = await _faultTypeService.GetFaultType(Fault.FaultTypeID, jwtToken);

            //Construct the email.
            string subject = "DFI Fault Reporting Administration: Reported fault status has changed";
            EmailAddress to = new EmailAddress(emailAddress);
            string textContent = string.Empty;
            string htmlContent = "<p>Hello,</p><p>The status of your reported fault has changed.</p>" +
                "<p><strong>Reported fault details:</strong></p>" +
                faultType.FaultTypeDescription + "<br/>" +
                Fault.RoadNumber + ", " + Fault.RoadName + ", " + Fault.RoadTown + ", " + Fault.RoadCounty + "<br/><br/>" +
                "<p><strong>Status has been changed to:</strong></p>" +
                faultStatus.FaultStatusDescription + "<br/><br/>" +
                "You can continue to track the progress of your reported fault using the DFI Fault Reporting application.";
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
            string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
            string? jwtToken = jwtTokenClaim.Value;
            CurrentStaff = await _staffService.GetStaff(Convert.ToInt32(userID), jwtToken);

            //Get fault types for dropdown.
            FaultTypes = await _faultTypeService.GetFaultTypes();
            FaultTypes = FaultTypes.Where(ft => ft.Active == true).ToList();

            FaultTypesList = FaultTypes.Select(ft => new SelectListItem()
            {
                Text = ft.FaultTypeDescription,
                Value = ft.ID.ToString()
            });

            //Get all fault priorities by calling the GetFaultPriorities method from the _faultPriorityService.
            FaultPriorities = await _faultPriorityService.GetFaultPriorities();
            FaultPriorities = FaultPriorities.Where(fp => fp.Active == true).ToList();

            FaultPriorityList = FaultPriorities.Select(fp => new SelectListItem()
            {
                Text = fp.FaultPriorityRating,
                Value = fp.ID.ToString()
            });

            //Get all fault statuses by calling the GetFaultStatuses method from the _faultStatusService.
            FaultStatuses = await _faultStatusService.GetFaultStatuses();
            FaultStatuses = FaultStatuses.Where(fs => fs.Active == true).ToList();

            //Remove the repaired status from the fault statuses list as this status is only set by contractors through the jobs page on the public app.
            FaultStatuses = FaultStatuses.Where(fs => fs.ID != 4).ToList();

            FaultStatusList = FaultStatuses.Select(fs => new SelectListItem()
            {
                Text = fs.FaultStatusDescription,
                Value = fs.ID.ToString()
            });
        }
        #endregion Data
    }
}
