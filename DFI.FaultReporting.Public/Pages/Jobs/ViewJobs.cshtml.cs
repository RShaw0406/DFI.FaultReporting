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
using DFI.FaultReporting.Services.Users;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.Security.Claims;

namespace DFI.FaultReporting.Public.Pages.Jobs
{
    public class ViewJobsModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<ViewJobsModel> _logger;
        private readonly IFaultService _faultService;
        private readonly IFaultPriorityService _faultPriorityService;
        private readonly IFaultStatusService _faultStatusService;
        private readonly IFaultTypeService _faultTypeService;
        private readonly IUserService _userService;
        private readonly IContractorService _contractorService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPagerService _pagerService;
        private readonly ISettingsService _settingsService;
        private readonly IRepairService _repairService;
        private readonly IRepairStatusService _repairStatusService;

        //Inject dependencies in constructor.
        public ViewJobsModel(ILogger<ViewJobsModel> logger, IFaultService faultService, IFaultTypeService faultTypeService,
            IFaultPriorityService faultPriorityService, IFaultStatusService faultStatusService, IUserService userService, IContractorService contractorService,
            IHttpContextAccessor httpContextAccessor, IPagerService pagerService, ISettingsService settingsService, IRepairService repairService, IRepairStatusService repairStatusService)
        {
            _logger = logger;
            _faultService = faultService;
            _faultPriorityService = faultPriorityService;
            _faultStatusService = faultStatusService;
            _faultTypeService = faultTypeService;
            _httpContextAccessor = httpContextAccessor;
            _pagerService = pagerService;
            _settingsService = settingsService;
            _repairService = repairService;
            _userService = userService;
            _contractorService = contractorService;
            _repairStatusService = repairStatusService;
        }
        #endregion Dependency Injection

        #region Properties
        //Declare CurrentUser property, this is needed when calling the _userService.
        public User CurrentUser { get; set; }

        //Declare Contractor property, this is needed for getting contractor details.
        [BindProperty]
        public Contractor Contractor { get; set; }

        //Declare Contractors property, this is needed for getting all contractors.
        [BindProperty]
        public List<Contractor> Contractors { get; set; }

        //Declare Faults property, this is needed for getting all faults.
        [BindProperty]
        public List<Fault> Faults { get; set; }

        //Declare Repairs property, this is needed for getting repairs assigned to contractor.
        [BindProperty]
        public List<Repair> Repairs { get; set; }

        //Declare PagedRepairs property, this is needed for displaying repairs in the table.
        [BindProperty]
        public List<Repair> PagedRepairs { get; set; }

        //Declare RepairStatuses property, this is needed for populating the dropdown filter.
        [BindProperty]
        public List<RepairStatus> RepairStatuses { get; set; }

        //Declare RepairStatusList property, this is needed for populating the dropdown filter.
        [BindProperty]
        public IEnumerable<SelectListItem> RepairStatusList { get; set; }

        //Declare RepairStatusFilter, this is needed for filtering the repairs.
        [DisplayName("Repair status")]
        [BindProperty]
        public int RepairStatusFilter { get; set; }

        //Declare FaultPriorities property, this is needed for getting fault priorities from the DB.
        [BindProperty]
        public List<FaultPriority> FaultPriorities { get; set; }

        //Declare FaultStatuses property, this is needed for getting fault statuses from the DB.
        [BindProperty]
        public List<FaultStatus> FaultStatuses { get; set; }

        //Declare FaultTypes property, this is needed for getting fault types from the DB.
        [BindProperty]
        public List<FaultType> FaultTypes { get; set; }

        //Declare ShowRepairMap property, this is needed for displaying the repairs map.
        [BindProperty]
        public bool ShowRepairMap { get; set; }

        //Declare ShowFaultTable property, this is needed for displaying the repairs table.
        [BindProperty]
        public bool ShowRepairTable { get; set; }

