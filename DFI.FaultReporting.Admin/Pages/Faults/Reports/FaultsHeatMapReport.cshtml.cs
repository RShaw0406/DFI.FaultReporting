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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
        public Staff CurrentStaff { get; set; }

        [BindProperty]
        public List<Fault> Faults { get; set; }

        [BindProperty]
        public List<Report> Reports { get; set; }

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

        public bool ValidFromDate { get; set; }

        public bool ValidToDate { get; set; }

        public bool InValidYearFrom { get; set; }

        public bool InValidYearTo { get; set; }

        public string? InvalidDateMessage = "";

        [DisplayName("Day")]
        [BindProperty]
        public int? DayFrom { get; set; }

        [DisplayName("Month")]
        [BindProperty]
        public int? MonthFrom { get; set; }

        [DisplayName("Year")]
        [BindProperty]
        public int? YearFrom { get; set; }

        [DataType(DataType.Date)]
        [BindProperty]
        public DateTime? DateFrom { get; set; }

        [DisplayName("Day")]
        [BindProperty]
        public int? DayTo { get; set; }

        [DisplayName("Month")]
        [BindProperty]
        public int? MonthTo { get; set; }

        [DisplayName("Year")]
        [BindProperty]
        public int? YearTo { get; set; }

        [DataType(DataType.Date)]
        [BindProperty]
        public DateTime? DateTo { get; set; }
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
                    //Clear session to ensure fresh start.
                    HttpContext.Session.Clear();

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

        #region Dropdown Filter Change
        //This method is executed when either of the dropdown filters are changed.
        //When executed the Faults property is filtered based on the selected filter.
        public async Task<IActionResult> OnPostFilter()
        {
            await PopulateProperties();

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

            SetSessionData();

            //Set the selected fault type in TempData.
            TempData["FaultTypeFilter"] = FaultTypeFilter;
            TempData["FaultStatusFilter"] = FaultStatusFilter;
            TempData["FaultPriorityFilter"] = FaultPriorityFilter;
            TempData.Keep();

            return Page();
        }
        #endregion Dropdown Filter Change

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
        }
        #endregion Data
    }
}
