using DFI.FaultReporting.Common.Pagination;
using DFI.FaultReporting.Common.SessionStorage;
using DFI.FaultReporting.Interfaces.Admin;
using DFI.FaultReporting.Interfaces.FaultReports;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Models.FaultReports;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Admin;
using DFI.FaultReporting.Services.Interfaces.Pagination;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.Security.Claims;

namespace DFI.FaultReporting.Admin.Pages.Faults.Reports
{
    public class FaultsHeatMapReportModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<FaultsHeatMapReportModel> _logger;
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
        public FaultsHeatMapReportModel(ILogger<FaultsHeatMapReportModel> logger, IStaffService staffService, IFaultService faultService, IFaultTypeService faultTypeService,
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
        //Declare CurrentStaff property, this is needed when calling the _staffService.
        public Staff CurrentStaff { get; set; }

        //Declare Faults property, this is needed for displaying faults on the map.
        [BindProperty]
        public List<Fault> Faults { get; set; }

        //Declare Staff property, this is needed for getting the staff assigned to each fault.
        [BindProperty]
        public List<Staff> Staff { get; set; }

        //Declare PagedFaults property, this is needed for displaying faults in the table.
        [BindProperty]
        public List<Fault> PagedFaults { get; set; }

        //Declare Reports property, this is needed for displaying the number of reports for each fault.
        [BindProperty]
        public List<Report> Reports { get; set; }

        //Declare FaultPriorities property, this is needed for displaying on map.
        [BindProperty]
        public List<FaultPriority> FaultPriorities { get; set; }

        //Declare FaultPriorityList property, this is needed for populating fault priority dropdown list.
        [BindProperty]
        public IEnumerable<SelectListItem> FaultPriorityList { get; set; }

        //Declare FaultStatuses property, this is needed for displaying on map.
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

        //Declare FaultTypeFilter, this is needed for filtering the faults.
        [DisplayName("Fault type")]
        [BindProperty]
        public int FaultTypeFilter { get; set; }

        //Declare FaultStatusFilter, this is needed for filtering the faults.
        [DisplayName("Fault status")]
        [BindProperty]
        public int FaultStatusFilter { get; set; }

        //Declare FaultPriorityFilter, this is needed for filtering the faults.
        [DisplayName("Fault priority")]
        [BindProperty]
        public int FaultPriorityFilter { get; set; }

        //Declare ShowFaultMap property, this is needed for displaying the fault map.
        [BindProperty]
        public bool ShowFaultMap { get; set; }

        //Declare ShowFaultTable property, this is needed for displaying the fault table.
        [BindProperty]
        public bool ShowFaultTable { get; set; }

        //Declare Pager property, this is needed for pagination.
        [BindProperty(SupportsGet = true)]
        public Pager Pager { get; set; } = new Pager();

        //Declare ShowMyFaults property, this is needed for displaying the current staffs faults.
        [BindProperty]
        public bool ShowMyFaults { get; set; }

        [BindProperty]
        public string? ReadWrite { get; set; }
        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is executed when the page loads.
        //When executed the FaultTypes, Faults, FaultPriorities, and FaultStatuses properties are populated.
        public async Task<IActionResult> OnGetAsync()
        {
            //The contexts current user exists.
            if (_httpContextAccessor.HttpContext.User != null)
            {
                //The contexts current user has been authenticated.
                if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true && HttpContext.User.IsInRole("StaffReadWrite") || HttpContext.User.IsInRole("StaffRead"))
                {
                    //Get the ID from the contexts current user, needed for populating CurrentUser property from DB.
                    string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

                    //Get the JWT token claim from the contexts current user, needed for populating CurrentUser property from DB.
                    Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

                    //Set the jwtToken string to the JWT token claims value, needed for populating CurrentUser property from DB.
                    string? jwtToken = jwtTokenClaim.Value;

                    //Set the CurrentStaff property by calling the GetUser method in the _userService.
                    CurrentStaff = await _staffService.GetStaff(Convert.ToInt32(userID), jwtToken);

                    //Clear session to ensure fresh start.
                    HttpContext.Session.Clear();

                    //Clear TempData to ensure fresh start.
                    TempData.Clear();

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

                    //Get all current faults by calling the GetFaults method from the _faultService.
                    Faults = await _faultService.GetFaults();

                    //Order the faults by priority.
                    Faults = Faults.OrderBy(f => f.FaultPriorityID).ToList();

                    //Show all faults apart from the ones with the status of repaired as default.
                    //Faults = Faults.Where(f => f.FaultStatusID != 4).ToList();

                    ////Show all faults apart from the ones assigned to the current staff member.
                    //Faults = Faults.Where(f => f.StaffID != CurrentStaff.ID).ToList();

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

                    //Populate fault statuses dropdown.
                    FaultStatusList = FaultStatuses.Select(fs => new SelectListItem()
                    {
                        Text = fs.FaultStatusDescription,
                        Value = fs.ID.ToString()
                    });

                    //Get all Reports by calling the GetReports method from the _reportService.
                    Reports = await _reportService.GetReports();

                    //Set the CurrentStaff property by calling the GetAllStaff method in the _staffService.
                    Staff = await _staffService.GetAllStaff(jwtToken);

                    //Set session data needed for the page.
                    HttpContext.Session.SetInSession("Faults", Faults);
                    HttpContext.Session.SetInSession("FaultTypes", FaultTypes);
                    HttpContext.Session.SetInSession("FaultPriorities", FaultPriorities);
                    HttpContext.Session.SetInSession("FaultStatuses", FaultStatuses);
                    HttpContext.Session.SetInSession("Reports", Reports);
                    HttpContext.Session.SetInSession("Staff", Staff);

                    //Return the page.
                    return Page();
                }
                //The contexts current user has not been authenticated.
                else
                {
                    //Redirect user to no permission.
                    return Redirect("/NoPermission");
                }
            }
            //The contexts current user does not exist.
            else
            {
                //Redirect user to no permission
                return Redirect("/NoPermission");
            }
        }
        #endregion Page Load
    }
}