        //Declare Pager property, this is needed for pagination.
        [BindProperty(SupportsGet = true)]
        public Pager Pager { get; set; } = new Pager();
        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is executed when the page loads.
        //When executed the user is checked for authentication and role, the required data is retrieved from the DB and set in session.
        public async Task<IActionResult> OnGetAsync()
        {
            //The contexts current user exists.
            if (_httpContextAccessor.HttpContext.User != null)
            {
                //The contexts current user has been authenticated.
                if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true && HttpContext.User.IsInRole("Contractor"))
                {
                    //Show the repair map by default.
                    ShowRepairMap = true;

                    //Get the ID from the contexts current user, needed for populating CurrentUser property from DB.
                    string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

                    //Get the JWT token claim from the contexts current user, needed for populating CurrentUser property from DB.
                    Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

                    //Set the jwtToken string to the JWT token claims value, needed for populating CurrentUser property from DB.
                    string? jwtToken = jwtTokenClaim.Value;

                    //Set the CurrentUser property by calling the GetUser method in the _userService.
                    CurrentUser = await _userService.GetUser(Convert.ToInt32(userID), jwtToken);

                    //Clear session to ensure fresh start.
                    HttpContext.Session.Clear();

                    //Clear TempData to ensure fresh start.
                    TempData.Clear();

                    //Get all contractors from the DB and set in Contractors property.
                    Contractors = await _contractorService.GetContractors(jwtToken);
                    if (Contractors != null)
                    {
                        //Get the contractor details from the Contractors property by filtering on the CurrentUser property.
                        Contractor = Contractors.Where(c => c.Email == CurrentUser.Email).FirstOrDefault();
                    }

                    //Get all repairs from the DB and set in Repairs property.
                    Repairs = await _repairService.GetRepairs(jwtToken);

                    //Filter out repairs that are not assigned to the current contractor.
                    Repairs = Repairs.Where(r => r.ContractorID == Contractor.ID).ToList();

                    //Filter out repairs that have been repaired.
                    Repairs = Repairs.Where(r => r.RepairStatusID != 3).ToList();

                    //Order the repairs by RepairTargetDate.
                    Repairs = Repairs.OrderBy(r => r.RepairTargetDate).ToList();

                    //Get all repair statuses from the DB and set in RepairStatuses property.
                    RepairStatuses = await _repairStatusService.GetRepairStatuses(jwtToken);

                    //Filter out inactive repair statuses.
                    RepairStatuses = RepairStatuses.Where(rs => rs.Active == true).ToList();

                    //Populate repair status dropdown.
                    RepairStatusList = RepairStatuses.Select(rs => new SelectListItem()
                    {
                        Text = rs.RepairStatusDescription,
                        Value = rs.ID.ToString()
                    });

                    //Get all faults from the DB and set in Faults property.
                    Faults = await _faultService.GetFaults();

                    //Get all fault priorities from the DB and set in FaultPriorities property.
                    FaultPriorities = await _faultPriorityService.GetFaultPriorities();

                    //Get all fault statuses from the DB and set in FaultStatuses property.
                    FaultStatuses = await _faultStatusService.GetFaultStatuses();

                    //Get all fault types from the DB and set in FaultTypes property.
                    FaultTypes = await _faultTypeService.GetFaultTypes();

                    //Set session data needed for the page.
                    HttpContext.Session.SetInSession("Faults", Faults);
                    HttpContext.Session.SetInSession("FaultTypes", FaultTypes);
                    HttpContext.Session.SetInSession("FaultPriorities", FaultPriorities);
                    HttpContext.Session.SetInSession("FaultStatuses", FaultStatuses);
                    HttpContext.Session.SetInSession("Repairs", Repairs);
                    HttpContext.Session.SetInSession("RepairStatuses", RepairStatuses);
                    HttpContext.Session.SetInSession("Contractor", Contractor);

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

        #region Map/Table Tile Controls
        //Method Summary:
        //This method is excuted when the "Map" link is clicked.
        //When executed the "Map" section is displayed.
        public async Task<IActionResult> OnGetShowMapView()
        {
            //Hide the repair table.
            ShowRepairTable = false;

            //Show the repair map.
            ShowRepairMap = true;

            //User has selected a repair status
            if (TempData["RepairStatusFilter"] != null)
            {
                //Get the RepairStatusFilter value from TempData.
                RepairStatusFilter = int.Parse(TempData["RepairStatusFilter"].ToString());
            }

            //Store the ShowRepairMap and ShowRepairTable values in TempData.
            TempData["ShowRepairMap"] = ShowRepairMap;
            TempData["ShowRepairTable"] = ShowRepairTable;
            TempData.Keep();

            //Get all required data from session.
            GetSessionData();

            //Return the page.
            return Page();
        }

        //Method Summary:
        //This method is excuted when the "Table" link is clicked.
        //When executed the "Table" section is displayed.
        public async Task<IActionResult> OnGetShowTableView()
        {
            //Set the current page to 1.
            Pager.CurrentPage = 1;

            //Set the page size to the value from the settings.
            Pager.PageSize = await _settingsService.GetSettingInt(DFI.FaultReporting.Common.Constants.Settings.PAGESIZE);

            //Hide the repair map.
            ShowRepairMap = false;

            //Show the repair table.
            ShowRepairTable = true;

            //User has selected a repair status
            if (TempData["RepairStatusFilter"] != null)
            {
                //Get the RepairStatusFilter value from TempData.
                RepairStatusFilter = int.Parse(TempData["RepairStatusFilter"].ToString());
            }

            //Store the ShowRepairMap and ShowRepairTable values in TempData.
            TempData["ShowRepairMap"] = ShowRepairMap;
            TempData["ShowRepairTable"] = ShowRepairTable;
            TempData.Keep();

            //Get all required data from session.
            GetSessionData();

            //Set the pager count to the number of repairs.
            Pager.Count = Repairs.Count;

            //Get the paginated repairs by calling the GetPaginatedRepairs method from the _pagerService.
            PagedRepairs = await _pagerService.GetPaginatedRepairs(Repairs, Pager.CurrentPage, Pager.PageSize);

            //Order the repairs by target date.
            PagedRepairs = PagedRepairs.OrderBy(r => r.RepairTargetDate).ToList();

            //Return the page.
            return Page();
        }
        #endregion Map/Table Tile Controls

        #region Dropdown Filter Change
        //This method is executed when either of the dropdown filters are changed.
        //When executed the Faults property is filtered based on the selected filter.
        public async Task<IActionResult> OnPostFilter()
        {
            //Get the ID from the contexts current user, needed for populating CurrentUser property from DB.
            string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            //Get the JWT token claim from the contexts current user, needed for populating CurrentUser property from DB.
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

            //Set the jwtToken string to the JWT token claims value, needed for populating CurrentUser property from DB.
            string? jwtToken = jwtTokenClaim.Value;

            //Set the CurrentUser property by calling the GetUser method in the _userService.
            CurrentUser = await _userService.GetUser(Convert.ToInt32(userID), jwtToken);

            //Get all required data from session.
            GetSessionData();

            //User has clicked the "Table" link.
            if (TempData["ShowRepairTable"] != null)
            {
                //Get the ShowRepairTable value from TempData.
                ShowRepairTable = Boolean.Parse(TempData["ShowRepairTable"].ToString());
            }

            //User has clicked the "Map" link or map is displayed by default.
            if (TempData["ShowRepairMap"] != null)
            {
                //Get the ShowRepairMap value from TempData.
                ShowRepairMap = Boolean.Parse(TempData["ShowRepairMap"].ToString());
            }
            //Map is displayed by default.
            else
            {
                //Show the repair map by default.
                ShowRepairMap = true;
            }


            //Get all repairs from the DB and set in Repairs property.
            Repairs = await _repairService.GetRepairs(jwtToken);

            //Filter out repairs that are not assigned to the current contractor.
            Repairs = Repairs.Where(r => r.ContractorID == Contractor.ID).ToList();

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
            //User has not selected to view repairs of all statuses
            else
            {
                //Filter out repairs that have been repaired.
                Repairs = Repairs.Where(r => r.RepairStatusID != 3).ToList();
            }

            //Order the repairs by RepairTargetDate.
            Repairs = Repairs.OrderBy(r => r.RepairTargetDate).ToList();

            //Set the Repairs in session, needed for displaying on map.
            HttpContext.Session.SetInSession("Repairs", Repairs);

            //Set the selected repair status in TempData.
            TempData["RepairStatusFilter"] = RepairStatusFilter;
            TempData.Keep();

            //User has selected the "Map" link.
            if (ShowRepairMap)
            {
                //Return the page and show the map, this is needed to ensure the correct section is displayed and the paging is reset.
                return Redirect("/Jobs/ViewJobs?handler=ShowMapView");
            }
            //User has selected the "Table" link.
            else
            {
                //Return the page and show the table, this is needed to ensure the correct section is displayed and the paging is reset.
                return Redirect("/Jobs/ViewJobs?handler=ShowTableView");
            }
        }
        #endregion Dropdown Filter Change

        #region Pagination
        //Method Summary:
        //This method is excuted when the pagination buttons are clicked.
        //When executed the desired page of faults is displayed.
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

            //Hide the repair map.
            ShowRepairMap = false;

            //Show the repair table.
            ShowRepairTable = true;

            //Get all required data from session.
            GetSessionData();

            //Set the pager count to the number of repairs.
            Pager.Count = Repairs.Count;

            //Get the paginated repairs by calling the GetPaginatedRepairs method from the _pagerService.
            PagedRepairs = await _pagerService.GetPaginatedRepairs(Repairs, Pager.CurrentPage, Pager.PageSize);

            //Order the repairs by target date.
            PagedRepairs = PagedRepairs.OrderBy(r => r.RepairTargetDate).ToList();
        }
        #endregion Pagination

        #region Session Data
        //Method Summary:
        //This method is excuted when the map/table tiles are clicked or when any of the pagination buttons are clicked.
        //When executed the required data is retrieved from session.
        public async void GetSessionData()
        {
            //Get the faults from session.
            Faults = HttpContext.Session.GetFromSession<List<Fault>>("Faults");

            //Order the faults by priority.
            Faults = Faults.OrderBy(f => f.FaultPriorityID).ToList();

            //Get the fault types from session.
            FaultTypes = HttpContext.Session.GetFromSession<List<FaultType>>("FaultTypes");

            //Get the fault priorities from session.
            FaultPriorities = HttpContext.Session.GetFromSession<List<FaultPriority>>("FaultPriorities");

            //Get the fault statuses from session.
            FaultStatuses = HttpContext.Session.GetFromSession<List<FaultStatus>>("FaultStatuses");

            //Get the repairs from session.
            Repairs = HttpContext.Session.GetFromSession<List<Repair>>("Repairs");

            //Get the repair statuses from session.
            RepairStatuses = HttpContext.Session.GetFromSession<List<RepairStatus>>("RepairStatuses");
            
            //Get the contractor from session.
            Contractor = HttpContext.Session.GetFromSession<Contractor>("Contractor");

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

            //Set the CurrentUser property by calling the GetUser method in the _userService.
            CurrentUser = await _userService.GetUser(Convert.ToInt32(userID), jwtToken);

            //Filter out repairs that are not assigned to the current contractor.
            Repairs = Repairs.Where(r => r.ContractorID == Contractor.ID).ToList();
        }
        #endregion Session Data
    }
}
