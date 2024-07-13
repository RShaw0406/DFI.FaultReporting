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
using DocumentFormat.OpenXml.Drawing.Diagrams;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.Security.Claims;

namespace DFI.FaultReporting.Admin.Pages.Faults
{
    public class FaultsModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<FaultsModel> _logger;
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

        //Inject dependencies in constructor.
        public FaultsModel(ILogger<FaultsModel> logger, IStaffService staffService, IFaultService faultService, IFaultTypeService faultTypeService,
            IFaultPriorityService faultPriorityService, IFaultStatusService faultStatusService, IReportService reportService,
            IHttpContextAccessor httpContextAccessor, IPagerService pagerService, ISettingsService settingsService, IRepairService repairService)
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
        }
        #endregion Dependency Injection

        #region Properties
        public Staff CurrentStaff { get; set; }

        [BindProperty]
        public List<Fault> Faults { get; set; }

        [BindProperty]
        public List<Staff> Staff { get; set; }

        [BindProperty]
        public List<Fault> PagedFaults { get; set; }

        [BindProperty]
        public List<Report> Reports { get; set; }

        [BindProperty]
        public List<Repair> Repairs { get; set; }

        [BindProperty]
        public Repair Repair { get; set; }

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

        [DisplayName("Fault type")]
        [BindProperty]
        public int FaultTypeFilter { get; set; }

        [DisplayName("Fault status")]
        [BindProperty]
        public int FaultStatusFilter { get; set; }

        [DisplayName("Fault priority")]
        [BindProperty]
        public int FaultPriorityFilter { get; set; }

        [BindProperty]
        public bool ShowFaultMap { get; set; }

        [BindProperty]
        public bool ShowFaultTable { get; set; }

        [BindProperty(SupportsGet = true)]
        public Pager Pager { get; set; } = new Pager();

        [BindProperty]
        public bool ShowMyFaults { get; set; }

        [BindProperty]
        public string? ReadWrite { get; set; }
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
                    //User is in the StaffReadWrite role.
                    if (HttpContext.User.IsInRole("StaffReadWrite"))
                    {
                        //Set ReadWrite to true, this is needed for showing buttons on map popups.
                        ReadWrite = "true";
                    }
                    //User is in the StaffRead role.
                    else
                    {
                        //Set ReadWrite to true, this is needed for hiding buttons on map popups.
                        ReadWrite = "false";
                    }

                    //Show the fault map by default.
                    ShowFaultMap = true;

                    //Show faults not assigned to current staff by default.
                    ShowMyFaults = false; 
                    
                    //Store the ShowMyFaults value in TempData.
                    TempData["ShowMyFaults"] = ShowMyFaults;
                    TempData.Keep();

                    //Clear session and temp data to ensure fresh start.
                    HttpContext.Session.Clear();
                    TempData.Clear();

                    await PopulateProperties();

                    await SetSessionData();

                    //Show all faults apart from the ones assigned to the current staff member.
                    Faults = Faults.Where(f => f.StaffID != CurrentStaff.ID).ToList();

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
            CurrentStaff = await _staffService.GetStaff(Convert.ToInt32(userID), jwtToken);

            FaultTypes = await _faultTypeService.GetFaultTypes();
            FaultTypes = FaultTypes.Where(ft => ft.Active == true).ToList();

            FaultTypesList = FaultTypes.Select(ft => new SelectListItem()
            {
                Text = ft.FaultTypeDescription,
                Value = ft.ID.ToString()
            });

            Faults = await _faultService.GetFaults();
            Faults = Faults.OrderBy(f => f.FaultPriorityID).ToList();
            //Show all faults apart from the ones with the status of repaired as default.
            Faults = Faults.Where(f => f.FaultStatusID != 4).ToList();

            FaultPriorities = await _faultPriorityService.GetFaultPriorities();
            FaultPriorities = FaultPriorities.Where(fp => fp.Active == true).ToList();

            FaultPriorityList = FaultPriorities.Select(fp => new SelectListItem()
            {
                Text = fp.FaultPriorityRating,
                Value = fp.ID.ToString()
            });

            FaultStatuses = await _faultStatusService.GetFaultStatuses();
            FaultStatuses = FaultStatuses.Where(fs => fs.Active == true).ToList();

            FaultStatusList = FaultStatuses.Select(fs => new SelectListItem()
            {
                Text = fs.FaultStatusDescription,
                Value = fs.ID.ToString()
            });

            Reports = await _reportService.GetReports();

            Staff = await _staffService.GetAllStaff(jwtToken);

            Repairs = await _repairService.GetRepairs(jwtToken);
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
            HttpContext.Session.SetInSession("Reports", Reports);
            HttpContext.Session.SetInSession("Staff", Staff);
            HttpContext.Session.SetInSession("Repairs", Repairs);
        }
        #endregion Data

        #region Map/Table Tile Controls
        //Method Summary:
        //This method is excuted when the "Map" link is clicked.
        //When executed the "Map" section is displayed.
        public async Task<IActionResult> OnGetShowMapView()
        {
            await PopulateProperties();

            //Show all faults apart from the ones assigned to the current staff member.
            Faults = Faults.Where(f => f.StaffID != CurrentStaff.ID).ToList();

            //Hide the fault table.
            ShowFaultTable = false;

            //Show the fault map.
            ShowFaultMap = true;

            //User has clicked "My Faults" button.
            if (TempData["ShowMyFaults"] != null)
            {
                ShowMyFaults = Boolean.Parse(TempData["ShowMyFaults"].ToString());

                if (ShowMyFaults)
                {
                    Faults = await _faultService.GetFaults();
                    Faults = Faults.Where(f => f.StaffID == CurrentStaff.ID).ToList();
                }
            }

            //User has selected a fault type.
            if (TempData["FaultTypeFilter"] != null && (int)TempData["FaultTypeFilter"] != 0)
            {
                FaultTypeFilter = int.Parse(TempData["FaultTypeFilter"].ToString());

                Faults = Faults.Where(f => f.FaultTypeID == FaultTypeFilter).ToList();
            }

            //User has selected a fault status.
            if (TempData["FaultStatusFilter"] != null && (int)TempData["FaultStatusFilter"] != 0)
            {
                FaultStatusFilter = int.Parse(TempData["FaultStatusFilter"].ToString());

                Faults = Faults.Where(f => f.FaultStatusID == FaultStatusFilter).ToList();
            }

            if ((TempData["FaultStatusFilter"] == null || (int)TempData["FaultStatusFilter"] == 0))
            {
                Faults = Faults.Where(f => f.FaultStatusID != 4).ToList();
            }

            //User has selected a fault priority.
            if (TempData["FaultPriorityFilter"] != null && (int)TempData["FaultPriorityFilter"] != 0)
            {
                FaultPriorityFilter = int.Parse(TempData["FaultPriorityFilter"].ToString());

                Faults = Faults.Where(f => f.FaultPriorityID == FaultPriorityFilter).ToList();
            }

            //Store the ShowFaultMap and ShowFaultTable values in TempData.
            TempData["ShowFaultMap"] = ShowFaultMap;
            TempData["ShowFaultTable"] = ShowFaultTable;
            TempData.Keep();

            //User is in the StaffReadWrite role.
            if (HttpContext.User.IsInRole("StaffReadWrite"))
            {
                //Set ReadWrite to true, this is needed for showing buttons on map popups.
                ReadWrite = "true";
            }
            //User is in the StaffRead role.
            else
            {
                //Set ReadWrite to true, this is needed for hiding buttons on map popups.
                ReadWrite = "false";
            }

            //Return the page.
            return Page();
        }

        //Method Summary:
        //This method is excuted when the "Table" link is clicked.
        //When executed the "Table" section is displayed.
        public async Task<IActionResult> OnGetShowTableView()
        {
            await PopulateProperties();

            //Show all faults apart from the ones assigned to the current staff member.
            Faults = Faults.Where(f => f.StaffID != CurrentStaff.ID).ToList();

            //Hide the fault map.
            ShowFaultMap = false;

            //Show the fault table.
            ShowFaultTable = true;

            //User has clicked "My Faults" button.
            if (TempData["ShowMyFaults"] != null)
            {
                ShowMyFaults = Boolean.Parse(TempData["ShowMyFaults"].ToString());

                if (ShowMyFaults)
                {
                    Faults = await _faultService.GetFaults();
                    Faults = Faults.Where(f => f.StaffID == CurrentStaff.ID).ToList();
                }
            }

            //User has selected a fault type.
            if (TempData["FaultTypeFilter"] != null && (int)TempData["FaultTypeFilter"] != 0)
            {
                FaultTypeFilter = int.Parse(TempData["FaultTypeFilter"].ToString());

                Faults = Faults.Where(f => f.FaultTypeID == FaultTypeFilter).ToList();
            }

            //User has selected a fault status.
            if (TempData["FaultStatusFilter"] != null && (int)TempData["FaultStatusFilter"] != 0)
            {
                FaultStatusFilter = int.Parse(TempData["FaultStatusFilter"].ToString());

                Faults = Faults.Where(f => f.FaultStatusID == FaultStatusFilter).ToList();
            }

            if ((TempData["FaultStatusFilter"] == null || (int)TempData["FaultStatusFilter"] == 0))
            {
                Faults = Faults.Where(f => f.FaultStatusID != 4).ToList();
            }

            //User has selected a fault priority.
            if (TempData["FaultPriorityFilter"] != null && (int)TempData["FaultPriorityFilter"] != 0)
            {
                FaultPriorityFilter = int.Parse(TempData["FaultPriorityFilter"].ToString());

                Faults = Faults.Where(f => f.FaultPriorityID == FaultPriorityFilter).ToList();
            }

            //Store the ShowFaultMap and ShowFaultTable values in TempData.
            TempData["ShowFaultMap"] = ShowFaultMap;
            TempData["ShowFaultTable"] = ShowFaultTable;
            TempData.Keep();

            //User is in the StaffReadWrite role.
            if (HttpContext.User.IsInRole("StaffReadWrite"))
            {
                //Set ReadWrite to true, this is needed for showing buttons on map popups.
                ReadWrite = "true";
            }
            //User is in the StaffRead role.
            else
            {
                //Set ReadWrite to true, this is needed for hiding buttons on map popups.
                ReadWrite = "false";
            }

            //Setup Pager.
            Pager.CurrentPage = 1;
            Pager.PageSize = await _settingsService.GetSettingInt(DFI.FaultReporting.Common.Constants.Settings.PAGESIZE);
            Pager.Count = Faults.Count;
            PagedFaults = await _pagerService.GetPaginatedFaults(Faults, Pager.CurrentPage, Pager.PageSize);
            PagedFaults = PagedFaults.OrderBy(f => f.FaultPriorityID).ToList();

            //Return the page.
            return Page();
        }
        #endregion Map/Table Tile Controls

        #region Dropdown Filter Change
        //This method is executed when either of the dropdown filters are changed.
        //When executed the Faults property is filtered based on the selected filter.
        public async Task<IActionResult> OnPostFilter()
        {
            await PopulateProperties();

            //Show all faults apart from the ones assigned to the current staff member.
            Faults = Faults.Where(f => f.StaffID != CurrentStaff.ID).ToList();

            //User has clicked the "Table" link.
            if (TempData["ShowFaultTable"] != null)
            {
                ShowFaultTable = Boolean.Parse(TempData["ShowFaultTable"].ToString());
            }

            //User has clicked the "Map" link or map is displayed by default.
            if (TempData["ShowFaultMap"] != null)
            {
                ShowFaultMap = Boolean.Parse(TempData["ShowFaultMap"].ToString());
            }
            //Map is displayed by default.
            else
            {
                ShowFaultMap = true;
            }

            //User has clicked "My Faults" button.
            if (TempData["ShowMyFaults"] != null)
            {
                ShowMyFaults = Boolean.Parse(TempData["ShowMyFaults"].ToString());

                if (ShowMyFaults)
                {
                    Faults = await _faultService.GetFaults();
                    Faults = Faults.Where(f => f.StaffID == CurrentStaff.ID).ToList();
                }
            }

            //User has clicked the "My Faults" button.
            if (ShowMyFaults)
            {
                Faults = await _faultService.GetFaults();
                Faults = Faults.Where(f => f.StaffID == CurrentStaff.ID).ToList();
            }
            else
            {
                Faults = await _faultService.GetFaults();
                Faults = Faults.Where(f => f.StaffID != CurrentStaff.ID).ToList();
            }

            //-------------------- TYPE FILTER --------------------
            //User has selected an type that is not "All".
            if (FaultTypeFilter != 0)
            {
                Faults = Faults.Where(f => f.FaultTypeID == FaultTypeFilter).ToList();
            }

            //-------------------- STATUS FILTER --------------------
            //User has selected a status that is not "All".
            if (FaultStatusFilter != 0)
            {
                //User has selected to view faults that have been repaired.
                if (FaultStatusFilter == 4)
                {
                    Faults = Faults.Where(f => f.FaultStatusID == 4).ToList();
                }
                else
                {
                    Faults = Faults.Where(f => f.FaultStatusID == FaultStatusFilter).ToList();
                }
            }
            else
            {
                Faults = Faults.Where(f => f.FaultStatusID != 4).ToList();
            }

            //-------------------- PRIORITY FILTER --------------------
            //User has selected a fault priority that is not "All".
            if (FaultPriorityFilter != 0)
            {
                Faults = Faults.Where(f => f.FaultPriorityID == FaultPriorityFilter).ToList();
                Faults = Faults.OrderBy(f => f.FaultPriorityID).ToList();
            }

            Faults = Faults.OrderBy(f => f.FaultPriorityID).ToList();

            //The fault table is displayed.
            if (ShowFaultTable)
            {
                //Setup Pager.
                Pager.CurrentPage = 1;
                Pager.Count = Faults.Count;
                Pager.PageSize = await _settingsService.GetSettingInt(DFI.FaultReporting.Common.Constants.Settings.PAGESIZE);
                PagedFaults = await _pagerService.GetPaginatedFaults(Faults, Pager.CurrentPage, Pager.PageSize);
                PagedFaults = PagedFaults.OrderBy(f => f.FaultPriorityID).ToList();
            }

            await SetSessionData();

            //Set the selected filters in temp data.
            TempData["FaultTypeFilter"] = FaultTypeFilter;
            TempData["FaultStatusFilter"] = FaultStatusFilter;
            TempData["FaultPriorityFilter"] = FaultPriorityFilter;
            TempData.Keep();

            //User is in the StaffReadWrite role.
            if (HttpContext.User.IsInRole("StaffReadWrite"))
            {
                //Set ReadWrite to true, this is needed for showing buttons on map popups.
                ReadWrite = "true";
            }
            //User is in the StaffRead role.
            else
            {
                //Set ReadWrite to true, this is needed for hiding buttons on map popups.
                ReadWrite = "false";
            }

            //User has selected the "Map" link.
            if (ShowFaultMap)
            {
                //Return the page and show the map, this is needed to ensure the correct section is displayed and the paging is reset.
                return Redirect("/Faults/Faults?handler=ShowMapView");
            }
            else
            {
                //Return the page and show the table, this is needed to ensure the correct section is displayed and the paging is reset.
                return Redirect("/Faults/Faults?handler=ShowTableView");
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

            //Show all faults apart from the ones assigned to the current staff member.
            Faults = Faults.Where(f => f.StaffID != CurrentStaff.ID).ToList();

            //User has clicked "My Faults" button.
            if (TempData["ShowMyFaults"] != null)
            {
                ShowMyFaults = Boolean.Parse(TempData["ShowMyFaults"].ToString());

                if (ShowMyFaults)
                {
                    Faults = await _faultService.GetFaults();
                    Faults = Faults.Where(f => f.StaffID == CurrentStaff.ID).ToList();
                }
            }

            //User has selected a fault type.
            if (TempData["FaultTypeFilter"] != null && (int)TempData["FaultTypeFilter"] != 0)
            {
                FaultTypeFilter = int.Parse(TempData["FaultTypeFilter"].ToString());

                Faults = Faults.Where(f => f.FaultTypeID == FaultTypeFilter).ToList();
            }

            //User has selected a fault status.
            if (TempData["FaultStatusFilter"] != null && (int)TempData["FaultStatusFilter"] != 0)
            {
                FaultStatusFilter = int.Parse(TempData["FaultStatusFilter"].ToString());

                Faults = Faults.Where(f => f.FaultStatusID == FaultStatusFilter).ToList();
            }

            if ((TempData["FaultStatusFilter"] == null || (int)TempData["FaultStatusFilter"] == 0))
            {
                Faults = Faults.Where(f => f.FaultStatusID != 4).ToList();
            }

            //User has selected a fault priority.
            if (TempData["FaultPriorityFilter"] != null && (int)TempData["FaultPriorityFilter"] != 0)
            {
                FaultPriorityFilter = int.Parse(TempData["FaultPriorityFilter"].ToString());

                Faults = Faults.Where(f => f.FaultPriorityID == FaultPriorityFilter).ToList();
            }

            //User is in the StaffReadWrite role.
            if (HttpContext.User.IsInRole("StaffReadWrite"))
            {
                //Set ReadWrite to true, this is needed for showing buttons on map popups.
                ReadWrite = "true";
            }
            //User is in the StaffRead role.
            else
            {
                //Set ReadWrite to true, this is needed for hiding buttons on map popups.
                ReadWrite = "false";
            }

            //Keep the TempData.
            TempData.Keep();

            //Hide the fault map.
            ShowFaultMap = false;

            //Show the fault table.
            ShowFaultTable = true;

            //Setup Pager.
            Pager.Count = Faults.Count;
            Pager.PageSize = await _settingsService.GetSettingInt(DFI.FaultReporting.Common.Constants.Settings.PAGESIZE);
            PagedFaults = await _pagerService.GetPaginatedFaults(Faults, Pager.CurrentPage, Pager.PageSize);
            PagedFaults = PagedFaults.OrderBy(f => f.FaultPriorityID).ToList();
        }
        #endregion Pagination

        #region Show My Faults/Show All Faults
        //Method Summary:
        //This method is executed when the "My Faults" button is clicked.
        //When executed the faults assigned to the current staff are displayed.
        public async Task<IActionResult> OnPostShowMyFaults() 
        {
            await PopulateProperties();

            //User has clicked the "Table" link.
            if (TempData["ShowFaultTable"] != null)
            {
                ShowFaultTable = Boolean.Parse(TempData["ShowFaultTable"].ToString());
            }

            //User has clicked the "Map" link or map is displayed by default.
            if (TempData["ShowFaultMap"] != null)
            {
                ShowFaultMap = Boolean.Parse(TempData["ShowFaultMap"].ToString());
            }
            //Map is displayed by default.
            else
            {
                ShowFaultMap = true;
            }

            //User has selected a fault type.
            if (TempData["FaultTypeFilter"] != null && (int)TempData["FaultTypeFilter"] != 0)
            {
                FaultTypeFilter = int.Parse(TempData["FaultTypeFilter"].ToString());

                Faults = Faults.Where(f => f.FaultTypeID == FaultTypeFilter).ToList();
            }

            //User has selected a fault status.
            if (TempData["FaultStatusFilter"] != null && (int)TempData["FaultStatusFilter"] != 0)
            {
                FaultStatusFilter = int.Parse(TempData["FaultStatusFilter"].ToString());

                Faults = Faults.Where(f => f.FaultStatusID == FaultStatusFilter).ToList();
            }

            if ((TempData["FaultStatusFilter"] == null || (int)TempData["FaultStatusFilter"] == 0))
            {
                Faults = Faults.Where(f => f.FaultStatusID != 4).ToList();
            }

            //User has selected a fault priority.
            if (TempData["FaultPriorityFilter"] != null && (int)TempData["FaultPriorityFilter"] != 0)
            {
                FaultPriorityFilter = int.Parse(TempData["FaultPriorityFilter"].ToString());

                Faults = Faults.Where(f => f.FaultPriorityID == FaultPriorityFilter).ToList();
            }

            //Set the ShowMyFaults property to true.
            ShowMyFaults = true;

            //Store the ShowMyFaults value in TempData.
            TempData["ShowMyFaults"] = ShowMyFaults;
            TempData.Keep();

            //Get faults assigned to the current staff.
            Faults = Faults.Where(f => f.StaffID == CurrentStaff.ID).ToList();

            //-------------------- TYPE FILTER --------------------
            //User has selected an type that is not "All".
            if (FaultTypeFilter != 0)
            {
                Faults = Faults.Where(f => f.FaultTypeID == FaultTypeFilter).ToList();
            }

            //-------------------- STATUS FILTER --------------------
            //User has selected a status that is not "All".
            if (FaultStatusFilter != 0)
            {

                //User has selected to view faults that have been repaired.
                if (FaultStatusFilter == 4)
                {
                    Faults = Faults.Where(f => f.FaultStatusID == 4).ToList();
                }
                else
                {
                    Faults = Faults.Where(f => f.FaultStatusID == FaultStatusFilter).ToList();
                }
            }
            else
            {
                Faults = Faults.Where(f => f.FaultStatusID != 4).ToList();
            }

            //-------------------- PRIORITY FILTER --------------------
            //User has selected a fault priority that is not "All".
            if (FaultPriorityFilter != 0)
            {
                Faults = Faults.Where(f => f.FaultPriorityID == FaultPriorityFilter).ToList();
                Faults = Faults.OrderBy(f => f.FaultPriorityID).ToList();
            }

            //Order the faults by priority.
            Faults = Faults.OrderBy(f => f.FaultPriorityID).ToList();

            //The fault table is displayed.
            if (ShowFaultTable)
            {
                //Setup Pager.
                Pager.CurrentPage = 1;
                Pager.PageSize = await _settingsService.GetSettingInt(DFI.FaultReporting.Common.Constants.Settings.PAGESIZE);
                Pager.Count = Faults.Count;
                PagedFaults = await _pagerService.GetPaginatedFaults(Faults, Pager.CurrentPage, Pager.PageSize);
                PagedFaults = PagedFaults.OrderBy(f => f.FaultPriorityID).ToList();
            }

            await SetSessionData();

            //User is in the StaffReadWrite role.
            if (HttpContext.User.IsInRole("StaffReadWrite"))
            {
                //Set ReadWrite to true, this is needed for showing buttons on map popups.
                ReadWrite = "true";
            }
            //User is in the StaffRead role.
            else
            {
                //Set ReadWrite to true, this is needed for hiding buttons on map popups.
                ReadWrite = "false";
            }

            //User has selected the "Map" link.
            if (ShowFaultMap)
            {
                //Return the page and show the map, this is needed to ensure the correct section is displayed and the paging is reset.
                return Redirect("/Faults/Faults?handler=ShowMapView");
            }
            else
            {
                //Return the page and show the table, this is needed to ensure the correct section is displayed and the paging is reset.
                return Redirect("/Faults/Faults?handler=ShowTableView");
            }
        }

        //Method Summary:
        //This method is executed when the "All faults" button is clicked.
        //When executed the all faults are displayed.
        public async Task<IActionResult> OnPostShowAllFaults()
        {
            await PopulateProperties();

            //User has clicked the "Table" link.
            if (TempData["ShowFaultTable"] != null)
            {
                ShowFaultTable = Boolean.Parse(TempData["ShowFaultTable"].ToString());
            }

            //User has clicked the "Map" link or map is displayed by default.
            if (TempData["ShowFaultMap"] != null)
            {
                ShowFaultMap = Boolean.Parse(TempData["ShowFaultMap"].ToString());
            }
            //Map is displayed by default.
            else
            {
                ShowFaultMap = true;
            }

            //User has selected a fault type.
            if (TempData["FaultTypeFilter"] != null && (int)TempData["FaultTypeFilter"] != 0)
            {
                FaultTypeFilter = int.Parse(TempData["FaultTypeFilter"].ToString());

                Faults = Faults.Where(f => f.FaultTypeID == FaultTypeFilter).ToList();
            }

            //User has selected a fault status.
            if (TempData["FaultStatusFilter"] != null && (int)TempData["FaultStatusFilter"] != 0)
            {
                FaultStatusFilter = int.Parse(TempData["FaultStatusFilter"].ToString());

                Faults = Faults.Where(f => f.FaultStatusID == FaultStatusFilter).ToList();
            }

            if ((TempData["FaultStatusFilter"] == null || (int)TempData["FaultStatusFilter"] == 0))
            {
                Faults = Faults.Where(f => f.FaultStatusID != 4).ToList();
            }

            //User has selected a fault priority.
            if (TempData["FaultPriorityFilter"] != null && (int)TempData["FaultPriorityFilter"] != 0)
            {
                FaultPriorityFilter = int.Parse(TempData["FaultPriorityFilter"].ToString());

                Faults = Faults.Where(f => f.FaultPriorityID == FaultPriorityFilter).ToList();
            }

            //Set the ShowMyFaults property to true.
            ShowMyFaults = false;

            //Store the ShowMyFaults value in TempData.
            TempData["ShowMyFaults"] = ShowMyFaults;
            TempData.Keep();

            //Get faults assigned to the current staff.
            Faults = Faults.Where(f => f.StaffID != CurrentStaff.ID).ToList();

            //-------------------- TYPE FILTER --------------------
            //User has selected an type that is not "All".
            if (FaultTypeFilter != 0)
            {
                Faults = Faults.Where(f => f.FaultTypeID == FaultTypeFilter).ToList();
            }

            //-------------------- STATUS FILTER --------------------
            //User has selected a status that is not "All".
            if (FaultStatusFilter != 0)
            {

                //User has selected to view faults that have been repaired.
                if (FaultStatusFilter == 4)
                {
                    Faults = Faults.Where(f => f.FaultStatusID == 4).ToList();
                }
                else
                {
                    Faults = Faults.Where(f => f.FaultStatusID == FaultStatusFilter).ToList();
                }
            }
            else
            {
                Faults = Faults.Where(f => f.FaultStatusID != 4).ToList();
            }

            //-------------------- PRIORITY FILTER --------------------
            //User has selected a fault priority that is not "All".
            if (FaultPriorityFilter != 0)
            {
                Faults = Faults.Where(f => f.FaultPriorityID == FaultPriorityFilter).ToList();
            }

            Faults = Faults.OrderBy(f => f.FaultPriorityID).ToList();

            //The fault table is displayed.
            if (ShowFaultTable)
            {
                //Setup Pager.
                Pager.CurrentPage = 1;
                Pager.PageSize = await _settingsService.GetSettingInt(DFI.FaultReporting.Common.Constants.Settings.PAGESIZE);
                Pager.Count = Faults.Count;
                PagedFaults = await _pagerService.GetPaginatedFaults(Faults, Pager.CurrentPage, Pager.PageSize);
                PagedFaults = PagedFaults.OrderBy(f => f.FaultPriorityID).ToList();
            }

            await SetSessionData();

            //User is in the StaffReadWrite role.
            if (HttpContext.User.IsInRole("StaffReadWrite"))
            {
                //Set ReadWrite to true, this is needed for showing buttons on map popups.
                ReadWrite = "true";
            }
            //User is in the StaffRead role.
            else
            {
                //Set ReadWrite to true, this is needed for hiding buttons on map popups.
                ReadWrite = "false";
            }

            //User has selected the "Map" link.
            if (ShowFaultMap)
            {
                //Return the page and show the map, this is needed to ensure the correct section is displayed and the paging is reset.
                return Redirect("/Faults/Faults?handler=ShowMapView");
            }
            else
            {
                //Return the page and show the table, this is needed to ensure the correct section is displayed and the paging is reset.
                return Redirect("/Faults/Faults?handler=ShowTableView");
            }
        }
        #endregion Show My Faults/Show All Faults
    }
}