using DFI.FaultReporting.Common.Pagination;
using DFI.FaultReporting.Common.SessionStorage;
using DFI.FaultReporting.Interfaces.Admin;
using DFI.FaultReporting.Interfaces.FaultReports;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Models.FaultReports;
using DFI.FaultReporting.Models.Roles;
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

namespace DFI.FaultReporting.Public.Pages.Jobs
{
    public class TodaysJobsModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<TodaysJobsModel> _logger;
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
        public TodaysJobsModel(ILogger<TodaysJobsModel> logger, IFaultService faultService, IFaultTypeService faultTypeService,
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

        [BindProperty(SupportsGet = true)]
        public SelectedJobsInputModel SelectedJobsInput { get; set; }

        [BindProperty(SupportsGet = true)]
        public List<SelectedJobsInputModel> AvailableJobs { get; set; }

        [BindProperty]
        public List<SelectedJobsInputModel> SelectedJobs { get; set; }

        public class SelectedJobsInputModel
        {
            [BindProperty]
            public int RepairID { get; set; }

            [BindProperty]
            public bool isSelected { get; set; }
        }

        public bool ShowRouteMap { get; set; }
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
                if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true && HttpContext.User.IsInRole("Contractor"))
                {
                    //Show the repair map by default.
                    ShowRepairMap = true;

                    //Clear session and tempdata to ensure fresh start.
                    HttpContext.Session.Clear();
                    TempData.Clear();

                    await PopulateProperties();

                    //Setup Pager.
                    Pager.CurrentPage = 1;
                    Pager.Count = Repairs.Count;
                    Pager.PageSize = await _settingsService.GetSettingInt(DFI.FaultReporting.Common.Constants.Settings.PAGESIZE);
                    PagedRepairs = await _pagerService.GetPaginatedRepairs(Repairs, Pager.CurrentPage, Pager.PageSize);
                    PagedRepairs = PagedRepairs.OrderBy(r => r.RepairTargetDate).ToList();

                    //Create a new list of AvailableJobs objects.
                    AvailableJobs = new List<SelectedJobsInputModel>();

                    //Loop through all repairs and add them to the AvailableJobs list.
                    foreach (Repair repair in PagedRepairs)
                    {
                        //Create a new SelectedJobsInput object.
                        SelectedJobsInput = new SelectedJobsInputModel();
                        SelectedJobsInput.RepairID = repair.ID;
                        SelectedJobsInput.isSelected = false;

                        //Add the SelectedJobsInput object to the AvailableJobs list.
                        AvailableJobs.Add(SelectedJobsInput);
                    }

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
            HttpContext.Session.SetInSession("FaultTypes", FaultTypes);
            HttpContext.Session.SetInSession("FaultPriorities", FaultPriorities);
            HttpContext.Session.SetInSession("FaultStatuses", FaultStatuses);
            HttpContext.Session.SetInSession("RepairStatuses", RepairStatuses);
            HttpContext.Session.SetInSession("Contractor", Contractor);
        }
        #endregion Data

        #region Pagination
        //Method Summary:
        //This method is excuted when the pagination buttons are clicked.
        //When executed the desired page of faults is displayed.
        public async Task OnGetPaging()
        {
            await PopulateProperties();

            //Hide the repair map.
            ShowRepairMap = false;

            //Show the repair table.
            ShowRepairTable = true;

            //Setup Pager.
            Pager.Count = Repairs.Count;
            Pager.PageSize = await _settingsService.GetSettingInt(DFI.FaultReporting.Common.Constants.Settings.PAGESIZE);
            PagedRepairs = await _pagerService.GetPaginatedRepairs(Repairs, Pager.CurrentPage, Pager.PageSize);
            PagedRepairs = PagedRepairs.OrderBy(r => r.RepairTargetDate).ToList();

            //Create a new list of AvailableJobs objects.
            AvailableJobs = new List<SelectedJobsInputModel>();

            //Loop through all repairs and add them to the AvailableJobs list.
            foreach (Repair repair in PagedRepairs)
            {
                //Create a new SelectedJobsInput object.
                SelectedJobsInput = new SelectedJobsInputModel();
                SelectedJobsInput.RepairID = repair.ID;
                SelectedJobsInput.isSelected = false;

                //Add the SelectedJobsInput object to the AvailableJobs list.
                AvailableJobs.Add(SelectedJobsInput);
            }

            SelectedJobs = HttpContext.Session.GetFromSession<List<SelectedJobsInputModel>>("SelectedJobs");

            foreach (SelectedJobsInputModel selectedJob in SelectedJobs)
            {
                foreach (SelectedJobsInputModel availableJob in AvailableJobs)
                {
                    if (selectedJob.RepairID == availableJob.RepairID && selectedJob.isSelected)
                    {
                        availableJob.isSelected = true;
                    }
                }
            }

            HttpContext.Session.SetInSession("AvailableJobs", AvailableJobs);

            TempData["CurrentPage"] = Pager.CurrentPage;
            TempData.Keep();
        }
        #endregion Pagination

