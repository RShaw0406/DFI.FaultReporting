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
        public User CurrentUser { get; set; }

        [BindProperty]
        public Contractor Contractor { get; set; }

        [BindProperty]
        public List<Contractor> Contractors { get; set; }

        [BindProperty]
        public List<Fault> Faults { get; set; }

        [BindProperty]
        public List<Repair> Repairs { get; set; }

        [BindProperty]
        public List<Repair> PagedRepairs { get; set; }

        [BindProperty]
        public List<RepairStatus> RepairStatuses { get; set; }

        [BindProperty]
        public IEnumerable<SelectListItem> RepairStatusList { get; set; }

        [DisplayName("Repair status")]
        [BindProperty]
        public int RepairStatusFilter { get; set; }

        [BindProperty]
        [DisplayName("Target met")]
        public int? TargetMetFilter { get; set; }

        [BindProperty]
        public List<FaultPriority> FaultPriorities { get; set; }

        [BindProperty]
        public List<FaultStatus> FaultStatuses { get; set; }

        [BindProperty]
        public List<FaultType> FaultTypes { get; set; }

        [BindProperty]
        public bool ShowRepairMap { get; set; }

        [BindProperty]
        public bool ShowRepairTable { get; set; }

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

                    //Clear session and tempdata to ensure fresh start.
                    HttpContext.Session.Clear();
                    TempData.Clear();

                    await PopulateProperties();

                    await SetSessionData();

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

        #region Data
        //Method Summary:
        //This method is excuted when the a post occurs.
        //When excuted, it populates the page properties.
        public async Task PopulateProperties()
        {
            //Get the current user ID and JWT token.
            string? userID = _httpContextAccessor.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
            string? jwtToken = jwtTokenClaim.Value;
            CurrentUser = await _userService.GetUser(Convert.ToInt32(userID), jwtToken);

            //Get all contractors from the DB and set in Contractors property.
            Contractors = await _contractorService.GetContractors(jwtToken);
            if (Contractors != null)
            {
                //Get the contractor details from the Contractors property by filtering on the CurrentUser property.
                Contractor = Contractors.Where(c => c.Email == CurrentUser.Email).FirstOrDefault();
            }

            Repairs = await _repairService.GetRepairs(jwtToken);
            Repairs = Repairs.OrderBy(r => r.RepairTargetDate).ToList();

            //Filter out repairs that have been repaired.
            Repairs = Repairs.Where(r => r.RepairStatusID != 3).ToList();

            //Filter out repairs that are not assigned to the current contractor.
            Repairs = Repairs.Where(r => r.ContractorID == Contractor.ID).ToList();

            RepairStatuses = await _repairStatusService.GetRepairStatuses(jwtToken);
            RepairStatuses = RepairStatuses.Where(rs => rs.Active == true).ToList();

            RepairStatusList = RepairStatuses.Select(rs => new SelectListItem()
            {
                Text = rs.RepairStatusDescription,
                Value = rs.ID.ToString()
            });

            Faults = await _faultService.GetFaults();

            FaultPriorities = await _faultPriorityService.GetFaultPriorities();
            FaultPriorities = FaultPriorities.Where(fp => fp.Active == true).ToList();

            FaultStatuses = await _faultStatusService.GetFaultStatuses();
            FaultStatuses = FaultStatuses.Where(fs => fs.Active == true).ToList();

            FaultTypes = await _faultTypeService.GetFaultTypes();
            FaultTypes = FaultTypes.Where(ft => ft.Active == true).ToList();
        }

        //Method Summary:
        //This method is executed when a post occurs.
        //When executed, the page properties required in the charts javascript are set in session storage.
        public async Task SetSessionData()
        {
            HttpContext.Session.SetInSession("Faults", Faults);
            HttpContext.Session.SetInSession("FaultTypes", FaultTypes);
            HttpContext.Session.SetInSession("FaultPriorities", FaultPriorities);
            HttpContext.Session.SetInSession("FaultStatuses", FaultStatuses);
            HttpContext.Session.SetInSession("Repairs", Repairs);
            HttpContext.Session.SetInSession("RepairStatuses", RepairStatuses);
            HttpContext.Session.SetInSession("Contractor", Contractor);
        }
        #endregion Data

        #region Map/Table Tile Controls
        //Method Summary:
        //This method is excuted when the "Map" link is clicked.
        //When executed the "Map" section is displayed.
        public async Task<IActionResult> OnGetShowMapView()
        {
            await PopulateProperties();

            //Hide the repair table.
            ShowRepairTable = false;

            //Show the repair map.
            ShowRepairMap = true;

            //Get repairs again as need to refresh the list for filtering in this case.
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
            string? jwtToken = jwtTokenClaim.Value;
            Repairs = await _repairService.GetRepairs(jwtToken);
            Repairs = Repairs.OrderBy(r => r.RepairTargetDate).ToList();

            //Filter out repairs that are not assigned to the current contractor.
            Repairs = Repairs.Where(r => r.ContractorID == Contractor.ID).ToList();

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

            //Store the ShowRepairMap and ShowRepairTable values in TempData.
            TempData["ShowRepairMap"] = ShowRepairMap;
            TempData["ShowRepairTable"] = ShowRepairTable;
            TempData.Keep();

            return Page();
        }

        //Method Summary:
        //This method is excuted when the "Table" link is clicked.
        //When executed the "Table" section is displayed.
        public async Task<IActionResult> OnGetShowTableView()
        {
            await PopulateProperties();

            //Hide the repair map.
            ShowRepairMap = false;

            //Show the repair table.
            ShowRepairTable = true;

            //Get repairs again as need to refresh the list for filtering in this case.
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
            string? jwtToken = jwtTokenClaim.Value;
            Repairs = await _repairService.GetRepairs(jwtToken);
            Repairs = Repairs.OrderBy(r => r.RepairTargetDate).ToList();

            //Filter out repairs that are not assigned to the current contractor.
            Repairs = Repairs.Where(r => r.ContractorID == Contractor.ID).ToList();

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

            //Store the ShowRepairMap and ShowRepairTable values in TempData.
            TempData["ShowRepairMap"] = ShowRepairMap;
            TempData["ShowRepairTable"] = ShowRepairTable;
            TempData.Keep();

            //Setup Pager.
            Pager.CurrentPage = 1;
            Pager.Count = Repairs.Count;
            Pager.PageSize = await _settingsService.GetSettingInt(DFI.FaultReporting.Common.Constants.Settings.PAGESIZE);
            PagedRepairs = await _pagerService.GetPaginatedRepairs(Repairs, Pager.CurrentPage, Pager.PageSize);
            PagedRepairs = PagedRepairs.OrderBy(r => r.RepairTargetDate).ToList();

            return Page();
        }
        #endregion Map/Table Tile Controls

        #region Dropdown Filter Change
        //This method is executed when either of the dropdown filters are changed.
        //When executed the Faults property is filtered based on the selected filter.
        public async Task<IActionResult> OnPostFilter()
        {
            await PopulateProperties();

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

            //Get repairs again as need to refresh the list for filtering in this case.
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
            string? jwtToken = jwtTokenClaim.Value;
            Repairs = await _repairService.GetRepairs(jwtToken);

            //-------------------- STATUS FILTER --------------------
            //User has selected a status that is not "All".
            if (RepairStatusFilter != 0)
            {
                //User has selected to view repairs that have been repaired.
                if (RepairStatusFilter == 3)
                {
                    Repairs = Repairs.Where(r => r.RepairStatusID == 3).ToList();
                }
                else
                {
                    Repairs = Repairs.Where(r => r.RepairStatusID == RepairStatusFilter).ToList();
                }
            }
            else
            {
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

            //Order the repairs by RepairTargetDate.
            Repairs = Repairs.OrderBy(r => r.RepairTargetDate).ToList();

            //The repair table is displayed.
            if (ShowRepairTable)
            {
                //Setup Pager.
                Pager.CurrentPage = 1;
                Pager.Count = Repairs.Count;
                Pager.PageSize = await _settingsService.GetSettingInt(DFI.FaultReporting.Common.Constants.Settings.PAGESIZE);
                PagedRepairs = await _pagerService.GetPaginatedRepairs(Repairs, Pager.CurrentPage, Pager.PageSize);
                PagedRepairs = PagedRepairs.OrderBy(r => r.RepairTargetDate).ToList();
            }

            await SetSessionData();

            //Set the selected repair status in TempData.
            TempData["RepairStatusFilter"] = RepairStatusFilter;
            TempData["TargetMetFilter"] = TargetMetFilter;
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
        public async Task OnGetPaging()
        {
            await PopulateProperties();

            //Get repairs again as need to refresh the list for filtering in this case.
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
            string? jwtToken = jwtTokenClaim.Value;
            Repairs = await _repairService.GetRepairs(jwtToken);
            Repairs = Repairs.OrderBy(r => r.RepairTargetDate).ToList();

            //Filter out repairs that are not assigned to the current contractor.
            Repairs = Repairs.Where(r => r.ContractorID == Contractor.ID).ToList();

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

            //Keep the TempData.
            TempData.Keep();

            //Hide the repair map.
            ShowRepairMap = false;

            //Show the repair table.
            ShowRepairTable = true;

            //Setup Pager.
            Pager.Count = Repairs.Count;
            Pager.PageSize = await _settingsService.GetSettingInt(DFI.FaultReporting.Common.Constants.Settings.PAGESIZE);
            PagedRepairs = await _pagerService.GetPaginatedRepairs(Repairs, Pager.CurrentPage, Pager.PageSize);
            PagedRepairs = PagedRepairs.OrderBy(r => r.RepairTargetDate).ToList();
        }
        #endregion Pagination
    }
}
