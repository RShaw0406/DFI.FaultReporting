using DFI.FaultReporting.Common.Pagination;
using DFI.FaultReporting.Interfaces.Admin;
using DFI.FaultReporting.Interfaces.FaultReports;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Models.FaultReports;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Admin;
using DFI.FaultReporting.Services.Interfaces.Pagination;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Users;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.Security.Claims;
using DFI.FaultReporting.Common.SessionStorage;

namespace DFI.FaultReporting.Admin.Pages.Repairs
{
    public class RepairsModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<RepairsModel> _logger;
        private readonly IStaffService _staffService;
        private readonly IFaultService _faultService;
        private readonly IFaultPriorityService _faultPriorityService;
        private readonly IFaultStatusService _faultStatusService;
        private readonly IFaultTypeService _faultTypeService;
        private readonly IReportService _reportService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPagerService _pagerService;
        private readonly ISettingsService _settingsService;
        private readonly IRepairService _repairService;
        private readonly IRepairStatusService _repairStatusService;
        private readonly IContractorService _contractorService;

        //Inject dependencies in constructor.
        public RepairsModel(ILogger<RepairsModel> logger, IStaffService staffService, IFaultService faultService, IFaultTypeService faultTypeService,
            IFaultPriorityService faultPriorityService, IFaultStatusService faultStatusService, IReportService reportService,
            IHttpContextAccessor httpContextAccessor, IPagerService pagerService, ISettingsService settingsService, IRepairService repairService, IRepairStatusService repairStatusService
            , IContractorService contractorService)
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
            _repairService = repairService;
            _repairStatusService = repairStatusService;
            _contractorService = contractorService;
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

        //Declare Repairs property, this is needed for getting repairs for each fault.
        [BindProperty]
        public List<Repair> Repairs { get; set; }

        //Declare PagedRepairs property, this is needed for displaying repairs in the table.
        [BindProperty]
        public List<Repair> PagedRepairs { get; set; }

        //Declare Repair property, this is needed for getting the repair for each fault.
        [BindProperty]
        public Repair Repair { get; set; }

        //Declare RepairStatuses property, this is needed for populating repair status dropdown list.
        [BindProperty]
        public List<RepairStatus> RepairStatuses { get; set; }

        //Declare RepairStatusesList property, this is needed for populating repair status dropdown list.
        [BindProperty]
        public IEnumerable<SelectListItem> RepairStatusList { get; set; }

        //Declare RepairStatusFilter property, this is needed for filtering repairs by repair status.
        [BindProperty]
        [DisplayName("Repair status")]
        public int? RepairStatusFilter { get; set; }

        //Declare FaultPriorities property, this is needed for getting the fault priorities for each fault.
        [BindProperty]
        public List<FaultPriority> FaultPriorities { get; set; }

        //Declare FaultStatuses property, this is needed for getting the fault statuses for each fault.
        [BindProperty]
        public List<FaultStatus> FaultStatuses { get; set; }

        //Declare FaultTypes property, this is needed for getting the fault types for each fault.
        [BindProperty]
        public List<FaultType> FaultTypes { get; set; }

        //Declare Contractors property, this is needed for getting the contractors assigned to each repair.
        [BindProperty]
        public List<Contractor> Contractors { get; set; }

        //Declare Pager property, this is needed for pagination.
        [BindProperty(SupportsGet = true)]
        public Pager Pager { get; set; } = new Pager();
        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is executed when the page loads.
        //When executed the user is checked for authentication and role, the required data is retrieved from the DB and session.
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

                    //Get repair statuses for dropdown.
                    RepairStatuses = await _repairStatusService.GetRepairStatuses(jwtToken);

                    //Filter out inactive repair statuses.
                    RepairStatuses = RepairStatuses.Where(rs => rs.Active == true).ToList();

                    //Populate repair status dropdown.
                    RepairStatusList = RepairStatuses.Select(rs => new SelectListItem()
                    {
                        Text = rs.RepairStatusDescription,
                        Value = rs.ID.ToString()
                    });

                    //Get all contractors by calling the GetContractors method from the _contractorService.
                    Contractors = await _contractorService.GetContractors(jwtToken);

                    //Get all current faults by calling the GetFaults method from the _faultService.
                    Faults = await _faultService.GetFaults();

