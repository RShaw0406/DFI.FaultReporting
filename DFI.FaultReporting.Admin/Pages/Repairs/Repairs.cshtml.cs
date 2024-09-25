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
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPagerService _pagerService;
        private readonly ISettingsService _settingsService;
        private readonly IRepairService _repairService;
        private readonly IRepairStatusService _repairStatusService;
        private readonly IContractorService _contractorService;

        //Inject dependencies in constructor.
        public RepairsModel(ILogger<RepairsModel> logger, IStaffService staffService, IFaultService faultService, IFaultTypeService faultTypeService,
            IFaultPriorityService faultPriorityService, IFaultStatusService faultStatusService, IHttpContextAccessor httpContextAccessor, 
            IPagerService pagerService, ISettingsService settingsService, IRepairService repairService, IRepairStatusService repairStatusService
            , IContractorService contractorService)
        {
            _logger = logger;
            _staffService = staffService;
            _faultService = faultService;
            _faultPriorityService = faultPriorityService;
            _faultStatusService = faultStatusService;
            _faultTypeService = faultTypeService;
            _httpContextAccessor = httpContextAccessor;
            _pagerService = pagerService;
            _settingsService = settingsService;
            _repairService = repairService;
            _repairStatusService = repairStatusService;
            _contractorService = contractorService;
        }
        #endregion Dependency Injection

        #region Properties
        public Staff CurrentStaff { get; set; }

        [BindProperty]
        public List<Fault> Faults { get; set; }

        [BindProperty]
        public List<Staff> Staff { get; set; }

        [BindProperty]
        public List<Repair> Repairs { get; set; }

        [BindProperty]
        public List<Repair> PagedRepairs { get; set; }

        [BindProperty]
        public Repair Repair { get; set; }

        [BindProperty]
        public List<RepairStatus> RepairStatuses { get; set; }

        [BindProperty]
        public IEnumerable<SelectListItem> RepairStatusList { get; set; }

        [BindProperty]
        [DisplayName("Repair status")]
        public int? RepairStatusFilter { get; set; }

        [BindProperty]
        [DisplayName("Target met")]
        public int? TargetMetFilter { get; set; }

        [DisplayName("Search for contractor")]
        [BindProperty]
        public string? SearchString { get; set; }

        [BindProperty]
        public int SearchID { get; set; }

        [BindProperty]
        public List<FaultPriority> FaultPriorities { get; set; }

        [BindProperty]
        public List<FaultStatus> FaultStatuses { get; set; }

        [BindProperty]
        public List<FaultType> FaultTypes { get; set; }

        [BindProperty]
        public List<Contractor> Contractors { get; set; }

        [BindProperty(SupportsGet = true)]
        public Pager Pager { get; set; } = new Pager();
        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is executed when the page loads.
        //When executed the user is checked for authentication and role, the required data is retrieved from the DB.
        public async Task<IActionResult> OnGetAsync()
        {
            //The contexts current user exists.
            if (_httpContextAccessor.HttpContext.User != null)
            {
                //The contexts current user has been authenticated.
                if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true && HttpContext.User.IsInRole("StaffReadWrite") || HttpContext.User.IsInRole("StaffRead"))
                {
                    //Clear session and temp data to ensure fresh start.
                    HttpContext.Session.Clear();
                    TempData.Clear();

                    await PopulateProperties();

                    await SetSessionData();

                    //Setup pager for table.
                    Pager.CurrentPage = 1;
                    Pager.PageSize = await _settingsService.GetSettingInt(DFI.FaultReporting.Common.Constants.Settings.PAGESIZE);
                    Pager.Count = Repairs.Count;
                    PagedRepairs = await _pagerService.GetPaginatedRepairs(Repairs, Pager.CurrentPage, Pager.PageSize);
                    PagedRepairs = PagedRepairs.OrderBy(r => r.RepairTargetDate).ToList();

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

        #region Filters
        //This method is executed when a user filters the repairs data.
        //When executed the Repairs property is filtered based on the selected filter.
        public async Task<IActionResult> OnPostFilter()
        {
            await PopulateProperties();

            //Get the JWT token.
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
            string? jwtToken = jwtTokenClaim.Value;

            //Repopulate Repairs as user may be selecting to view repairs that have been repaired. Repaired repairs are not shown by default.
            Repairs = await _repairService.GetRepairs(jwtToken);

            //-------------------- SEARCH FILTER --------------------
            if (SearchString != null && SearchString != string.Empty)
            {
                Repairs = Repairs.Where(r => r.ContractorID == SearchID).ToList();
            }

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

            //-------------------- TARGET MET FILTER --------------------
            //User has selected a target met status that is not "All".
            if (TargetMetFilter != 0)
            {
                //Target met.
                if (TargetMetFilter == 1)
                {
                    Repairs = Repairs.Where(r => r.RepairStatusID == 3).ToList();

                    Repairs = Repairs.Where(r => r.ActualRepairDate <= r.RepairTargetDate).ToList();
                }
                //Target not met.
                else if (TargetMetFilter == 2)
                {
                    Repairs = Repairs.Where(r => r.RepairStatusID == 3).ToList();

                    Repairs = Repairs.Where(r => r.ActualRepairDate > r.RepairTargetDate).ToList();
                }
            }

            //Setup pager for table.
            Pager.CurrentPage = 1;
            Pager.Count = Repairs.Count;
            Pager.PageSize = await _settingsService.GetSettingInt(DFI.FaultReporting.Common.Constants.Settings.PAGESIZE);
            PagedRepairs = await _pagerService.GetPaginatedRepairs(Repairs, Pager.CurrentPage, Pager.PageSize);
            PagedRepairs = PagedRepairs.OrderBy(r => r.RepairStatusID).ToList();

            //Set the search string, selected repair status and taregt met in TempData.
            TempData["RepairStatusFilter"] = RepairStatusFilter;
            TempData["TargetMetFilter"] = TargetMetFilter;
            TempData["SearchString"] = SearchString;
            TempData.Keep();

            return Page();
        }
        #endregion Filters

        #region Pagination
        //Method Summary:
        //This method is excuted when the pagination buttons are clicked.
        //When executed the desired page of repairs is displayed.
        public async Task OnGetPaging()
        {
            await PopulateProperties();

            //Get repairs again as need to refresh the list for filtering in this case.
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
            string? jwtToken = jwtTokenClaim.Value;
            Repairs = await _repairService.GetRepairs(jwtToken);
            Repairs = Repairs.OrderBy(r => r.RepairTargetDate).ToList();

            //User has selected a repair status.
            if (TempData["RepairStatusFilter"] != null && (int)TempData["RepairStatusFilter"] != 0)
            {
                RepairStatusFilter = int.Parse(TempData["RepairStatusFilter"].ToString());

                Repairs = Repairs.Where(r => r.RepairStatusID == RepairStatusFilter).ToList();
            }

            //User has selected a target met status.
            if (TempData["TargetMetFilter"] != null && (int)TempData["TargetMetFilter"] != 0)
            {
                TargetMetFilter = int.Parse(TempData["TargetMetFilter"].ToString());

                //Target met.
                if (TargetMetFilter == 1)
                {
                    Repairs = Repairs.Where(r => r.RepairStatusID == 3).ToList();

                    Repairs = Repairs.Where(r => r.ActualRepairDate <= r.RepairTargetDate).ToList();
                }
                //Target not met.
                else if (TargetMetFilter == 2)
                {
                    Repairs = Repairs.Where(r => r.RepairStatusID == 3).ToList();

                    Repairs = Repairs.Where(r => r.ActualRepairDate > r.RepairTargetDate).ToList();
                }
            }

            //User has entered a search string.
            if (TempData["SearchString"] != null && TempData["SearchString"].ToString() != string.Empty)
            {
                SearchString = TempData["SearchString"].ToString();

                Repairs = Repairs.Where(r => r.ContractorID == SearchID).ToList();
            }

            TempData.Keep();

            //Setup pager for table.
            Pager.Count = Repairs.Count;
            Pager.PageSize = await _settingsService.GetSettingInt(DFI.FaultReporting.Common.Constants.Settings.PAGESIZE);
            PagedRepairs = await _pagerService.GetPaginatedRepairs(Repairs, Pager.CurrentPage, Pager.PageSize);
            PagedRepairs = PagedRepairs.OrderBy(r => r.RepairStatusID).ToList();
        }
        #endregion Pagination

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

            RepairStatuses = await _repairStatusService.GetRepairStatuses(jwtToken);
            RepairStatuses = RepairStatuses.Where(rs => rs.Active == true).ToList();

            //Populate the repair status list, this is needed for the repair status dropdown options.
            RepairStatusList = RepairStatuses.Select(rs => new SelectListItem()
            {
                Text = rs.RepairStatusDescription,
                Value = rs.ID.ToString()
            });

            Contractors = await _contractorService.GetContractors(jwtToken);
            Contractors = Contractors.Where(c => c.Active == true).ToList();

            Faults = await _faultService.GetFaults();

            FaultTypes = await _faultTypeService.GetFaultTypes();
            FaultTypes = FaultTypes.Where(ft => ft.Active == true).ToList();

            FaultPriorities = await _faultPriorityService.GetFaultPriorities();
            FaultPriorities = FaultPriorities.Where(fp => fp.Active == true).ToList();

            FaultStatuses = await _faultStatusService.GetFaultStatuses();
            FaultStatuses = FaultStatuses.Where(fs => fs.Active == true).ToList();

            Staff = await _staffService.GetAllStaff(jwtToken);
            Staff = Staff.Where(s => s.Active == true).ToList();

            Repairs = await _repairService.GetRepairs(jwtToken);

            //Show all repairs apart from the ones with the status of repaired as default.
            Repairs = Repairs.Where(r => r.RepairStatusID != 3).ToList();
        }

        //Method Summary:
        //This method is executed when a post occurs.
        //When executed, the page properties required are set in session storage.
        public async Task SetSessionData()
        {
            HttpContext.Session.SetInSession("Contractors", Contractors);
        }
        #endregion Data
    }
}
