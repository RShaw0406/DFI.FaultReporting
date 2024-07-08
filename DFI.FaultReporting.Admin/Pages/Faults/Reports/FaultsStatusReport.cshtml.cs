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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using DFI.FaultReporting.Common.SessionStorage;
using System.Security.Claims;
using System.Drawing;
using DFI.FaultReporting.Common.Pagination;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text;
using OfficeOpenXml;

namespace DFI.FaultReporting.Admin.Pages.Faults.Reports
{
    public class FaultsStatusReportModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<FaultsStatusReportModel> _logger;
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
        public FaultsStatusReportModel(ILogger<FaultsStatusReportModel> logger, IStaffService staffService, IFaultService faultService, IFaultTypeService faultTypeService,
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

        //Declare PagedFaults property, this is needed for displaying faults in the table.
        [BindProperty]
        public List<Fault> PagedFaults { get; set; }

        //Declare Staff property, this is needed for getting the staff assigned to each fault.
        [BindProperty]
        public List<Staff> Staff { get; set; }

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

        //Declare ValidFromDate property, this is needed for validating the filter from date.
        public bool ValidFromDate { get; set; }

        //Declare ValidToDate property, this is needed for validating the filter to date.
        public bool ValidToDate { get; set; }

        //Declare InValidYearFrom property, this is needed for validating the year from in the from date filter.
        public bool InValidYearFrom { get; set; }

        //Declare InValidYearDOB property, this is needed for validating the year to in the to date filter.
        public bool InValidYearTo { get; set; }

        //Declare InvalidDateMessage property, this is needed for storing the specific error message for the date filters.
        public string? InvalidDateMessage = "";

        ////Declare InValidYearToMessage property, this is needed for storing the specific error message when using the to date filter.
        //public string? InValidYearToMessage = "";

        //Declare DayFrom property, this is needed for storing the day in from date filter.
        [DisplayName("Day")]
        [BindProperty]
        public int? DayFrom { get; set; }

        //Declare MonthFrom property, this is needed for storing the month in from date filter.
        [DisplayName("Month")]
        [BindProperty]
        public int? MonthFrom { get; set; }

        //Declare YearFrom property, this is needed for storing the year in from date filter.
        [DisplayName("Year")]
        [BindProperty]
        public int? YearFrom { get; set; }

        //Declare DateFrom property, this is needed for storing the from date filter.
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [BindProperty]
        public DateTime? DateFrom { get; set; }

        //Declare DayTo property, this is needed for storing the day in to date filter.
        [DisplayName("Day")]
        [BindProperty]
        public int? DayTo { get; set; }

        //Declare MonthTo property, this is needed for storing the month in to date filter.
        [DisplayName("Month")]
        [BindProperty]
        public int? MonthTo { get; set; }

        //Declare YearTo property, this is needed for storing the year in to date filter.
        [DisplayName("Year")]
        [BindProperty]
        public int? YearTo { get; set; }

        //Declare DateTo property, this is needed for storing the to date filter.
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [BindProperty]
        public DateTime? DateTo { get; set; }

        //Declare Pager property, this is needed for pagination.
        [BindProperty(SupportsGet = true)]
        public Pager Pager { get; set; } = new Pager();

        //Declare ExportFaultDataList property, this is needed for exporting the data to excel.
        [BindProperty]
        public List<ExportFaultDataModel> ExportFaultDataList { get; set; }

        //Declare ExportFaultData property, this is needed for exporting the data to excel.
        [BindProperty]
        public ExportFaultDataModel ExportFaultData { get; set; }

        //Declare ExportFaultDataModel class, this is needed for exporting the data to excel as we need the type, priority, status, road number,
        //road name, road town, road county, input on, and staff.
        public class ExportFaultDataModel
        {
            public int ID { get; set; }
            public string? Type { get; set; }

            public string? PriorityRating { get; set; }

            public string? Priority { get; set; }

            public string? Status { get; set; }

            public string? RoadNumber { get; set; }

            public string? RoadName { get; set; }

            public string? RoadTown { get; set; }

            public string? RoadCounty { get; set; }

            public string? InputOn { get; set; }
            public string? Staff { get; set; }
        }
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

                    //Order the fault types by ID.
                    FaultTypes = FaultTypes.OrderBy(ft => ft.ID).ToList();

                    //Populate fault types dropdown.
                    FaultTypesList = FaultTypes.Select(ft => new SelectListItem()
                    {
                        Text = ft.FaultTypeDescription,
                        Value = ft.ID.ToString()
                    });

                    //Get all current faults by calling the GetFaults method from the _faultService.
                    Faults = await _faultService.GetFaults();

                    //Order the faults by status.
                    Faults = Faults.OrderBy(f => f.FaultStatusID).ToList();

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

                    FaultStatuses = FaultStatuses.OrderBy(fs => fs.ID).ToList();

                    //Populate fault statuses dropdown.
                    FaultStatusList = FaultStatuses.Select(fs => new SelectListItem()
                    {
                        Text = fs.FaultStatusDescription,
                        Value = fs.ID.ToString()
                    });

                    //Set the CurrentStaff property by calling the GetAllStaff method in the _staffService.
                    Staff = await _staffService.GetAllStaff(jwtToken);

                    //Get all Reports by calling the GetReports method from the _reportService.
                    Reports = await _reportService.GetReports();

                    //Set the current page to 1.
                    Pager.CurrentPage = 1;

                    //Set the pager count to the number of faults.
                    Pager.Count = Faults.Count;

                    //Get the first page of faults by calling the GetPaginatedFaults method from the _pagerService.
                    PagedFaults = await _pagerService.GetPaginatedFaults(Faults, Pager.CurrentPage, Pager.PageSize);

                    //Order the faults by priority.
                    PagedFaults = PagedFaults.OrderBy(f => f.FaultPriorityID).ToList();

                    //Set session data needed for the page.
                    HttpContext.Session.SetInSession("Faults", Faults);
                    HttpContext.Session.SetInSession("FaultTypes", FaultTypes);
                    HttpContext.Session.SetInSession("FaultPriorities", FaultPriorities);
                    HttpContext.Session.SetInSession("FaultStatuses", FaultStatuses);
                    HttpContext.Session.SetInSession("Reports", Reports);
                    HttpContext.Session.SetInSession("Staff", Staff);

                    //Check if the ChartColors session exists.
                    if (HttpContext.Session.GetFromSession<List<string>>("ChartColors") == null || HttpContext.Session.GetFromSession<List<string>>("ChartColors").Count == 0)
                    {
                        //Create a list to store the chart colors.
                        List<string> chartColors = new List<string>();

                        //Create a random object.
                        Random random = new Random();

                        //Loop through each fault type, this is needed to ensure that all fault types are assigned a color with any colors being reused.
                        foreach (FaultType faultType in FaultTypes)
                        {
                            //Create a random color.
                            System.Drawing.Color color = System.Drawing.Color.FromArgb(random.Next(0, 256), random.Next(0, 256), random.Next(0, 256));

                            //Create a string with the rgb values.
                            string rgb = "rgba(" + color.R + ", " + color.G + ", " + color.B + ", " + 0.2 + ")";

                            //Check if the color is already in the list.
                            if (chartColors.Contains(rgb))
                            {
                                //Create a new color.
                                while (chartColors.Contains(rgb))
                                {
                                    //Create a new random color.
                                    color = System.Drawing.Color.FromArgb(random.Next(0, 256), random.Next(0, 256), random.Next(0, 256));

                                    //Create a new string with the rgb values.
                                    rgb = "rgba(" + color.R + ", " + color.G + ", " + color.B + ", " + 0.2 + ")";
                                }
                            }

                            //Add the color to the list.
                            chartColors.Add(rgb);
                        }

                        //Set the ChartColors in session, this is needed for retrieval by the javascript for displying the charts.
                        HttpContext.Session.SetInSession("ChartColors", chartColors);
                    }

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

            //Get all current faults by calling the GetFaults method from the _faultService, this is needed to ensure the selected filter is applied to map.
            Faults = Faults = await _faultService.GetFaults();

            //Order the faults by status.
            Faults = Faults.OrderBy(f => f.FaultStatusID).ToList();

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
                //Get the faults for the selected status.
                Faults = Faults.Where(f => f.FaultStatusID == FaultStatusFilter).ToList();
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


            //-------------------- DATE FILTER --------------------
            if (DayFrom != null && MonthFrom != null && YearFrom != null)
            {
                //Check if the year is valid.
                if (YearFrom < 2024 || YearFrom > DateTime.Now.Year)
                {
                    //Set the InValidYearFrom property to true.
                    InValidYearFrom = true;

                    //Set the InvalidDateMessage property to the specific error message.
                    InvalidDateMessage = "Invalid year, please select a year between 2024 and " + DateTime.Now.Year;
                }
                else
                {
                    //Set the InValidYearFrom property to false.
                    InValidYearFrom = false;
                }

                //Check if the date is valid.
                if (DayFrom > 0 && DayFrom <= 31 && MonthFrom > 0 && MonthFrom <= 12 && YearFrom >= 2024 && YearFrom <= DateTime.Now.Year)
                {
                    //Set the ValidFromDate property to true.
                    ValidFromDate = true;

                    //Set the InvalidDateMessage property to an empty string.
                    InvalidDateMessage = "";
                }
                else
                {
                    //Set the ValidFromDate property to false.
                    ValidFromDate = false;

                    //Set the InvalidDateMessage property to the specific error message.
                    InvalidDateMessage = "Invalid date, please enter a valid date";
                }

                //The date is valid.
                if (ValidFromDate == true)
                {
                    //Set the DateFrom property to the selected date.
                    DateFrom = new DateTime(YearFrom.Value, MonthFrom.Value, DayFrom.Value);
                }
            }

            if (DayTo != null && MonthTo != null && YearTo != null)
            {
                //Check if the year is valid.
                if (YearTo < 2024 || YearTo > DateTime.Now.Year)
                {
                    //Set the InValidYearTo property to true.
                    InValidYearTo = true;

                    //Set the InvalidDateMessage property to the specific error message.
                    InvalidDateMessage = "Invalid year, please select a year between 2024 and " + DateTime.Now.Year;
                }
                else
                {
                    //Set the InValidYearTo property to false.
                    InValidYearTo = false;
                }

                //Check if the date is valid.
                if (DayTo > 0 && DayTo <= 31 && MonthTo > 0 && MonthTo <= 12 && YearTo >= 2024 && YearTo <= DateTime.Now.Year)
                {
                    //Set the ValidToDate property to true.
                    ValidToDate = true;

                    //Set the InvalidDateMessage property to an empty string.
                    InvalidDateMessage = "";
                }
                else
                {
                    //Set the ValidToDate property to false.
                    ValidToDate = false;

                    //Set the InvalidDateMessage property to the specific error message.
                    InvalidDateMessage = "Invalid date, please enter a valid date";
                }

                //The date is valid.
                if (ValidToDate == true)
                {
                    //Set the DateTo property to the selected date.
                    DateTo = new DateTime(YearTo.Value, MonthTo.Value, DayTo.Value);
                }
            }

            if (DateTo < DateFrom)
            {
                ValidToDate = false;

                //Set the InvalidDateMessage property to the specific error message.
                InvalidDateMessage = "To date can not be greater than from date";
            }

            if (ValidToDate && ValidFromDate)
            {
                if (DateFrom == DateTo)
                {
                    //Get the faults for the selected date range.
                    Faults = Faults.Where(f => f.InputOn.Date == DateFrom).ToList();
                }
                else
                {
                    //Get the faults for the selected date range.
                    Faults = Faults.Where(f => f.InputOn.Date >= DateFrom && f.InputOn.Date <= DateTo).ToList();
                }
            }

            //Set the current page to 1.
            Pager.CurrentPage = 1;

            //Set the pager count to the number of faults.
            Pager.Count = Faults.Count;

            //Get the first page of faults by calling the GetPaginatedFaults method from the _pagerService.
            PagedFaults = await _pagerService.GetPaginatedFaults(Faults, Pager.CurrentPage, Pager.PageSize);

            //Order the faults by priority.
            PagedFaults = PagedFaults.OrderBy(f => f.FaultPriorityID).ToList();

            //Set the Faults in session, needed for displaying on map.
            HttpContext.Session.SetInSession("Faults", Faults);

            //Set the selected fault type in TempData.
            TempData["FaultTypeFilter"] = FaultTypeFilter;
            TempData["FaultStatusFilter"] = FaultStatusFilter;
            //TempData["FaultPriorityFilter"] = FaultPriorityFilter;
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
        }
        #endregion Session Data

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

