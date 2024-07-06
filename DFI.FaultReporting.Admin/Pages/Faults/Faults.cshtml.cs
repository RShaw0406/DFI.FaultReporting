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

        //Inject dependencies in constructor.
        public FaultsModel(ILogger<FaultsModel> logger, IStaffService staffService, IFaultService faultService, IFaultTypeService faultTypeService,
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
                    FaultTypes = await GetFaultTypes();

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
                    Faults = Faults.Where(f => f.FaultStatusID != 4).ToList();

                    //Show all faults apart from the ones assigned to the current staff member.
                    Faults = Faults.Where(f => f.StaffID != CurrentStaff.ID).ToList();

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

        #region Map/Table Tile Controls
        //Method Summary:
        //This method is excuted when the "Map" link is clicked.
        //When executed the "Map" section is displayed.
        public async Task<IActionResult> OnGetShowMapView()
        {
            //Hide the fault table.
            ShowFaultTable = false;

            //Show the fault map.
            ShowFaultMap = true;

            //User has selected a fault type.
            if (TempData["FaultTypeFilter"] != null)
            {
                //Get the FaultTypeFilter value from TempData.
                FaultTypeFilter = int.Parse(TempData["FaultTypeFilter"].ToString());
            }

            //User has selected a fault status.
            if (TempData["FaultStatusFilter"] != null)
            {
                //Get the FaultStatusFilter value from TempData.
                FaultStatusFilter = int.Parse(TempData["FaultStatusFilter"].ToString());
            }

            //User has selected a fault priority.
            if (TempData["FaultPriorityFilter"] != null)
            {
                //Get the FaultPriorityFilter value from TempData.
                FaultPriorityFilter = int.Parse(TempData["FaultPriorityFilter"].ToString());
            }

            //User has clicked "My Faults" button.
            if (TempData["ShowMyFaults"] != null)
            {
                //Get the ShowMyFaults value from TempData.
                ShowMyFaults = Boolean.Parse(TempData["ShowMyFaults"].ToString());
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

            //Hide the fault map.
            ShowFaultMap = false;

            //Show the fault table.
            ShowFaultTable = true;

            //User has selected a fault type.
            if (TempData["FaultTypeFilter"] != null)
            {
                //Get the FaultTypeFilter value from TempData.
                FaultTypeFilter = int.Parse(TempData["FaultTypeFilter"].ToString());
            }

            //User has selected a fault status.
            if (TempData["FaultStatusFilter"] != null)
            {
                //Get the FaultStatusFilter value from TempData.
                FaultStatusFilter = int.Parse(TempData["FaultStatusFilter"].ToString());
            }

            //User has selected a fault priority.
            if (TempData["FaultPriorityFilter"] != null)
            {
                //Get the FaultPriorityFilter value from TempData.
                FaultPriorityFilter = int.Parse(TempData["FaultPriorityFilter"].ToString());
            }

            //User has clicked "My Faults" button.
            if (TempData["ShowMyFaults"] != null)
            {
                //Get the ShowMyFaults value from TempData.
                ShowMyFaults = Boolean.Parse(TempData["ShowMyFaults"].ToString());
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

            //Get all required data from session.
            GetSessionData();
         
            //Set the pager count to the number of faults.
            Pager.Count = Faults.Count;

            //Get the paginated faults by calling the GetPaginatedFaults method from the _pagerService.
            PagedFaults = await _pagerService.GetPaginatedFaults(Faults, Pager.CurrentPage, Pager.PageSize);

            //Order the faults by priority.
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

            //User has clicked the "Table" link.
            if (TempData["ShowFaultTable"] != null)
            {
                //Get the ShowFaultTable value from TempData.
                ShowFaultTable = Boolean.Parse(TempData["ShowFaultTable"].ToString());
            }

            //User has clicked the "Map" link or map is displayed by default.
            if (TempData["ShowFaultMap"] != null)
            {
                //Get the ShowFaultMap value from TempData.
                ShowFaultMap = Boolean.Parse(TempData["ShowFaultMap"].ToString());
            }
            //Map is displayed by default.
            else
            {
                //Show the fault map by default.
                ShowFaultMap = true;
            }

            //User has clicked "My Faults" button.
            if (TempData["ShowMyFaults"] != null)
            {
                //Get the ShowMyFaults value from TempData.
                ShowMyFaults = Boolean.Parse(TempData["ShowMyFaults"].ToString());
            }

            //User has clicked the "My Faults" button.
            if (ShowMyFaults)
            {
                //Get all current faults by calling the GetFaults method from the _faultService, this is needed to ensure the selected filter is applied to map and table.
                Faults = await _faultService.GetFaults();

                //Get the faults assigned to the current staff.
                Faults = Faults.Where(f => f.StaffID == CurrentStaff.ID).ToList();
            }
            //User has not clicked the "My Faults" button.
            else
            {
                //Get all current faults by calling the GetFaults method from the _faultService, this is needed to ensure the selected filter is applied to map and table.
                Faults = await _faultService.GetFaults();

                //Show all faults apart from the ones assigned to the current staff member.
                Faults = Faults.Where(f => f.StaffID != CurrentStaff.ID).ToList();
            }

            //-------------------- TYPE FILTER --------------------
            //User has selected an type that is not "All".
            if (FaultTypeFilter != 0)
            {
                //Get the faults for the selected type.
                Faults = Faults.Where(f => f.FaultTypeID == FaultTypeFilter).ToList();
            }

            //-------------------- STATUS FILTER --------------------
            //User has selected a status that is not "All".
            if (FaultStatusFilter != 0)
            {

                //User has selected to view faults that have been repaired.
                if (FaultStatusFilter == 4)
                {
                    ////Get all current faults by calling the GetFaults method from the _faultService, this is needed to ensure that faults of repaired status are shown.
                    //Faults = await _faultService.GetFaults();

                    //Show all faults apart from the ones with the status of repaired.
                    Faults = Faults.Where(f => f.FaultStatusID == 4).ToList();
                }
                //User has selected a status that is not "All" and not "Repaired".
                else
                {
                    //Get the faults for the selected status.
                    Faults = Faults.Where(f => f.FaultStatusID == FaultStatusFilter).ToList();
                }
            }
            //User has not selected to view faults of all statuses
            else
            {
                //Show all faults apart from the ones with the status of repaired as default.
                Faults = Faults.Where(f => f.FaultStatusID != 4).ToList();
            }

            //-------------------- PRIORITY FILTER --------------------
            //User has selected a fault priority that is not "All".
            if (FaultPriorityFilter != 0)
            {
                //Get the faults for the selected priority.
                Faults = Faults.Where(f => f.FaultPriorityID == FaultPriorityFilter).ToList();

                //Order the faults by priority.
                Faults = Faults.OrderBy(f => f.FaultPriorityID).ToList();
            }

            //Order the faults by priority.
            Faults = Faults.OrderBy(f => f.FaultPriorityID).ToList();

            //The fault table is displayed.
            if (ShowFaultTable)
            {
                //Set the current page to 1.
                Pager.CurrentPage = 1;

                //Set the pager count to the number of faults.
                Pager.Count = Faults.Count;

                //Get the first page of faults by calling the GetPaginatedFaults method from the _pagerService.
                PagedFaults = await _pagerService.GetPaginatedFaults(Faults, Pager.CurrentPage, Pager.PageSize);

                //Order the faults by priority.
                PagedFaults = PagedFaults.OrderBy(f => f.FaultPriorityID).ToList();
            }

            //Set the Faults in session, needed for displaying on map.
            HttpContext.Session.SetInSession("Faults", Faults);

            //Set the selected fault type in TempData.
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
            //User has selected the "Table" link.
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
        public async void OnGetPaging()
        {
            //User has selected a fault type.
            if (TempData["FaultTypeFilter"] != null)
            {
                //Get the FaultTypeFilter value from TempData.
                FaultTypeFilter = int.Parse(TempData["FaultTypeFilter"].ToString());
            }

            //User has selected a fault status.
            if (TempData["FaultStatusFilter"] != null)
            {
                //Get the FaultStatusFilter value from TempData.
                FaultStatusFilter = int.Parse(TempData["FaultStatusFilter"].ToString());
            }

            //User has selected a fault priority.
            if (TempData["FaultPriorityFilter"] != null)
            {
                //Get the FaultPriorityFilter value from TempData.
                FaultPriorityFilter = int.Parse(TempData["FaultPriorityFilter"].ToString());
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

            //Get all required data from session.
            GetSessionData();

            //Set the pager count to the number of faults.
            Pager.Count = Faults.Count;

            //Get the first page of faults by calling the GetPaginatedFaults method from the _pagerService.
            PagedFaults = await _pagerService.GetPaginatedFaults(Faults, Pager.CurrentPage, Pager.PageSize);

            //Order the faults by priority.
            PagedFaults = PagedFaults.OrderBy(f => f.FaultPriorityID).ToList();
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

            ////Show all faults apart from the ones with the status of repaired as default.
            //Faults = Faults.Where(f => f.FaultStatusID != 4).ToList();

            //Order the faults by priority.
            Faults = Faults.OrderBy(f => f.FaultPriorityID).ToList();

            //Get the fault types from session.
            FaultTypes = HttpContext.Session.GetFromSession<List<FaultType>>("FaultTypes");

            //Filter out inactive fault types.
            FaultTypes = FaultTypes.Where(ft => ft.Active == true).ToList();

            //Get the fault priorities from session.
            FaultPriorities = HttpContext.Session.GetFromSession<List<FaultPriority>>("FaultPriorities");

            //Filter out inactive fault priorities.
            FaultPriorities = FaultPriorities.Where(fp => fp.Active == true).ToList();

            //Get the fault statuses from session.
            FaultStatuses = HttpContext.Session.GetFromSession<List<FaultStatus>>("FaultStatuses");

            //Filter out inactive fault statuses.
            FaultStatuses = FaultStatuses.Where(fs => fs.Active == true).ToList();

            //Get the reports from session.
            Reports = HttpContext.Session.GetFromSession<List<Report>>("Reports");

            //Get the staff from session.
            Staff = HttpContext.Session.GetFromSession<List<Staff>>("Staff");

            //Populate fault types dropdown.
            FaultTypesList = FaultTypes.Select(ft => new SelectListItem()
            {
                Text = ft.FaultTypeDescription,
                Value = ft.ID.ToString()
            });

            //Populate fault priorities dropdown.
            FaultPriorityList = FaultPriorities.Select(fp => new SelectListItem()
            {
                Text = fp.FaultPriorityRating,
                Value = fp.ID.ToString()
            });

            //Populate fault statuses dropdown.
            FaultStatusList = FaultStatuses.Select(fs => new SelectListItem()
            {
                Text = fs.FaultStatusDescription,
                Value = fs.ID.ToString()
            });

            //Get the ID from the contexts current user, needed for populating CurrentUser property from DB.
            string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            //Get the JWT token claim from the contexts current user, needed for populating CurrentUser property from DB.
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

            //Set the jwtToken string to the JWT token claims value, needed for populating CurrentUser property from DB.
            string? jwtToken = jwtTokenClaim.Value;

            //Set the CurrentStaff property by calling the GetUser method in the _userService.
            CurrentStaff = await _staffService.GetStaff(Convert.ToInt32(userID), jwtToken);

            //User has clicked "My Faults" button.
            if (TempData["ShowMyFaults"] != null)
            {
                //Get the ShowMyFaults value from TempData.
                ShowMyFaults = Boolean.Parse(TempData["ShowMyFaults"].ToString());
            }

            //User has clicked the "My Faults" button.
            if (ShowMyFaults)
            {
                //Get all current faults by calling the GetFaults method from the _faultService, this is needed to ensure the selected filter is applied to map and table.
                Faults = await _faultService.GetFaults();

                //Get the faults assigned to the current staff.
                Faults = Faults.Where(f => f.StaffID == CurrentStaff.ID).ToList();
            }
            //User has not clicked the "My Faults" button.
            else
            {
                //Get all current faults by calling the GetFaults method from the _faultService, this is needed to ensure the selected filter is applied to map and table.
                Faults = await _faultService.GetFaults();

                //Show all faults apart from the ones assigned to the current staff member.
                Faults = Faults.Where(f => f.StaffID != CurrentStaff.ID).ToList();
            }
        }
        #endregion Session Data

        #region Show My Faults
        //Method Summary:
        //This method is executed when the "My Faults" button is clicked.
        //When executed the faults assigned to the current staff are displayed.
        public async Task<IActionResult> OnPostShowMyFaults() 
        {
            //User has clicked the "Table" link.
            if (TempData["ShowFaultTable"] != null)
            {
                //Get the ShowFaultTable value from TempData.
                ShowFaultTable = Boolean.Parse(TempData["ShowFaultTable"].ToString());
            }

            //User has clicked the "Map" link or map is displayed by default.
            if (TempData["ShowFaultMap"] != null)
            {
                //Get the ShowFaultMap value from TempData.
                ShowFaultMap = Boolean.Parse(TempData["ShowFaultMap"].ToString());
            }
            //Map is displayed by default.
            else
            {
                //Show the fault map by default.
                ShowFaultMap = true;
            }

            //User has selected a fault type.
            if (TempData["FaultTypeFilter"] != null)
            {
                //Get the FaultTypeFilter value from TempData.
                FaultTypeFilter = int.Parse(TempData["FaultTypeFilter"].ToString());
            }

            //User has selected a fault status.
            if (TempData["FaultStatusFilter"] != null)
            {
                //Get the FaultStatusFilter value from TempData.
                FaultStatusFilter = int.Parse(TempData["FaultStatusFilter"].ToString());
            }

            //User has selected a fault priority.
            if (TempData["FaultPriorityFilter"] != null)
            {
                //Get the FaultPriorityFilter value from TempData.
                FaultPriorityFilter = int.Parse(TempData["FaultPriorityFilter"].ToString());
            }

            //Get all required data from session.
            GetSessionData();

            //Set the ShowMyFaults property to true.
            ShowMyFaults = true;

            //Store the ShowMyFaults value in TempData.
            TempData["ShowMyFaults"] = ShowMyFaults;
            TempData.Keep();

            //Get the ID from the contexts current user, needed for populating CurrentUser property from DB.
            string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            //Get the JWT token claim from the contexts current user, needed for populating CurrentUser property from DB.
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

            //Set the jwtToken string to the JWT token claims value, needed for populating CurrentUser property from DB.
            string? jwtToken = jwtTokenClaim.Value;

            //Set the CurrentStaff property by calling the GetUser method in the _userService.
            CurrentStaff = await _staffService.GetStaff(Convert.ToInt32(userID), jwtToken);

            //Get all current faults by calling the GetFaults method from the _faultService.
            Faults = await _faultService.GetFaults();

            //Get faults assigned to the current staff.
            Faults = Faults.Where(f => f.StaffID == CurrentStaff.ID).ToList();

            //-------------------- TYPE FILTER --------------------
            //User has selected an type that is not "All".
            if (FaultTypeFilter != 0)
            {
                //Get the faults for the selected type.
                Faults = Faults.Where(f => f.FaultTypeID == FaultTypeFilter).ToList();
            }

            //-------------------- STATUS FILTER --------------------
            //User has selected a status that is not "All".
            if (FaultStatusFilter != 0)
            {

                //User has selected to view faults that have been repaired.
                if (FaultStatusFilter == 4)
                {
                    ////Get all current faults by calling the GetFaults method from the _faultService, this is needed to ensure that faults of repaired status are shown.
                    //Faults = await _faultService.GetFaults();

                    //Show all faults apart from the ones with the status of repaired.
                    Faults = Faults.Where(f => f.FaultStatusID == 4).ToList();
                }
                //User has selected a status that is not "All" and not "Repaired".
                else
                {
                    //Get the faults for the selected status.
                    Faults = Faults.Where(f => f.FaultStatusID == FaultStatusFilter).ToList();
                }
            }
            //User has not selected to view faults of all statuses
            else
            {
                //Show all faults apart from the ones with the status of repaired as default.
                Faults = Faults.Where(f => f.FaultStatusID != 4).ToList();
            }

            //-------------------- PRIORITY FILTER --------------------
            //User has selected a fault priority that is not "All".
            if (FaultPriorityFilter != 0)
            {
                //Get the faults for the selected priority.
                Faults = Faults.Where(f => f.FaultPriorityID == FaultPriorityFilter).ToList();

                //Order the faults by priority.
                Faults = Faults.OrderBy(f => f.FaultPriorityID).ToList();
            }

            //Order the faults by priority.
            Faults = Faults.OrderBy(f => f.FaultPriorityID).ToList();

            //The fault table is displayed.
            if (ShowFaultTable)
            {
                //Set the current page to 1.
                Pager.CurrentPage = 1;

                //Set the pager count to the number of faults.
                Pager.Count = Faults.Count;

                //Get the first page of faults by calling the GetPaginatedFaults method from the _pagerService.
                PagedFaults = await _pagerService.GetPaginatedFaults(Faults, Pager.CurrentPage, Pager.PageSize);

                //Order the faults by priority.
                PagedFaults = PagedFaults.OrderBy(f => f.FaultPriorityID).ToList();
            }

            //Set session data needed for the page.
            HttpContext.Session.SetInSession("Faults", Faults);

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
            //User has selected the "Table" link.
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
            //User has clicked the "Table" link.
            if (TempData["ShowFaultTable"] != null)
            {
                //Get the ShowFaultTable value from TempData.
                ShowFaultTable = Boolean.Parse(TempData["ShowFaultTable"].ToString());
            }

            //User has clicked the "Map" link or map is displayed by default.
            if (TempData["ShowFaultMap"] != null)
            {
                //Get the ShowFaultMap value from TempData.
                ShowFaultMap = Boolean.Parse(TempData["ShowFaultMap"].ToString());
            }
            //Map is displayed by default.
            else
            {
                //Show the fault map by default.
                ShowFaultMap = true;
            }

            //User has selected a fault type.
            if (TempData["FaultTypeFilter"] != null)
            {
                //Get the FaultTypeFilter value from TempData.
                FaultTypeFilter = int.Parse(TempData["FaultTypeFilter"].ToString());
            }

            //User has selected a fault status.
            if (TempData["FaultStatusFilter"] != null)
            {
                //Get the FaultStatusFilter value from TempData.
                FaultStatusFilter = int.Parse(TempData["FaultStatusFilter"].ToString());
            }

            //User has selected a fault priority.
            if (TempData["FaultPriorityFilter"] != null)
            {
                //Get the FaultPriorityFilter value from TempData.
                FaultPriorityFilter = int.Parse(TempData["FaultPriorityFilter"].ToString());
            }

            //Get all required data from session.
            GetSessionData();

            //Set the ShowMyFaults property to true.
            ShowMyFaults = false;

            //Store the ShowMyFaults value in TempData.
            TempData["ShowMyFaults"] = ShowMyFaults;
            TempData.Keep();

            //Get the ID from the contexts current user, needed for populating CurrentUser property from DB.
            string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            //Get the JWT token claim from the contexts current user, needed for populating CurrentUser property from DB.
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

            //Set the jwtToken string to the JWT token claims value, needed for populating CurrentUser property from DB.
            string? jwtToken = jwtTokenClaim.Value;

            //Set the CurrentStaff property by calling the GetUser method in the _userService.
            CurrentStaff = await _staffService.GetStaff(Convert.ToInt32(userID), jwtToken);

            //Get all current faults by calling the GetFaults method from the _faultService.
            Faults = await _faultService.GetFaults();

            //Get faults assigned to the current staff.
            Faults = Faults.Where(f => f.StaffID != CurrentStaff.ID).ToList();

            //-------------------- TYPE FILTER --------------------
            //User has selected an type that is not "All".
            if (FaultTypeFilter != 0)
            {
                //Get the faults for the selected type.
                Faults = Faults.Where(f => f.FaultTypeID == FaultTypeFilter).ToList();
            }

            //-------------------- STATUS FILTER --------------------
            //User has selected a status that is not "All".
            if (FaultStatusFilter != 0)
            {

                //User has selected to view faults that have been repaired.
                if (FaultStatusFilter == 4)
                {
                    ////Get all current faults by calling the GetFaults method from the _faultService, this is needed to ensure that faults of repaired status are shown.
                    //Faults = await _faultService.GetFaults();

                    //Show all faults apart from the ones with the status of repaired.
                    Faults = Faults.Where(f => f.FaultStatusID == 4).ToList();
                }
                //User has selected a status that is not "All" and not "Repaired".
                else
                {
                    //Get the faults for the selected status.
                    Faults = Faults.Where(f => f.FaultStatusID == FaultStatusFilter).ToList();
                }
            }
            //User has not selected to view faults of all statuses
            else
            {
                //Show all faults apart from the ones with the status of repaired as default.
                Faults = Faults.Where(f => f.FaultStatusID != 4).ToList();
            }

            //-------------------- PRIORITY FILTER --------------------
            //User has selected a fault priority that is not "All".
            if (FaultPriorityFilter != 0)
            {
                //Get the faults for the selected priority.
                Faults = Faults.Where(f => f.FaultPriorityID == FaultPriorityFilter).ToList();

                //Order the faults by priority.
                Faults = Faults.OrderBy(f => f.FaultPriorityID).ToList();
            }

            //Order the faults by priority.
            Faults = Faults.OrderBy(f => f.FaultPriorityID).ToList();

            //The fault table is displayed.
            if (ShowFaultTable)
            {
                //Set the current page to 1.
                Pager.CurrentPage = 1;

                //Set the pager count to the number of faults.
                Pager.Count = Faults.Count;

                //Get the first page of faults by calling the GetPaginatedFaults method from the _pagerService.
                PagedFaults = await _pagerService.GetPaginatedFaults(Faults, Pager.CurrentPage, Pager.PageSize);

                //Order the faults by priority.
                PagedFaults = PagedFaults.OrderBy(f => f.FaultPriorityID).ToList();
            }

            //Set session data needed for the page.
            HttpContext.Session.SetInSession("Faults", Faults);

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
            //User has selected the "Table" link.
            else
            {
                //Return the page and show the table, this is needed to ensure the correct section is displayed and the paging is reset.
                return Redirect("/Faults/Faults?handler=ShowTableView");
            }
        }
        #endregion Show My Faults

        #region Fault Types
        //Method Summary:
        //This method is executed when the page loads and when an error occurs.
        //When executed this method returns a list of fault types to populate the fault types dropdown list.
        public async Task<List<FaultType>> GetFaultTypes()
        {
            //Get fault types by calling the GetFaultTypes method from the _faultTypeService.
            List<FaultType> faultTypes = await _faultTypeService.GetFaultTypes();

            //Filter out inactive fault types.
            faultTypes = faultTypes.Where(ft => ft.Active == true).ToList();

            //Return fault types.
            return faultTypes;
        }
        #endregion Fault Types
    }
}