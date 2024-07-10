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
        //Declare CurrentStaff property, this is needed when calling the _staffService.
        public Staff CurrentStaff { get; set; }

        //Declare Fault property, this is needed for getting the selected fault from the DB.
        [BindProperty]
        public Fault Fault { get; set; }

        //Declare FaultPriorities property, this is needed for populatinh fault priority dropdown list.
        [BindProperty]
        public List<FaultPriority> FaultPriorities { get; set; }

        //Declare FaultPriorityList property, this is needed for populating fault priority dropdown list.
        [BindProperty]
        public IEnumerable<SelectListItem> FaultPriorityList { get; set; }

        //Declare FaultStatuses property, this is needed for populating fault status dropdown list.
        [BindProperty]
        public List<FaultStatus> FaultStatuses { get; set; }

        //Declare FaultStatusList property, this is needed for populating fault status dropdown list.
        [BindProperty]
        public IEnumerable<SelectListItem> FaultStatusList { get; set; }

        //Declare FaultTypes property, this is needed for populating fault types dropdown list.
        [BindProperty]
        public List<FaultType> FaultTypes { get; set; }

        //Declare FaultTypesList property, this is needed for populating fault types dropdown list.
        [BindProperty]
        public IEnumerable<SelectListItem> FaultTypesList { get; set; }

        //Declare UpdateSuccess property, this is needed for displaying the updated success message.
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
                    //Get the ID from the contexts current user, needed for populating CurrentUser property from DB.
                    string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

                    //Get the JWT token claim from the contexts current user, needed for populating CurrentUser property from DB.
                    Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

                    //Set the jwtToken string to the JWT token claims value, needed for populating CurrentUser property from DB.
                    string? jwtToken = jwtTokenClaim.Value;

                    //Set the CurrentStaff property by calling the GetUser method in the _userService.
                    CurrentStaff = await _staffService.GetStaff(Convert.ToInt32(userID), jwtToken);

                    //Get fault from the DB.
                    Fault = await _faultService.GetFault((int)ID, jwtToken);

                    //Store the fault status ID in TempData.
                    TempData["StatusID"] = Fault.FaultStatusID;

                    //Get fault types for dropdown.
                    FaultTypes = await _faultTypeService.GetFaultTypes();

                    //Filter out inactive fault types.
                    FaultTypes = FaultTypes.Where(ft => ft.Active == true).ToList();

                    //Populate fault types dropdown.
                    FaultTypesList = FaultTypes.Select(ft => new SelectListItem()
                    {
                        Text = ft.FaultTypeDescription,
                        Value = ft.ID.ToString()
                    });

                    //Get all fault priorities by calling the GetFaultPriorities method from the _faultPriorityService.
                    FaultPriorities = await _faultPriorityService.GetFaultPriorities();

                    //Filter out inactive fault priorities.
                    FaultPriorities = FaultPriorities.Where(fp => fp.Active == true).ToList();

                    //Populate fault priorities dropdown.
                    FaultPriorityList = FaultPriorities.Select(fp => new SelectListItem()
                    {
                        Text = fp.FaultPriorityRating,
                        Value = fp.ID.ToString()
                    });

                    //Get all fault statuses by calling the GetFaultStatuses method from the _faultStatusService.
                    FaultStatuses = await _faultStatusService.GetFaultStatuses();

                    //Filter out inactive fault statuses.
                    FaultStatuses = FaultStatuses.Where(fs => fs.Active == true).ToList();

                    //Remove the repaired status from the fault statuses list.
                    FaultStatuses = FaultStatuses.Where(fs => fs.ID != 4).ToList();

                    //Populate fault statuses dropdown.
                    FaultStatusList = FaultStatuses.Select(fs => new SelectListItem()
                    {
                        Text = fs.FaultStatusDescription,
                        Value = fs.ID.ToString()
                    });

                    //Keep the TempData.
                    TempData.Keep();

                    //Return the page.
                    return Page();
                }
                else
                {
                    //Redirect user to no permission.
                    return Redirect("/NoPermission");
                }
            }
            else
            {
                //Redirect user to no permission.
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
            //Get fault types for dropdown.
            FaultTypes = await _faultTypeService.GetFaultTypes();

            //Filter out inactive fault types.
            FaultTypes = FaultTypes.Where(ft => ft.Active == true).ToList();

            //Populate fault types dropdown.
            FaultTypesList = FaultTypes.Select(ft => new SelectListItem()
            {
                Text = ft.FaultTypeDescription,
                Value = ft.ID.ToString()
            });

            //Get all fault priorities by calling the GetFaultPriorities method from the _faultPriorityService.
            FaultPriorities = await _faultPriorityService.GetFaultPriorities();

            //Filter out inactive fault priorities.
            FaultPriorities = FaultPriorities.Where(fp => fp.Active == true).ToList();

            //Populate fault priorities dropdown.
            FaultPriorityList = FaultPriorities.Select(fp => new SelectListItem()
            {
                Text = fp.FaultPriorityRating,
                Value = fp.ID.ToString()
            });

            //Get all fault statuses by calling the GetFaultStatuses method from the _faultStatusService.
            FaultStatuses = await _faultStatusService.GetFaultStatuses();

            //Filter out inactive fault statuses.
            FaultStatuses = FaultStatuses.Where(fs => fs.Active == true).ToList();

            //Remove the repaired status from the fault statuses list.
            FaultStatuses = FaultStatuses.Where(fs => fs.ID != 4).ToList();

            //Populate fault statuses dropdown.
            FaultStatusList = FaultStatuses.Select(fs => new SelectListItem()
            {
                Text = fs.FaultStatusDescription,
                Value = fs.ID.ToString()
            });

            //Modelstate is not valid.
            if (!ModelState.IsValid)
            {
                //Loop through all model state items.
                foreach (var item in ModelState)
                {
                    //If the model state error has errors, add them to the model state.
                    if (item.Value.Errors.Count > 0)
                    {
                        //Loop through all errors and add them to the model state.
                        foreach (var error in item.Value.Errors)
                        {
                            //Add the error to the model state.
                            ModelState.AddModelError(string.Empty, error.ErrorMessage);
                        }
                    }
                }

                //Return the page.
                return Page();
            }
            //Modelstate is valid.
            else
            {
                //Get the ID from the contexts current user, needed for populating CurrentUser property from DB.
                string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

                //Get the JWT token claim from the contexts current user, needed for populating CurrentUser property from DB.
                Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

                //Set the jwtToken string to the JWT token claims value, needed for populating CurrentUser property from DB.
                string? jwtToken = jwtTokenClaim.Value;

                //Set the CurrentStaff property by calling the GetUser method in the _userService.
                CurrentStaff = await _staffService.GetStaff(Convert.ToInt32(userID), jwtToken);

                //Update fault in the DB.
                Fault updatedFault = await _faultService.UpdateFault(Fault, jwtToken);

                //Fault was updated.
                if (updatedFault != null)
                {
                    //Set the UpdateSuccess property to true.
                    UpdateSuccess = true;

                    //Check if the fault status ID has changed.
                    if (updatedFault.FaultStatusID != (int)TempData["StatusID"])
                    {
                        //Get all reports from the DB.
                        List<Report> reports = await _reportService.GetReports();

                        //Loop through all reports.
                        foreach (Report report in reports)
                        {
                            //If the fault ID from the report is the same as the updated fault ID.
                            if (report.FaultID == updatedFault.ID)
                            {
                                //Send email to user who reported the fault.
                                SendGrid.Response response = await SendFaultUpdateEmail(report.InputBy);

                                //Email was sent.
                                if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
                                {
                                    //Set the UpdateSuccess property to true.
                                    UpdateSuccess = true;
                                }
                            }
                        }
                    }

                    //Store the fault status ID in TempData.
                    TempData["StatusID"] = updatedFault.FaultStatusID;

                    //Keep the TempData.
                    TempData.Keep();

                    //Return the page.
                    return Page();
                }
                //Fault could not be updated.
                else
                {
                    //Add model state error.
                    ModelState.AddModelError(string.Empty, "Fault could not be updated");

                    //Keep the TempData.
                    TempData.Keep();

                    //Return the page.
                    return Page();
                }
            }
        }

        //Method Summary:
        //This method is executed when the fault status has changed "Update" button is clicked.
        //When executed this method attempts to send an email to user who reported the fault and returns the response from the _emailService.
        public async Task<SendGrid.Response> SendFaultUpdateEmail(string emailAddress)
        {
            //Get the ID from the contexts current user, needed for populating CurrentUser property from DB.
            string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            //Get the JWT token claim from the contexts current user, needed for populating CurrentUser property from DB.
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

            //Set the jwtToken string to the JWT token claims value, needed for populating CurrentUser property from DB.
            string? jwtToken = jwtTokenClaim.Value;

            //Get the status, priority and type of the fault, this is needed for sending info in the email.
            FaultStatus faultStatus = await _faultStatusService.GetFaultStatus(Fault.FaultStatusID, jwtToken);
            FaultPriority faultPriority = await _faultPriorityService.GetFaultPriority(Fault.FaultPriorityID, jwtToken);
            FaultType faultType = await _faultTypeService.GetFaultType(Fault.FaultTypeID, jwtToken);

            //Set the subject of the email explaining that the status of the reported fault has changed.
            string subject = "DFI Fault Reporting Administration: Reported fault status has changed";

            //Declare a new EmailAddress object and assign the user email address as the value.
            EmailAddress to = new EmailAddress(emailAddress);

            //Set textContent to empty string as it will not be used here.
            string textContent = string.Empty;

            //Set the htmlContent to a message explaining to the user that the status of their reported fault has changed.
            string htmlContent = "<p>Hello,</p><p>The status of your reported fault has changed.</p>" +
                "<p><strong>Reported fault details:</strong></p>" +
                faultType.FaultTypeDescription + "<br/>" +
                Fault.RoadNumber + ", " + Fault.RoadName + ", " + Fault.RoadTown + ", " + Fault.RoadCounty + "<br/><br/>" +
                "<p><strong>Status has been changed to:</strong></p>" +
                faultStatus.FaultStatusDescription + "<br/><br/>" +
                "You can continue to track the progress of your reported fault using the DFI Fault Reporting application.";

            //Set the attachment to null as it will not be used here.
            SendGrid.Helpers.Mail.Attachment? attachment = null;

            //Call the SendEmail in the _emailService and return the response.
            return await _emailService.SendEmail(subject, to, textContent, htmlContent, attachment);
        }
        #endregion Edit Fault
    }
}
