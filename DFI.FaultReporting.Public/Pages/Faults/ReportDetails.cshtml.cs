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

namespace DFI.FaultReporting.Public.Pages.Faults
{
    public class ReportDetailsModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<ReportDetailsModel> _logger;
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
        public ReportDetailsModel(ILogger<ReportDetailsModel> logger, IUserService userService, IFaultService faultService, IFaultTypeService faultTypeService,
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

        //Declare Faults property, this is needed for displaying on fault details.
        [BindProperty]
        public Fault Fault { get; set; }

        //Declare Reports property, this is needed for displaying the number of reports for each fault.
        [BindProperty]
        public List<Report> Reports { get; set; }

        //Declare PagedReports property, this is needed for displaying reports in the table.
        [BindProperty]
        public List<Report> PagedReports { get; set; }

        //Declare FaultPriority property, this is needed for displaying on fault details.
        [BindProperty]
        public FaultPriority FaultPriority { get; set; }

        //Declare FaultStatus property, this is needed for displaying on fault details.
        [BindProperty]
        public FaultStatus FaultStatus { get; set; }

        //Declare FaultType property, this is needed for displaying on fault details.
        [BindProperty]
        public FaultType FaultType { get; set; }

        //Declare Pager property, this is needed for pagination.
        [BindProperty(SupportsGet = true)]
        public Pager Pager { get; set; } = new Pager();
        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is executed when the page loads.
        //When executed the details of the reported fault selected by the user are populated and displayed on screen.
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

                    //Retreive the ID in the url query string.
                    string? ID = HttpContext.Request.Query["ID"].ToString();

                    //User has arrived on page by clicking link in map.
                    if (ID != null && ID != string.Empty)
                    {
                        //Set the Fault property by calling the GetFault method from the _faultService using the query string ID.
                        Fault = await _faultService.GetFault(Convert.ToInt32(ID), jwtToken);
                    }
                    //User has arrived on page by clicking link in table.
                    else
                    {
                        //Set the Fault property by calling the GetFault method from the _faultService using the ID in TempData.
                        Fault = await _faultService.GetFault(Convert.ToInt32(TempData["FaultID"]), jwtToken);
                    }

                    //Keep the TempData.
                    TempData.Keep();

                    //Set the FaultPriority property by calling the GetFaultPriority method from the _faultPriorityService.
                    FaultType = await _faultTypeService.GetFaultType(Fault.FaultTypeID, jwtToken);

                    //Set the FaultStatus property by calling the GetFaultStatus method from the _faultStatusService.
                    FaultStatus = await _faultStatusService.GetFaultStatus(Fault.FaultStatusID, jwtToken);

                    //Set the FaultPriority property by calling the GetFaultPriority method from the _faultPriorityService.
                    FaultPriority = await _faultPriorityService.GetFaultPriority(Fault.FaultPriorityID, jwtToken);

                    //Get all Reports by calling the GetReports method from the _reportService.
                    Reports = await _reportService.GetReports();

                    //Get the reports for the current user.
                    Reports = Reports.Where(r => r.UserID == CurrentUser.ID && r.FaultID == Fault.ID).ToList();

                    //Set the current page to 1.
                    Pager.CurrentPage = 1;

                    //Set the page size to the value from the settings.
                    Pager.PageSize = await _settingsService.GetSettingInt(DFI.FaultReporting.Common.Constants.Settings.PAGESIZE);

                    //Set the pager count to the number of reports.
                    Pager.Count = Reports.Count;

                    //Get the paginated reports by calling the GetPaginatedReports method from the _pagerService.
                    PagedReports = await _pagerService.GetPaginatedReports(Reports, Pager.CurrentPage, Pager.PageSize);

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

        #region View Images
        //Method Summary:
        //This method is excuted when the "View Images" button in the reports table view is clicked.
        //When executed the report ID is stored in TempData and the user is redirected to the ReportImages page.
        public async Task<IActionResult> OnGetViewImages(int ID)
        {
            //Set the report ID in TempData.
            TempData["ReportID"] = ID;
            TempData.Keep();

            //Redirect user to the ReportImages page.
            return Redirect("/Faults/ReportImages");
        }
        #endregion View Images

        #region Pagination
        //Method Summary:
        //This method is excuted when the pagination buttons are clicked.
        //When executed the desired page of reports is displayed.
        public async void OnGetPaging()
        {
            //Keep the TempData.
            TempData.Keep();

            //Set the pager count to the number of reports.
            Pager.Count = Reports.Count;

            //Get the first page of reports by calling the GetPaginatedReports method from the _pagerService.
            PagedReports = await _pagerService.GetPaginatedReports(Reports, Pager.CurrentPage, Pager.PageSize);
        }
        #endregion Pagination
    }
}