                    //Get all fault types by calling the GetFaultTypes method from the _faultTypeService.
                    FaultTypes = await _faultTypeService.GetFaultTypes();

                    //Get all fault priorities by calling the GetFaultPriorities method from the _faultPriorityService.
                    FaultPriorities = await _faultPriorityService.GetFaultPriorities();

                    //Filter out inactive fault priorities.
                    FaultPriorities = FaultPriorities.Where(fp => fp.Active == true).ToList();

                    //Get all fault statuses by calling the GetFaultStatuses method from the _faultStatusService.
                    FaultStatuses = await _faultStatusService.GetFaultStatuses();

                    //Filter out inactive fault statuses.
                    FaultStatuses = FaultStatuses.Where(fs => fs.Active == true).ToList();

                    //Set the CurrentStaff property by calling the GetAllStaff method in the _staffService.
                    Staff = await _staffService.GetAllStaff(jwtToken);

                    //Get all repairs by calling the GetRepairs method from the _repairService.
                    Repairs = await _repairService.GetRepairs(jwtToken);

                    //Show all repairs apart from the ones with the status of repaired as default.
                    Repairs = Repairs.Where(r => r.RepairStatusID != 3).ToList();

                    //Set the current page to 1.
                    Pager.CurrentPage = 1;

                    //Set the page size to the value from the settings.
                    Pager.PageSize = await _settingsService.GetSettingInt(DFI.FaultReporting.Common.Constants.Settings.PAGESIZE);

                    //Set the pager count to the number of repairs.
                    Pager.Count = Repairs.Count;

                    //Get the paginated repairs by calling the GetPaginatedRepairs method from the _pagerService.
                    PagedRepairs = await _pagerService.GetPaginatedRepairs(Repairs, Pager.CurrentPage, Pager.PageSize);

                    //Order the repairs by priority.
                    PagedRepairs = PagedRepairs.OrderBy(r => r.RepairTargetDate).ToList();

                    //Set session data needed for the page.
                    HttpContext.Session.SetInSession("Faults", Faults);
                    HttpContext.Session.SetInSession("FaultTypes", FaultTypes);
                    HttpContext.Session.SetInSession("FaultPriorities", FaultPriorities);
                    HttpContext.Session.SetInSession("FaultStatuses", FaultStatuses);
                    HttpContext.Session.SetInSession("Contractors", Contractors);
                    HttpContext.Session.SetInSession("Staff", Staff);
                    HttpContext.Session.SetInSession("Repairs", Repairs);
                    HttpContext.Session.SetInSession("RepairStatuses", RepairStatuses);

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

        #region Dropdown Filter Change
        //This method is executed when either of the dropdown filters are changed.
        //When executed the Repairs property is filtered based on the selected filter.
        public async Task<IActionResult> OnPostFilter()
        {
            //Get the ID from the contexts current user, needed for populating CurrentUser property from DB.
            string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            //Get the JWT token claim from the contexts current user, needed for populating CurrentUser property from DB.
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

            //Set the jwtToken string to the JWT token claims value, needed for populating CurrentUser property from DB.
            string? jwtToken = jwtTokenClaim.Value;

            //Set the CurrentStaff property by calling the GetUser method in the _userService.
            CurrentStaff = await _staffService.GetStaff(Convert.ToInt32(userID), jwtToken);

            //Get all required data from session.
            GetSessionData();

            //Get all repairs by calling the GetRepairs method from the _repairService.
            Repairs = await _repairService.GetRepairs(jwtToken);

            //-------------------- STATUS FILTER --------------------
            //User has selected a status that is not "All".
            if (RepairStatusFilter != 0)
            {

                //User has selected to view repairs that have been repaired.
                if (RepairStatusFilter == 3)
                {
                    //Show all repairs which have been repaired.
                    Repairs = Repairs.Where(r => r.RepairStatusID == 3).ToList();
                }
                //User has selected a status that is not "All" and not "Repaired".
                else
                {
                    //Get the repairs for the selected status.
                    Repairs = Repairs.Where(r => r.RepairStatusID == RepairStatusFilter).ToList();
                }
            }
            //User has not selected to view faults of all statuses
            else
            {
                //Show all repairs apart from the ones with the status of repaired as default.
                Repairs = Repairs.Where(r => r.RepairStatusID != 3).ToList();
            }

            //Order the repairs by target date.
            Repairs = Repairs.OrderBy(r => r.RepairTargetDate).ToList();

            //Set the current page to 1.
            Pager.CurrentPage = 1;

            //Set the page size to the value from the settings.
            Pager.PageSize = await _settingsService.GetSettingInt(DFI.FaultReporting.Common.Constants.Settings.PAGESIZE);

            //Set the pager count to the number of repairs.
            Pager.Count = Repairs.Count;

            //Get the paginated repairs by calling the GetPaginatedRepairs method from the _pagerService.
            PagedRepairs = await _pagerService.GetPaginatedRepairs(Repairs, Pager.CurrentPage, Pager.PageSize);

            //Order the repairs by priority.
            PagedRepairs = PagedRepairs.OrderBy(r => r.RepairTargetDate).ToList();

            //Set the Repairs in session.
            HttpContext.Session.SetInSession("Repairs", Repairs);

            //Set the selected repair status in TempData.
            TempData["RepairStatusFilter"] = RepairStatusFilter;
            TempData.Keep();

            //Return the page.
            return Page();
        }
        #endregion Dropdown Filter Change

