using DFI.FaultReporting.Admin.Pages.Faults.Reports;
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using DFI.FaultReporting.Interfaces.Claims;
using DFI.FaultReporting.Services.Interfaces.Claims;
using DFI.FaultReporting.Models.Claims;
using DocumentFormat.OpenXml.Spreadsheet;
using DFI.FaultReporting.Common.SessionStorage;
using OfficeOpenXml;
using static DFI.FaultReporting.Admin.Pages.Faults.Reports.FaultsDataReportModel;

namespace DFI.FaultReporting.Admin.Pages.Claims.Reports
{
    public class ClaimsDataReportModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<ClaimsDataReportModel> _logger;
        private readonly IStaffService _staffService;
        private readonly IClaimService _claimService;
        private readonly IClaimTypeService _claimTypeService;
        private readonly ILegalRepService _legalRepService;
        private readonly ISettingsService _settingsService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IClaimStatusService _claimStatusService;
        private readonly IWitnessService _witnessService;
        private readonly IPagerService _pagerService;
        private readonly IUserService _userService;

        //Inject dependencies in constructor.
        public ClaimsDataReportModel(ILogger<ClaimsDataReportModel> logger, IStaffService staffService, IClaimService claimService, IClaimTypeService claimTypeService,
            ISettingsService settingsService, IHttpContextAccessor httpContextAccessor, IClaimStatusService claimStatusService,
            IWitnessService witnessService, ILegalRepService legalRepService, IPagerService pagerService, IUserService userService)
        {
            _logger = logger;
            _staffService = staffService;
            _claimService = claimService;
            _claimTypeService = claimTypeService;
            _settingsService = settingsService;
            _httpContextAccessor = httpContextAccessor;
            _claimStatusService = claimStatusService;
            _witnessService = witnessService;
            _legalRepService = legalRepService;
            _pagerService = pagerService;
            _userService = userService;
        }
        #endregion Dependency Injection

        #region Properties
        public Staff CurrentStaff { get; set; }

        public List<Claim> Claims { get; set; }

        public List<Claim> PagedClaims { get; set; }

        [BindProperty(SupportsGet = true)]
        public Pager Pager { get; set; } = new Pager();

        [BindProperty]
        public List<ClaimType> ClaimTypes { get; set; }

        [BindProperty]
        public IEnumerable<SelectListItem> ClaimTypesList { get; set; }

        [DisplayName("Claim type")]
        [BindProperty]
        public int ClaimTypeFilter { get; set; }

        [BindProperty]
        public List<ClaimStatus> ClaimStatuses { get; set; }

        [BindProperty]
        public IEnumerable<SelectListItem> ClaimStatusList { get; set; }

        [DisplayName("Claim status")]
        [BindProperty]
        public int ClaimStatusFilter { get; set; }

        public List<Witness> Witnesses { get; set; }

        public List<LegalRep> LegalReps { get; set; }

        public List<Staff> Staff { get; set; }

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

        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
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

        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [BindProperty]
        public DateTime? DateTo { get; set; }

        [BindProperty]
        public List<string> ChartColors { get; set; }

        [BindProperty]
        public List<ExportClaimDataModel> ExportClaimDataList { get; set; }

        [BindProperty]
        public ExportClaimDataModel ExportClaimData { get; set; }

        public class ExportClaimDataModel
        {
            public int ID { get; set; }

            public string? Type { get; set; }

            public string? Status { get; set; }

            public string? FaultID { get; set; }

            public string? IncidentDate { get; set; }

            public string? IncidentDescription { get; set; }

            public string? IncidentLocationDescription { get; set; }

            public string? InjuryDescription { get; set; }

            public string? DamageDescription { get; set; }

            public string? DamageClaimDescription { get; set; }

            public string? InputOn { get; set; }
            public string? Staff { get; set; }
        }
        #endregion Properties


        #region Page Load

        //Method Summary:
        //This method is executed when the page loads.
        //When executed the ClaimTypes, and ClaimStatuses properties are populated.
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

                    await GenerateChartColors();

                    await SetSessionData();

