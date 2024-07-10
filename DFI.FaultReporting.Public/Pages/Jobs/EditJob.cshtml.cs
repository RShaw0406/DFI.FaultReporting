using DFI.FaultReporting.Interfaces.Admin;
using DFI.FaultReporting.Interfaces.FaultReports;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Models.FaultReports;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.FaultReports;
using DFI.FaultReporting.Services.Interfaces.Admin;
using DFI.FaultReporting.Services.Interfaces.Emails;
using DFI.FaultReporting.Services.Interfaces.Pagination;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Users;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using SendGrid.Helpers.Mail;
using System.ComponentModel;
using System.Security.Claims;

namespace DFI.FaultReporting.Public.Pages.Jobs
{
    public class EditJobModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<EditJobModel> _logger;
        private readonly IUserService _userService;
        private readonly IContractorService _contractorService;
        private readonly IRepairService _repairService;
        private readonly IRepairStatusService _repairStatusService;
        private readonly IFaultService _faultService;
        private readonly IFaultPriorityService _faultPriorityService;
        private readonly IFaultStatusService _faultStatusService;
        private readonly IFaultTypeService _faultTypeService;
        private readonly IReportService _reportService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;

        //Inject dependencies in constructor.
        public EditJobModel(ILogger<EditJobModel> logger, IFaultService faultService, IFaultTypeService faultTypeService, IUserService userService, IRepairService repairService,
            IFaultPriorityService faultPriorityService, IFaultStatusService faultStatusService, IRepairStatusService repairStatusService, IHttpContextAccessor httpContextAccessor, 
            IEmailService emailService, IContractorService contractorService, IReportService reportService)
        {
            _logger = logger;
            _userService = userService;
            _repairService = repairService;
            _repairStatusService = repairStatusService;
            _faultService = faultService;
            _faultPriorityService = faultPriorityService;
            _faultStatusService = faultStatusService;
            _faultTypeService = faultTypeService;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
            _contractorService = contractorService;
            _reportService = reportService;
        }
        #endregion Dependency Injection

        #region Properties
        public User CurrentUser { get; set; }
        public Contractor Contractor { get; set; }

        [BindProperty]
        public Repair Repair { get; set; }

        [DisplayName("Day")]
        public int TargetDateDay { get; set; }

        [DisplayName("Month")]
        public int TargetDateMonth { get; set; }

        [DisplayName("Year")]
        public int TargetDateYear { get; set; }

        public List<RepairStatus> RepairStatuses { get; set; }

        public IEnumerable<SelectListItem> RepairStatusList { get; set; }

        public Fault Fault { get; set; }

        public List<FaultPriority> FaultPriorities { get; set; }

        public List<FaultStatus> FaultStatuses { get; set; }

        public List<FaultType> FaultTypes { get; set; }

        public List<Report> Reports { get; set; }