        #region Select Jobs
        //This method is executed when a checkbox is clicked in the jobs table.
        //When executed the Selected Jobs list is updated.
        public async Task<IActionResult> OnPostSelectJobs()
        {
            await PopulateProperties();

            int CurrentPage = 1;

            if (TempData["CurrentPage"] != null && (int)TempData["CurrentPage"] != 0)
            {
                CurrentPage = (int)TempData["CurrentPage"];
            }

            //Setup Pager.
            Pager.Count = Repairs.Count;
            Pager.CurrentPage = CurrentPage;
            Pager.PageSize = await _settingsService.GetSettingInt(DFI.FaultReporting.Common.Constants.Settings.PAGESIZE);
            PagedRepairs = await _pagerService.GetPaginatedRepairs(Repairs, Pager.CurrentPage, Pager.PageSize);
            PagedRepairs = PagedRepairs.OrderBy(r => r.RepairTargetDate).ToList();

            SelectedJobs = HttpContext.Session.GetFromSession<List<SelectedJobsInputModel>>("SelectedJobs");

            if (SelectedJobs == null || SelectedJobs.Count == 0)
            {
                SelectedJobs = new List<SelectedJobsInputModel>();
            }

            if (AvailableJobs == null || AvailableJobs.Count == 0)
            {
                //Create a new list of AvailableJobs objects.
                AvailableJobs = new List<SelectedJobsInputModel>();
                AvailableJobs = new List<SelectedJobsInputModel>();

                //Loop through all repairs and add them to the AvailableJobs list.
                foreach (Repair repair in PagedRepairs)
                {
                    //Create a new SelectedJobsInput object.
                    SelectedJobsInput = new SelectedJobsInputModel();
                    SelectedJobsInput.RepairID = repair.ID;
                    SelectedJobsInput.isSelected = false;

                    //Add the SelectedJobsInput object to the AvailableJobs list.
                    AvailableJobs.Add(SelectedJobsInput);
                }
            }

            foreach (SelectedJobsInputModel availableJob in AvailableJobs)
            {
                if (availableJob.isSelected)
                {
                    SelectedJobsInputModel jobToAdd = SelectedJobs.Where(sj => sj.RepairID == availableJob.RepairID).FirstOrDefault();

                    if (jobToAdd == null)
                    {
                        SelectedJobsInputModel selectedJob = new SelectedJobsInputModel();
                        selectedJob.RepairID = availableJob.RepairID;
                        selectedJob.isSelected = true;

                        SelectedJobs.Add(selectedJob);
                    }
                }
                else
                {
                    SelectedJobsInputModel jobToRemove = SelectedJobs.Where(sj => sj.RepairID == availableJob.RepairID).FirstOrDefault();

                    if (jobToRemove != null)
                    {
                        SelectedJobs.Remove(jobToRemove);
                    }
                }
            }

            foreach (SelectedJobsInputModel selectedJob in SelectedJobs)
            {
                foreach (SelectedJobsInputModel availableJob in AvailableJobs)
                {
                    if (selectedJob.RepairID == availableJob.RepairID && selectedJob.isSelected)
                    {
                        availableJob.isSelected = true;
                    }
                }
            }

            HttpContext.Session.SetInSession("AvailableJobs", AvailableJobs);
            HttpContext.Session.SetInSession("SelectedJobs", SelectedJobs);

            TempData.Keep();

            return Page();
        }
        #endregion Select Jobs

        #region Plot Routes
        //Method Summary:
        //This method is excuted when the plot route button is clicked.
        //When executed the repairs are sorted by priority, the map is shown and the route is plotted.
        public async Task OnGetPlotRoute()
        {
            ShowRouteMap = true;

            await PopulateProperties();

            int CurrentPage = 1;

            if (TempData["CurrentPage"] != null && (int)TempData["CurrentPage"] != 0)
            {
                CurrentPage = (int)TempData["CurrentPage"];
            }

            //Setup Pager.
            Pager.Count = Repairs.Count;
            Pager.CurrentPage = CurrentPage;
            Pager.PageSize = await _settingsService.GetSettingInt(DFI.FaultReporting.Common.Constants.Settings.PAGESIZE);
            PagedRepairs = await _pagerService.GetPaginatedRepairs(Repairs, Pager.CurrentPage, Pager.PageSize);
            PagedRepairs = PagedRepairs.OrderBy(r => r.RepairTargetDate).ToList();

            SelectedJobs = HttpContext.Session.GetFromSession<List<SelectedJobsInputModel>>("SelectedJobs");

            List<Repair> SelectedRepairs = new List<Repair>();

            foreach (SelectedJobsInputModel selectedJob in SelectedJobs)
            {
                foreach (Repair repair in Repairs)
                {
                    if (selectedJob.RepairID == repair.ID)
                    {
                        SelectedRepairs.Add(repair);
                    }
                }
            }

            List<Fault> selectedFaults = new List<Fault>();

            foreach (Fault fault in Faults)
            {
                foreach (Repair repair in SelectedRepairs)
                {
                    if (fault.ID == repair.FaultID)
                    {
                        selectedFaults.Add(fault);
                    }
                }
            }

            selectedFaults = selectedFaults.OrderBy(f => f.FaultPriorityID).ToList();

            HttpContext.Session.SetInSession("SelectedRepairs", SelectedRepairs);
            HttpContext.Session.SetInSession("SelectedFaults", selectedFaults);

            await SetSessionData();
        }
        #endregion Plot Routes
    }
}