                    //Setup pager.
                    Pager.CurrentPage = 1;
                    Pager.PageSize = await _settingsService.GetSettingInt(DFI.FaultReporting.Common.Constants.Settings.PAGESIZE);
                    Pager.Count = Claims.Count;
                    PagedClaims = await _pagerService.GetPaginatedClaims(Claims, Pager.CurrentPage, Pager.PageSize);

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
        //This method is executed when either of the dropdown filters are changed.
        //When executed the Faults property is filtered based on the selected filter.
        public async Task<IActionResult> OnPostFilter()
        {
            await PopulateProperties();

            //-------------------- TYPE FILTER --------------------
            //User has selected an type that is not "All".
            if (ClaimTypeFilter != 0)
            {
                Claims = Claims.Where(c => c.ClaimTypeID == ClaimTypeFilter).ToList();
            }

            //-------------------- STATUS FILTER --------------------
            //User has selected a status that is not "All".
            if (ClaimStatusFilter != 0)
            {
                Claims = Claims.Where(c => c.ClaimStatusID == ClaimStatusFilter).ToList();
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
                    //Get the claims for the selected date range.
                    Claims = Claims.Where(c => c.InputOn.Date == DateFrom).ToList();
                }
                else
                {
                    //Get the claims for the selected date range.
                    Claims = Claims.Where(c => c.InputOn.Date >= DateFrom && c.InputOn.Date <= DateTo).ToList();
                }
            }

            //Setup pager.
            Pager.CurrentPage = 1;
            Pager.PageSize = await _settingsService.GetSettingInt(DFI.FaultReporting.Common.Constants.Settings.PAGESIZE);
            Pager.Count = Claims.Count;
            PagedClaims = await _pagerService.GetPaginatedClaims(Claims, Pager.CurrentPage, Pager.PageSize);

            await SetSessionData();

            //Set the selected filters in temp data.
            TempData["ClaimTypeFilter"] = ClaimTypeFilter;
            TempData["ClaimStatusFilter"] = ClaimStatusFilter;
            TempData.Keep();