        public bool UpdateSuccess { get; set; }
        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is called when the page is loaded.
        //It checks if the current user is authenticated and if so, it gets the repair details from the DB.
        public async Task<IActionResult> OnGetAsync(int? ID)
        {
            //The contexts current user exists.
            if (_httpContextAccessor.HttpContext.User != null)
            {
                //The contexts current user has been authenticated and has contractor role.
                if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true && HttpContext.User.IsInRole("Contractor"))
                {
                    //Clear session and tempdata to ensure fresh start.
                    HttpContext.Session.Clear();
                    TempData.Clear();

                    await PopulateProperties(ID);

                    //Store the repair status ID in TempData, this is needed to check if the status has changed when the form is submitted.
                    TempData["StatusID"] = Repair.RepairStatusID;
                    //Store the repair ID in TempData, this is needed populate page properties, when the form is submitted.
                    TempData["RepairID"] = Repair.ID;
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

        #region Edit Job
        //Method Summary:
        //This method is executed when the update button is clicked.
        //When executed, it validates the form and updates the repair in the DB.
        public async Task<IActionResult> OnPostAsync()
        {
            //Get the repair ID from TempData.
            int RepairID = Convert.ToInt32(TempData["RepairID"]);

            //Modelstate is not valid, so get all validation errors and return the page.
            if (!ModelState.IsValid)
            {
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

                await PopulateProperties(RepairID);

                return Page();
            }
            else
            {
                Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
                string? jwtToken = jwtTokenClaim.Value;


                //Check if the repair status ID has changed.
                if (Repair.RepairStatusID != (int)TempData["StatusID"])
                {
                    //Repair status has changed to 'Repaired'.
                    if (Repair.RepairStatusID == 3)
                    {
                        //Set the actual repair date to the current date.
                        Repair.ActualRepairDate = DateTime.Now;
                    }
                }

                //Attempt to update the repair in the DB.
                Repair updatedRepair = await _repairService.UpdateRepair(Repair, jwtToken);

                //Repair was updated.
                if (updatedRepair != null)
                {
                    await PopulateProperties(RepairID);

                    UpdateSuccess = true;

                    //Check if the repair status ID has changed.
                    if (updatedRepair.RepairStatusID != (int)TempData["StatusID"])
                    {
                        //Repair status has changed to 'Repaired'.
                        if (updatedRepair.RepairStatusID == 3)
                        {
                            //Attempt to update the fault status to 'Repaired'.
                            Fault.FaultStatusID = 4;
                            Fault updatedFault = await _faultService.UpdateFault(Fault, jwtToken);

                            //Attempt to send an email to each of the users who reported the fault.
                            List<Report> faultReports = Reports.Where(r => r.FaultID == updatedFault.ID).ToList();

                            foreach (Report report in faultReports)
                            {
                                SendGrid.Response response = await SendFaultUpdateEmail(report.InputBy);
                            }
                        }
                    }

                    //Store the repair status ID in TempData.
                    TempData["StatusID"] = updatedRepair.RepairStatusID;
                    TempData.Keep();

                    return Page();
                }
                //Fault could not be updated.
                else
                {
                    ModelState.AddModelError(string.Empty, "An error occure when updating the repair");

                    TempData.Keep();

                    await PopulateProperties(RepairID);

                    return Page();
                }
            }
        }
        #endregion Edit Job

        #region Email
        //Method Summary:
        //This method is executed when the fault status has changed "Update" button is clicked.
        //When executed this method attempts to send an email to users who reported the fault and returns the response from the _emailService.
        public async Task<SendGrid.Response> SendFaultUpdateEmail(string emailAddress)
        {
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
            string? jwtToken = jwtTokenClaim.Value;

            //Get the status, priority and type of the fault, this is needed for sending info in the email.
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
        //This method is excuted when the page loads or when the user submits the form.
        //When excuted, it populates the page properties.
        public async Task PopulateProperties(int? ID) 
        {
            //Get the current user ID and JWT token.
            string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
            string? jwtToken = jwtTokenClaim.Value;
            CurrentUser = await _userService.GetUser(Convert.ToInt32(userID), jwtToken);

            Repair = await _repairService.GetRepair(Convert.ToInt32(ID), jwtToken);

            Contractor = await _contractorService.GetContractor(Repair.ContractorID, jwtToken);

            //Populate the target date day, month and year from the repair target date.
            TargetDateDay = Repair.RepairTargetDate.Day;
            TargetDateMonth = Repair.RepairTargetDate.Month;
            TargetDateYear = Repair.RepairTargetDate.Year;

            RepairStatuses = await _repairStatusService.GetRepairStatuses(jwtToken);
            RepairStatuses = RepairStatuses.Where(rs => rs.Active == true).ToList();

            //Populate the repair status list, this is needed for the repair status dropdown options.
            RepairStatusList = RepairStatuses.Select(rs => new SelectListItem
            {
                Text = rs.RepairStatusDescription,
                Value = rs.ID.ToString()
            });

            Fault = await _faultService.GetFault(Repair.FaultID, jwtToken);

            FaultPriorities = await _faultPriorityService.GetFaultPriorities();

            FaultStatuses = await _faultStatusService.GetFaultStatuses();

            FaultTypes = await _faultTypeService.GetFaultTypes();

            Reports = await _reportService.GetReports();
        }
        #endregion Data
    }
}