            //Keep the TempData.
            TempData.Keep();

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

        #region Export Data
        //Method Summary:
        //This method is executed when the export button is clicked.
        //When executed the data is exported to an excel file.
        public FileResult OnPostExportData()
        {
            //Get all required data from session.
            GetSessionData();

            //Loop over each of the faults.
            foreach (Fault fault in Faults)
            {
                //Get the fault type for the fault.
                FaultType faultType = FaultTypes.Where(ft => ft.ID == fault.FaultTypeID).FirstOrDefault();
                string Type = faultType.FaultTypeDescription;

                //Get the fault priority for the fault.
                FaultPriority faultPriority = FaultPriorities.Where(fp => fp.ID == fault.FaultPriorityID).FirstOrDefault();
                string PriorityRating = faultPriority.FaultPriorityRating;
                string Priority = faultPriority.FaultPriorityDescription;

                //Get the fault status for the fault.
                FaultStatus faultStatus = FaultStatuses.Where(fs => fs.ID == fault.FaultStatusID).FirstOrDefault();
                string Status = faultStatus.FaultStatusDescription;

                //Declare staff name as string.
                string staffName = "";

                //Check if the fault has been assigned to a staff member.
                if (fault.StaffID != null && fault.StaffID != 0)
                {
                    //Get the staff member assigned to the fault.
                    Staff staff = Staff.Where(s => s.ID == fault.StaffID).FirstOrDefault();
                    staffName = staff.FirstName + " " + staff.LastName;
                }

                //Create a new ExportFaultDataModel object.
                ExportFaultData = new ExportFaultDataModel
                {
                    ID = fault.ID,
                    Type = Type,
                    PriorityRating = PriorityRating,
                    Priority = Priority,
                    Status = Status,
                    RoadNumber = fault.RoadNumber,
                    RoadName = fault.RoadName,
                    RoadTown = fault.RoadTown,
                    RoadCounty = fault.RoadCounty,
                    InputOn = fault.InputOn.ToString("dd/MM/yyyy"),
                    Staff = staffName
                };

                //Add the ExportFaultData to the ExportFaultDataList.
                ExportFaultDataList.Add(ExportFaultData);
            }

            //Create a new memory stream.
            MemoryStream memoryStream = new MemoryStream();

            //Set the license context to non commercial.
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            //Create a new excel package.
            using (ExcelPackage excelPackage = new ExcelPackage(memoryStream))
            {
                //Create a new worksheet.
                var sheet = excelPackage.Workbook.Worksheets.Add("Sheet1");
                sheet.Cells.LoadFromCollection(ExportFaultDataList, true);
                excelPackage.Save();
            }

            //Set the position of the memory stream to 0.
            memoryStream.Position = 0;
            string excelName = "Fault Status Report.xlsx";

            //Return the file.
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);     
        }
        #endregion
    }
}
