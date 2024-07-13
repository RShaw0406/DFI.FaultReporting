using DFI.FaultReporting.Common.Pagination;
using DFI.FaultReporting.Common.SessionStorage;
using DFI.FaultReporting.Interfaces.Admin;
using DFI.FaultReporting.Interfaces.FaultReports;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Models.FaultReports;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.FaultReports;
using DFI.FaultReporting.Services.Interfaces.Admin;
using DFI.FaultReporting.Services.Interfaces.Pagination;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Users;
using DFI.FaultReporting.Services.Users;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace DFI.FaultReporting.Admin.Pages.Faults
{
    public class ReportDetailsModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<ReportDetailsModel> _logger;
        private readonly IStaffService _staffService;
        private readonly IFaultService _faultService;
        private readonly IFaultPriorityService _faultPriorityService;
        private readonly IFaultStatusService _faultStatusService;
        private readonly IFaultTypeService _faultTypeService;
        private readonly IReportService _reportService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPagerService _pagerService;
        private readonly ISettingsService _settingsService;

        //Inject dependencies in constructor.
        public ReportDetailsModel(ILogger<ReportDetailsModel> logger, IStaffService staffService, IFaultService faultService, IFaultTypeService faultTypeService,
            IFaultPriorityService faultPriorityService, IFaultStatusService faultStatusService, IReportService reportService,
            IHttpContextAccessor httpContextAccessor, IPagerService pagerService, ISettingsService settingsService)
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
        }
        #endregion Dependency Injection

        #region Properties
        public Staff CurrentStaff { get; set; }

        [BindProperty]
        public Fault Fault { get; set; }

        [BindProperty]
        public List<Report> Reports { get; set; }

        [BindProperty]
        public List<Report> PagedReports { get; set; }

        [BindProperty]
        public FaultPriority FaultPriority { get; set; }

        [BindProperty]
        public FaultStatus FaultStatus { get; set; }

        [BindProperty]
        public FaultType FaultType { get; set; }

        [BindProperty(SupportsGet = true)]
        public Pager Pager { get; set; } = new Pager();
        #endregion Properties


        #region Page Load
        //Method Summary:
        //This method is executed when the page loads.
        //When executed the details of the reported fault selected by the user are populated and displayed on screen.
        public async Task<IActionResult> OnGetAsync(int? ID)
        {
            //The contexts current user exists.
            if (_httpContextAccessor.HttpContext.User != null)
            {
                //The contexts current user has been authenticated.
                if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true && HttpContext.User.IsInRole("StaffReadWrite") || HttpContext.User.IsInRole("StaffRead"))
                {

                    await PopulateProperties((int)ID);

                    //Set the ID in tempdata.
                    TempData["ID"] = ID;
                    TempData.Keep();
                
                    //Setup the pager.
                    Pager.CurrentPage = 1;
                    Pager.PageSize = await _settingsService.GetSettingInt(DFI.FaultReporting.Common.Constants.Settings.PAGESIZE);
                    Pager.Count = Reports.Count;
                    PagedReports = await _pagerService.GetPaginatedReports(Reports, Pager.CurrentPage, Pager.PageSize);

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

        #region Pagination
        //Method Summary:
        //This method is excuted when the pagination buttons are clicked.
        //When executed the desired page of reports is displayed.
        public async Task OnGetPaging()
        {
            await PopulateProperties((int)TempData["ID"]);

            //Keep the TempData.
            TempData.Keep();

            //Setup the pager.
            Pager.PageSize = await _settingsService.GetSettingInt(DFI.FaultReporting.Common.Constants.Settings.PAGESIZE);
            Pager.Count = Reports.Count;
            PagedReports = await _pagerService.GetPaginatedReports(Reports, Pager.CurrentPage, Pager.PageSize);
        }
        #endregion Pagination

        #region Data
        //Method Summary:
        //This method is excuted when the a post occurs.
        //When excuted, it populates the page properties.
        public async Task PopulateProperties(int ID)
        {
            //Get the current user ID and JWT token.
            string? userID = _httpContextAccessor.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
            string? jwtToken = jwtTokenClaim.Value;
            CurrentStaff = await _staffService.GetStaff(Convert.ToInt32(userID), jwtToken);

            Fault = await _faultService.GetFault(Convert.ToInt32(ID), jwtToken);

            FaultType = await _faultTypeService.GetFaultType(Fault.FaultTypeID, jwtToken);

            FaultStatus = await _faultStatusService.GetFaultStatus(Fault.FaultStatusID, jwtToken);

            FaultPriority = await _faultPriorityService.GetFaultPriority(Fault.FaultPriorityID, jwtToken);

            Reports = await _reportService.GetReports();

            Reports = Reports.Where(r => r.FaultID == Fault.ID).ToList();
        }
        #endregion Data
    }
}