            return Page();
        }
        #endregion Filters 

        #region Pagination
        //Method Summary:
        //This method is excuted when the pagination buttons are clicked.
        //When executed the desired page of claims is displayed.
        public async Task OnGetPaging()
        {
            await PopulateProperties();

            //User has selected a claim type.
            if (TempData["ClaimTypeFilter"] != null && (int)TempData["ClaimTypeFilter"] != 0)
            {
                ClaimTypeFilter = int.Parse(TempData["ClaimTypeFilter"].ToString());

                Claims = Claims.Where(c => c.ClaimTypeID == ClaimTypeFilter).ToList();
            }

            //User has selected a claim status.
            if (TempData["ClaimStatusFilter"] != null && (int)TempData["ClaimStatusFilter"] != 0)
            {
                ClaimStatusFilter = int.Parse(TempData["ClaimStatusFilter"].ToString());

                Claims = Claims.Where(c => c.ClaimStatusID == ClaimStatusFilter).ToList();
            }

            //Keep the TempData.
            TempData.Keep();

            //Setup Pager.
            Pager.PageSize = await _settingsService.GetSettingInt(DFI.FaultReporting.Common.Constants.Settings.PAGESIZE);
            Pager.Count = Claims.Count;
            PagedClaims = await _pagerService.GetPaginatedClaims(Claims, Pager.CurrentPage, Pager.PageSize);
        }
        #endregion Pagination

        #region Chart Colors
        //Method Summary:
        //This method is executed when the page loads.
        //When executed, it generates random colors for each repair status for the charts.
        public async Task GenerateChartColors()
        {
            //Check if the ChartColors session exists, this is needed to ensure that the chart colors dont change when the user changes the filter options.
            if (HttpContext.Session.GetFromSession<List<string>>("ChartColors") == null || HttpContext.Session.GetFromSession<List<string>>("ChartColors").Count == 0)
            {
                ChartColors = new List<string>();

                Random random = new Random();

                //Loop through each claim type, this is needed to ensure that all types are assigned a color with any colors being reused.
                foreach (ClaimType claimType in ClaimTypes)
                {
                    System.Drawing.Color color = System.Drawing.Color.FromArgb(random.Next(0, 256), random.Next(0, 256), random.Next(0, 256));

                    string rgb = "rgba(" + color.R + ", " + color.G + ", " + color.B + ", " + 0.2 + ")";

                    //Check if the color is already in the list, this is needed to ensure that no colors are reused.
                    if (ChartColors.Contains(rgb))
                    {
                        while (ChartColors.Contains(rgb))
                        {
                            color = System.Drawing.Color.FromArgb(random.Next(0, 256), random.Next(0, 256), random.Next(0, 256));
                            rgb = "rgba(" + color.R + ", " + color.G + ", " + color.B + ", " + 0.2 + ")";
                        }
                    }

                    ChartColors.Add(rgb);
                }

                HttpContext.Session.SetInSession("ChartColors", ChartColors);
            }
        }
        #endregion Chart Colors

        #region Data
        //Method Summary:
        //This method is excuted when the a post occurs.
        //When excuted, it populates the page properties.
        public async Task PopulateProperties()
        {
            //Get the current user ID and JWT token.
            string? userID = _httpContextAccessor.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;
            System.Security.Claims.Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
            string? jwtToken = jwtTokenClaim.Value;
            CurrentStaff = await _staffService.GetStaff(Convert.ToInt32(userID), jwtToken);

            Claims = await _claimService.GetClaims(jwtToken);

            ClaimTypes = await _claimTypeService.GetClaimTypes(jwtToken);
            ClaimTypes = ClaimTypes.Where(ct => ct.Active == true).ToList();
            ClaimTypesList = ClaimTypes.Select(ct => new SelectListItem
            {
                Value = ct.ID.ToString(),
                Text = ct.ClaimTypeDescription
            });

            ClaimStatuses = await _claimStatusService.GetClaimStatuses(jwtToken);
            ClaimStatuses = ClaimStatuses.Where(cs => cs.Active == true).ToList();
            ClaimStatusList = ClaimStatuses.Select(cs => new SelectListItem
            {
                Value = cs.ID.ToString(),
                Text = cs.ClaimStatusDescription
            });

            Witnesses = await _witnessService.GetWitnesses(jwtToken);
            LegalReps = await _legalRepService.GetLegalReps(jwtToken);

            Staff = await _staffService.GetAllStaff(jwtToken);
        }

        //Method Summary:
        //This method is executed when a post occurs.
        //When executed, the page properties required in the charts javascript are set in session storage.
        public async Task SetSessionData()
        {
            HttpContext.Session.SetInSession("Claims", Claims);
            HttpContext.Session.SetInSession("ClaimTypes", ClaimTypes);
            HttpContext.Session.SetInSession("ClaimStatuses", ClaimStatuses);
            HttpContext.Session.SetInSession("Staff", Staff);
        }

        //Method Summary:
        //This method is executed when the export button is clicked.
        //When executed, the page properties required in the charts javascript are retrieved from session storage.
        public void GetSessionData()
        {
            Claims = HttpContext.Session.GetFromSession<List<Claim>>("Claims");
            ClaimTypes = HttpContext.Session.GetFromSession<List<ClaimType>>("ClaimTypes");
            ClaimStatuses = HttpContext.Session.GetFromSession<List<ClaimStatus>>("ClaimStatuses");
            Staff = HttpContext.Session.GetFromSession<List<Staff>>("Staff");
        }
        #endregion Data

        #region Export Data
        //Method Summary:
        //This method is executed when the export button is clicked.
        //When executed the data is exported to an excel file.
        public FileResult OnPostExportData()
        {
            GetSessionData();

            foreach (Claim claim in Claims)
            {
                ClaimType claimType = ClaimTypes.Where(ct => ct.ID == claim.ClaimTypeID).FirstOrDefault();
                string Type = claimType.ClaimTypeDescription;

                ClaimStatus claimStatus = ClaimStatuses.Where(cs => cs.ID == claim.ClaimStatusID).FirstOrDefault();
                string Status = claimStatus.ClaimStatusDescription;

                string staffName = "";

                if (claim.StaffID != null && claim.StaffID != 0)
                {
                    Staff staff = Staff.Where(s => s.ID == claim.StaffID).FirstOrDefault();
                    staffName = staff.FirstName + " " + staff.LastName;
                }

                //Create a new ExportClaimDataModel object, this is needed for the excel file.
                ExportClaimData = new ExportClaimDataModel
                {
                    ID = claim.ID,
                    Type = Type,
                    Status = Status,
                    FaultID = claim.FaultID.ToString(),
                    IncidentDate = claim.IncidentDate.ToString("dd/MM/yyyy"),
                    IncidentDescription = claim.IncidentDescription,
                    IncidentLocationDescription = claim.IncidentLocationDescription,
                    InjuryDescription = claim.InjuryDescription,
                    DamageDescription = claim.DamageDescription,
                    DamageClaimDescription = claim.DamageClaimDescription,
                    InputOn = claim.InputOn.ToString("dd/MM/yyyy"),
                    Staff = staffName
                };

                ExportClaimDataList.Add(ExportClaimData);
            }

            //Set the license context to non commercial.
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            //Create an excel file containing the faults data using memory stream.
            MemoryStream memoryStream = new MemoryStream();
            using (ExcelPackage excelPackage = new ExcelPackage(memoryStream))
            {
                var sheet = excelPackage.Workbook.Worksheets.Add("Sheet1");
                sheet.Cells.LoadFromCollection(ExportClaimDataList, true);
                excelPackage.Save();
            }

            memoryStream.Position = 0;
            string excelName = "Claims Data Report.xlsx";
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
        }
        #endregion
    }
}
