using DFI.FaultReporting.Common.Pagination;
using DFI.FaultReporting.Common.SessionStorage;
using DFI.FaultReporting.Interfaces.Admin;
using DFI.FaultReporting.Interfaces.FaultReports;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Models.FaultReports;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Admin;
using DFI.FaultReporting.Services.Interfaces.Emails;
using DFI.FaultReporting.Services.Interfaces.Pagination;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Tokens;
using DFI.FaultReporting.Services.Interfaces.Users;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Claims;

namespace DFI.FaultReporting.Public.Pages.Faults
{
    public class MyReportsModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<MyReportsModel> _logger;
        private readonly IUserService _userService;
        private readonly IFaultService _faultService;
        private readonly IFaultPriorityService _faultPriorityService;
        private readonly IFaultStatusService _faultStatusService;
        private readonly IFaultTypeService _faultTypeService;
        private readonly IReportService _reportService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPagerService _pagerService;
        private readonly ISettingsService _settingsService;

        //Inject dependencies in constructor.
        public MyReportsModel(ILogger<MyReportsModel> logger, IUserService userService, IFaultService faultService, IFaultTypeService faultTypeService,
            IFaultPriorityService faultPriorityService, IFaultStatusService faultStatusService, IReportService reportService,
            IHttpContextAccessor httpContextAccessor, IPagerService pagerService, ISettingsService settingsService)
        {
            _logger = logger;
            _userService = userService;
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
        //Declare CurrentUser property, this is needed when calling the _userService.
        public User CurrentUser { get; set; }

        //Declare Faults property, this is needed for displaying faults on the map.
        [BindProperty]
        public List<Fault> Faults { get; set; }

        //Declare PagedFaults property, this is needed for displaying faults in the table.
        [BindProperty]
        public List<Fault> PagedFaults { get; set; }

        //Declare Reports property, this is needed for displaying the number of reports for each fault.
        [BindProperty]
        public List<Report> Reports { get; set; }

        //Declare FaultPriorities property, this is needed for displaying on map.
        [BindProperty]
        public List<FaultPriority> FaultPriorities { get; set; }

        //Declare FaultStatuses property, this is needed for displaying on map.
        [BindProperty]
        public List<FaultStatus> FaultStatuses { get; set; }

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

        //Declare UserHasReports property, this is needed for displaying the no reports message.
        [BindProperty]
        public bool UserHasReports { get; set; }

        //Declare ShowFaultMap property, this is needed for displaying the fault map.
        [BindProperty]
        public bool ShowFaultMap { get; set; }

        //Declare ShowFaultTable property, this is needed for displaying the fault table.
        [BindProperty]
        public bool ShowFaultTable { get; set; }

        //Declare Pager property, this is needed for pagination.
        [BindProperty(SupportsGet = true)]
        public Pager Pager { get; set; } = new Pager();
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
                if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true)
                {
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

                    //Get all Reports by calling the GetReports method from the _reportService.
                    Reports = await _reportService.GetReports();

                    //Get the reports for the current user.
                    Reports = Reports.Where(r => r.UserID == CurrentUser.ID).ToList();

                    //The user has submitted reports.
                    if (Reports != null && Reports.Count > 0)
                    {
                        //Hide the no reports message.
                        UserHasReports = true;

                        //Show the fault map by default.
                        ShowFaultMap = true;

                        //Get fault types for dropdown.
                        FaultTypes = await GetFaultTypes();

                        //Populate fault types dropdown.
                        FaultTypesList = FaultTypes.Select(ft => new SelectListItem()
                        {
                            Text = ft.FaultTypeDescription,
                            Value = ft.ID.ToString()
                        });

                        //Declare a new list of faults.
                        Faults = new List<Fault>();

                        //Get all fault priorities by calling the GetFaultPriorities method from the _faultPriorityService.
                        FaultPriorities = await _faultPriorityService.GetFaultPriorities();

                        //Get all fault statuses by calling the GetFaultStatuses method from the _faultStatusService.
                        FaultStatuses = await _faultStatusService.GetFaultStatuses();


                        //Loop through each report and get the fault for each report.
                        foreach (Report report in Reports)
                        {
                            //Get the fault by calling the GetFault method from the _faultService.
                            Fault fault = await _faultService.GetFault(report.FaultID, jwtToken);

                            //Add the fault to the Faults list.
                            Faults.Add(fault);
                        }

                        //Set the Faults in session, needed for displaying on map.
                        HttpContext.Session.SetInSession("Faults", Faults);
                        HttpContext.Session.SetInSession("FaultTypes", FaultTypes);
                        HttpContext.Session.SetInSession("FaultPriorities", FaultPriorities);
                        HttpContext.Session.SetInSession("FaultStatuses", FaultStatuses);
                        HttpContext.Session.SetInSession("Reports", Reports);

                        //Return the page.
                        return Page();
                    }
                    //The user has not submitted any reports.
                    else
                    {
                        //Show the no reports message.
                        UserHasReports = false;

                        //Hide the fault map.
                        ShowFaultMap = false;

                        //Show the fault table.
                        ShowFaultTable = false;

                        //Return the page.
                        return Page();
                    }
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
            //Hide the no reports message.
            UserHasReports = true;

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

            //Store the ShowFaultMap and ShowFaultTable values in TempData.
            TempData["ShowFaultMap"] = ShowFaultMap;
            TempData["ShowFaultTable"] = ShowFaultTable;
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

            //Hide the no reports message.
            UserHasReports = true;

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

            //Store the ShowFaultMap and ShowFaultTable values in TempData.
            TempData["ShowFaultMap"] = ShowFaultMap;
            TempData["ShowFaultTable"] = ShowFaultTable;
            TempData.Keep();

            //Get all required data from session.
            GetSessionData();

            //Set the pager count to the number of faults.
            Pager.Count = Faults.Count;

            //Get the paginated faults by calling the GetPaginatedFaults method from the _pagerService.
            PagedFaults = await _pagerService.GetPaginatedFaults(Faults, Pager.CurrentPage, Pager.PageSize);

            //Return the page.
            return Page();
        }
        #endregion Map/Table Tile Controls

