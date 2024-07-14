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
using static DFI.FaultReporting.Public.Pages.Jobs.TodaysJobsModel;

namespace DFI.FaultReporting.Public.Pages.Jobs
{
    public class ViewRouteMapModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<ViewRouteMapModel> _logger;
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
        public ViewRouteMapModel(ILogger<ViewRouteMapModel> logger, IFaultService faultService, IFaultTypeService faultTypeService,
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
        public List<Fault> Faults { get; set; }

        [BindProperty]
        public List<Fault> PagedFaults { get; set; }

        [BindProperty(SupportsGet = true)]
        public Pager Pager { get; set; } = new Pager();

        [BindProperty]
        public List<FaultPriority> FaultPriorities { get; set; }


        [BindProperty]
        public List<FaultStatus> FaultStatuses { get; set; }

        [BindProperty]
        public List<FaultType> FaultTypes { get; set; }
        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is executed when the page loads.
        //When executed the user is checked for authentication and role, the required data is retrieved from session.
        public async Task<IActionResult> OnGetAsync()
        {
            //The contexts current user exists.
            if (_httpContextAccessor.HttpContext.User != null)
            {
                //The contexts current user has been authenticated.
                if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true && HttpContext.User.IsInRole("Contractor"))
                {
                    //Get the selected jobs from session.
                    Faults = HttpContext.Session.GetFromSession<List<Fault>>("SelectedFaults");

                    //Setup Pager.
                    Pager.CurrentPage = 1;
                    Pager.Count = Faults.Count;
                    Pager.PageSize = await _settingsService.GetSettingInt(DFI.FaultReporting.Common.Constants.Settings.PAGESIZE);
                    PagedFaults = await _pagerService.GetPaginatedFaults(Faults, Pager.CurrentPage, Pager.PageSize);
                    //PagedFaults = PagedFaults.OrderBy(f => f.FaultPriorityID).ToList();

                    await PopulateProperties();

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
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
            string? jwtToken = jwtTokenClaim.Value;

            FaultTypes = await _faultTypeService.GetFaultTypes();
            FaultTypes = FaultTypes.Where(ft => ft.Active == true).ToList();

            FaultPriorities = await _faultPriorityService.GetFaultPriorities();
            FaultPriorities = FaultPriorities.Where(fp => fp.Active == true).ToList();

            FaultStatuses = await _faultStatusService.GetFaultStatuses();
            FaultStatuses = FaultStatuses.Where(fs => fs.Active == true).ToList();
        }
        #endregion Data

        #region Pagination
        //Method Summary:
        //This method is excuted when the pagination buttons are clicked.
        //When executed the desired page of seleced jobs is displayed.
        public async Task OnGetPaging()
        {
            //Get the selected jobs from session.
            Faults = HttpContext.Session.GetFromSession<List<Fault>>("SelectedFaults");

            await PopulateProperties();

            //Setup Pager.
            Pager.Count = Faults.Count;
            Pager.PageSize = await _settingsService.GetSettingInt(DFI.FaultReporting.Common.Constants.Settings.PAGESIZE);
            PagedFaults = await _pagerService.GetPaginatedFaults(Faults, Pager.CurrentPage, Pager.PageSize);
            //PagedFaults = PagedFaults.OrderBy(f => f.FaultPriorityID).ToList();
        }
        #endregion Pagination
    }
}