        #region Session Data
        //Method Summary:
        //This method is excuted when the map/table tiles are clicked or when any of the pagination buttons are clicked.
        //When executed the required data is retrieved from session.
        public async void GetSessionData()
        {
            //Get the faults from session.
            Faults = HttpContext.Session.GetFromSession<List<Fault>>("Faults");

            //Get the fault types from session.
            FaultTypes = HttpContext.Session.GetFromSession<List<FaultType>>("FaultTypes");

            //Get the fault priorities from session.
            FaultPriorities = HttpContext.Session.GetFromSession<List<FaultPriority>>("FaultPriorities");

            //Get the fault statuses from session.
            FaultStatuses = HttpContext.Session.GetFromSession<List<FaultStatus>>("FaultStatuses");

            //Get the contractors from session.
            Contractors = HttpContext.Session.GetFromSession<List<Contractor>>("Contractors");

            //Get the staff from session.
            Staff = HttpContext.Session.GetFromSession<List<Staff>>("Staff");

            //Get the repairs from session.
            Repairs = HttpContext.Session.GetFromSession<List<Repair>>("Repairs");

            //Get the repair statuses from session.
            RepairStatuses = HttpContext.Session.GetFromSession<List<RepairStatus>>("RepairStatuses");

            //Populate repair status dropdown.
            RepairStatusList = RepairStatuses.Select(rs => new SelectListItem()
            {
                Text = rs.RepairStatusDescription,
                Value = rs.ID.ToString()
            });

            //Get the ID from the contexts current user, needed for populating CurrentUser property from DB.
            string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            //Get the JWT token claim from the contexts current user, needed for populating CurrentUser property from DB.
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

            //Set the jwtToken string to the JWT token claims value, needed for populating CurrentUser property from DB.
            string? jwtToken = jwtTokenClaim.Value;

            //Set the CurrentStaff property by calling the GetUser method in the _userService.
            CurrentStaff = await _staffService.GetStaff(Convert.ToInt32(userID), jwtToken);
        }
        #endregion Session Data

        #region Pagination
        //Method Summary:
        //This method is excuted when the pagination buttons are clicked.
        //When executed the desired page of repairs is displayed.
        public async void OnGetPaging()
        {
            //User has selected a repair status.
            if (TempData["RepairStatusFilter"] != null)
            {
                //Get the RepairStatusFilter value from TempData.
                RepairStatusFilter = int.Parse(TempData["RepairStatusFilter"].ToString());
            }

            //Keep the TempData.
            TempData.Keep();

            //Get all required data from session.
            GetSessionData();

            //Set the pager count to the number of repairs.
            Pager.Count = Repairs.Count;

            //Get the paginated repairs by calling the GetPaginatedRepairs method from the _pagerService.
            PagedRepairs = await _pagerService.GetPaginatedRepairs(Repairs, Pager.CurrentPage, Pager.PageSize);

            //Order the repairs by priority.
            PagedRepairs = PagedRepairs.OrderBy(r => r.RepairTargetDate).ToList();
        }
        #endregion Pagination
    }
}