        #region Dropdown Filter Change
        //This method is executed when either of the dropdown filters are changed.
        //When executed the Faults property is filtered based on the selected filter.
        public async Task<IActionResult> OnPost()
        {
            //Hide the no reports message.
            UserHasReports = true;

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

            //Get all required data from session.
            GetSessionData();

            //Get the JWT token claim from the contexts current user, needed for populating CurrentUser property from DB.
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

            //Set the jwtToken string to the JWT token claims value, needed for populating CurrentUser property from DB.
            string? jwtToken = jwtTokenClaim.Value;


            //Declare a new list of faults.
            Faults = new List<Fault>();

            //Loop through each report and get the fault for each report.
            foreach (Report report in Reports)
            {
                //Get the fault by calling the GetFault method from the _faultService.
                Fault fault = await _faultService.GetFault(report.FaultID, jwtToken);

                //Add the fault to the Faults list.
                Faults.Add(fault);
            }

            //User has selected an type that is not "All"
            if (FaultTypeFilter != 0)
            {
                //Get the faults for the selected type.
                Faults = Faults.Where(f => f.FaultTypeID == FaultTypeFilter).ToList();
            }

            //The fault table is displayed.
            if (ShowFaultTable)
            {             
                //Set the current page to 1.
                Pager.CurrentPage = 1;

                //Set the pager count to the number of faults.
                Pager.Count = Faults.Count;

                //Get the first page of faults by calling the GetPaginatedFaults method from the _pagerService.
                PagedFaults = await _pagerService.GetPaginatedFaults(Faults, Pager.CurrentPage, Pager.PageSize);
            }

            //Set the Faults in session, needed for displaying on map.
            HttpContext.Session.SetInSession("Faults", Faults);

            //Get fault types for dropdown.
            FaultTypes = await GetFaultTypes();

            //Populate fault types dropdown.
            FaultTypesList = FaultTypes.Select(ft => new SelectListItem()
            {
                Text = ft.FaultTypeDescription,
                Value = ft.ID.ToString()
            });

            //Get all fault priorities by calling the GetFaultPriorities method from the _faultPriorityService.
            FaultPriorities = await _faultPriorityService.GetFaultPriorities();

            //Get all fault statuses by calling the GetFaultStatuses method from the _faultStatusService.
            FaultStatuses = await _faultStatusService.GetFaultStatuses();

            //Set the selected fault type in TempData.
            TempData["FaultTypeFilter"] = FaultTypeFilter;
            TempData.Keep();

            //User has selected the "Map" link.
            if (ShowFaultMap)
            {
                //Return the page and show the map, this is needed to ensure the correct section is displayed and the paging is reset.
                return Redirect("/Faults/MyReports?handler=ShowMapView");
            }
            //User has selected the "Table" link.
            else
            {
                //Return the page and show the table, this is needed to ensure the correct section is displayed and the paging is reset.
                return Redirect("/Faults/MyReports?handler=ShowTableView");
            }
        }
        #endregion Dropdown Filter Change

        #region Pagination
        //Method Summary:
        //This method is excuted when the pagination buttons are clicked.
        //When executed the desired page of faults is displayed.
        public async void OnGetPaging()
        {
            //Set the FaultTypeFilter value to the value from TempData.
            if (TempData["FaultTypeFilter"] != null)
            {
                FaultTypeFilter = (int)TempData["FaultTypeFilter"];
            }

            //Keep the TempData.
            TempData.Keep();

            //Hide the no reports message.
            UserHasReports = true;

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
        }
        #endregion Pagination

        #region View Details
        //Method Summary:
        //This method is excuted when the "View Details" button in the faults table view is clicked.
        //When executed the fault ID is stored in TempData and the user is redirected to the ReportDetails page.
        public async Task<IActionResult> OnGetViewDetails(int ID)
        {
            //Clear TempData.
            //TempData.Clear();

            //Set the fault ID in TempData.
            TempData["FaultID"] = ID;
            TempData.Keep();

            //Redirect user to the ReportDetails page.
            return Redirect("/Faults/ReportDetails");
        }
        #endregion View Details

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

            //Get the reports from session.
            Reports = HttpContext.Session.GetFromSession<List<Report>>("Reports");

            //Populate fault types dropdown.
            FaultTypesList = FaultTypes.Select(ft => new SelectListItem()
            {
                Text = ft.FaultTypeDescription,
                Value = ft.ID.ToString()
            });
        }
        #endregion Session Data

        #region Fault Types
        //Method Summary:
        //This method is executed when the page loads and when an error occurs.
        //When executed this method returns a list of fault types to populate the fault types dropdown list.
        public async Task<List<FaultType>> GetFaultTypes()
        {
            //Get fault types by calling the GetFaultTypes method from the _faultTypeService.
            List<FaultType> faultTypes = await _faultTypeService.GetFaultTypes();

            //Return fault types.
            return faultTypes;
        }
        #endregion Fault Types
    }
}
