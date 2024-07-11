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
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using static DFI.FaultReporting.Admin.Pages.Faults.Reports.FaultsDataReportModel;

namespace DFI.FaultReporting.Admin.Pages.Repairs.Reports
{
    public class RepairsDataReportModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<RepairsDataReportModel> _logger;
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
        public RepairsDataReportModel(ILogger<RepairsDataReportModel> logger, IStaffService staffService, IFaultService faultService, IFaultTypeService faultTypeService,
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

        [BindProperty]
        public Contractor Contractor { get; set; }

        [BindProperty(SupportsGet = true)]
        public Pager Pager { get; set; } = new Pager();

        [BindProperty]
        public List<string> ChartColors { get; set; }

        [BindProperty]
        public List<ExportRepairDataModel> ExportRepairDataList { get; set; }

        [BindProperty]
        public ExportRepairDataModel ExportRepairData { get; set; }

        public class ExportRepairDataModel
        {
            public int ID { get; set; }

            public string? Contractor { get; set; }

            public string? Status { get; set; }

            public string? TargetDate { get; set; }

            public string? ActualRepairDate { get; set; }

            public string? FaultType { get; set; }

            public string? FaultPriority { get; set; }

            public string? FaultPriorityRating { get; set; }

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
        //When executed the user is checked for authentication and role, the required data is retrieved from the DB, and chart colors are generated.
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

                    await GenerateChartColors();

                    await SetSessionData();

                    //Setup pager for table.
                    Pager.CurrentPage = 1;
                    Pager.Count = Repairs.Count;
                    PagedRepairs = await _pagerService.GetPaginatedRepairs(Repairs, Pager.CurrentPage, Pager.PageSize);
                    PagedRepairs = PagedRepairs.OrderBy(r => r.RepairStatusID).ToList();

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

            //-------------------- SEARCH FILTER --------------------
            if (SearchString != null && SearchString != string.Empty)
            {
                Repairs = Repairs.Where(r => r.ContractorID == SearchID).ToList();
            }

            //-------------------- STATUS FILTER --------------------
            //User has selected a status that is not "All".
            if (RepairStatusFilter != 0)
            {
                Repairs = Repairs.Where(r => r.RepairStatusID == RepairStatusFilter).ToList();
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
            Pager.Count = Repairs.Count;
            PagedRepairs = await _pagerService.GetPaginatedRepairs(Repairs, Pager.CurrentPage, Pager.PageSize);
            PagedRepairs = PagedRepairs.OrderBy(r => r.RepairStatusID).ToList();

            await SetSessionData();

            //Set the selected repair status and taregt met in TempData.
            TempData["RepairStatusFilter"] = RepairStatusFilter;
            TempData["TargetMetFilter"] = TargetMetFilter;
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

            //User has selected a repair status.
            if (TempData["RepairStatusFilter"] != null && (int)TempData["RepairStatusFilter"] != 0)
            {
                RepairStatusFilter = int.Parse(TempData["RepairStatusFilter"].ToString());
            }

            //User has selected a target met status.
            if (TempData["TargetMetFilter"] != null && (int)TempData["TargetMetFilter"] != 0)
            {
                TargetMetFilter = int.Parse(TempData["TargetMetFilter"].ToString());
            }

            TempData.Keep();

            //Setup pager for table.
            Pager.Count = Repairs.Count;
            PagedRepairs = await _pagerService.GetPaginatedRepairs(Repairs, Pager.CurrentPage, Pager.PageSize);
            PagedRepairs = PagedRepairs.OrderBy(r => r.RepairStatusID).ToList();
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

                //Loop through each repair status, this is needed to ensure that all statuses are assigned a color with any colors being reused.
                foreach (RepairStatus repairStatus in RepairStatuses)
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
        //This method is excuted when the page loads or when the user changes the filter options.
        //When excuted, it populates the page properties.
        public async Task PopulateProperties()
        {
            //Get the current user ID and JWT token.
            string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
            string? jwtToken = jwtTokenClaim.Value;
            CurrentStaff = await _staffService.GetStaff(Convert.ToInt32(userID), jwtToken);

            Faults = await _faultService.GetFaults();

            Staff = await _staffService.GetAllStaff(jwtToken);

            Repairs = await _repairService.GetRepairs(jwtToken);

            RepairStatuses = await _repairStatusService.GetRepairStatuses(jwtToken);
            RepairStatuses = RepairStatuses.Where(rs => rs.Active == true).ToList();

            //Populate the repair status list, this is needed for the repair status dropdown options.
            RepairStatusList = RepairStatuses.Select(rs => new SelectListItem
            {
                Text = rs.RepairStatusDescription,
                Value = rs.ID.ToString()
            });

            FaultPriorities = await _faultPriorityService.GetFaultPriorities();

            FaultStatuses = await _faultStatusService.GetFaultStatuses();

            FaultTypes = await _faultTypeService.GetFaultTypes();

            Contractors = await _contractorService.GetContractors(jwtToken);
        }

        //Method Summary:
        //This method is executed when a post occurs.
        //When executed, the page properties required in the charts javascript are set in session storage.
        public async Task SetSessionData()
        {
            HttpContext.Session.SetInSession("Repairs", Repairs);
            HttpContext.Session.SetInSession("Faults", Faults);
            HttpContext.Session.SetInSession("Staff", Staff);
            HttpContext.Session.SetInSession("RepairStatuses", RepairStatuses);
            HttpContext.Session.SetInSession("FaultPriorities", FaultPriorities);
            HttpContext.Session.SetInSession("FaultStatuses", FaultStatuses);
            HttpContext.Session.SetInSession("FaultTypes", FaultTypes);
            HttpContext.Session.SetInSession("Contractors", Contractors);
        }

        //Method Summary:
        //This method is executed when the export button is clicked.
        //When executed, the page properties required in the charts javascript are retrieved from session storage.
        public void GetSessionData()
        {
            Repairs = HttpContext.Session.GetFromSession<List<Repair>>("Repairs");
            Faults = HttpContext.Session.GetFromSession<List<Fault>>("Faults");
            Staff = HttpContext.Session.GetFromSession<List<Staff>>("Staff");
            RepairStatuses = HttpContext.Session.GetFromSession<List<RepairStatus>>("RepairStatuses");
            FaultPriorities = HttpContext.Session.GetFromSession<List<FaultPriority>>("FaultPriorities");
            FaultStatuses = HttpContext.Session.GetFromSession<List<FaultStatus>>("FaultStatuses");
            FaultTypes = HttpContext.Session.GetFromSession<List<FaultType>>("FaultTypes");
            Contractors = HttpContext.Session.GetFromSession<List<Contractor>>("Contractors");
        }
        #endregion Data

        #region Export Data
        //Method Summary:
        //This method is executed when the export button is clicked.
        //When executed the repairs data is exported to an excel file.
        public FileResult OnPostExportData()
        {
            GetSessionData();

            foreach (Repair repair in Repairs)
            {
                //Populate the data needed for the excel file.
                Contractor contractor = Contractors.Where(c => c.ID == repair.ContractorID).FirstOrDefault();
                string contractorName = contractor.ContractorName;

                RepairStatus repairStatus = RepairStatuses.Where(rs => rs.ID == repair.RepairStatusID).FirstOrDefault();
                string Status = repairStatus.RepairStatusDescription;

                Fault fault = Faults.Where(f => f.ID == repair.FaultID).FirstOrDefault();

                FaultType faultType = FaultTypes.Where(ft => ft.ID == fault.FaultTypeID).FirstOrDefault();
                string faultTypeDescription = faultType.FaultTypeDescription;

                FaultPriority faultPriority = FaultPriorities.Where(fp => fp.ID == fault.FaultPriorityID).FirstOrDefault();
                string faultPriorityRating = faultPriority.FaultPriorityRating;
                string faultPriorityDescription = faultPriority.FaultPriorityDescription;

                string staffName = "";

                if (fault.StaffID != null && fault.StaffID != 0)
                {
                    Staff staff = Staff.Where(s => s.ID == fault.StaffID).FirstOrDefault();
                    staffName = staff.FirstName + " " + staff.LastName;
                }

                string actualRepairDate = "";

                if (repair.ActualRepairDate != null)
                {
                    actualRepairDate = DateTime.Parse(repair.ActualRepairDate.ToString()).ToString("dd/MM/yyyy");
                }

                //Create a new ExportFaultDataModel object, this is needed for the excel file.
                ExportRepairData = new ExportRepairDataModel
                {
                    ID = repair.ID,
                    Contractor = contractorName,
                    TargetDate = repair.RepairTargetDate.ToString("dd/MM/yyyy"),
                    ActualRepairDate = actualRepairDate,
                    FaultType = faultTypeDescription,
                    FaultPriorityRating = faultPriorityRating,
                    FaultPriority = faultPriorityDescription,
                    Status = Status,
                    RoadNumber = fault.RoadNumber,
                    RoadName = fault.RoadName,
                    RoadTown = fault.RoadTown,
                    RoadCounty = fault.RoadCounty,
                    InputOn = fault.InputOn.ToString("dd/MM/yyyy"),
                    Staff = staffName
                };

                ExportRepairDataList.Add(ExportRepairData);
            }

            //Set the license context to non commercial.
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            //Create an excel file containing the repairs data using memory stream.
            MemoryStream memoryStream = new MemoryStream();
            using (ExcelPackage excelPackage = new ExcelPackage(memoryStream))
            {
                var sheet = excelPackage.Workbook.Worksheets.Add("Sheet1");
                sheet.Cells.LoadFromCollection(ExportRepairDataList, true);
                excelPackage.Save();
            }
            memoryStream.Position = 0;
            string excelName = "Repairs Data Report.xlsx";
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
        }
        #endregion Export Data
    }
}
