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
    public class AssignStaffModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<AssignStaffModel> _logger;
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
        public AssignStaffModel(ILogger<AssignStaffModel> logger, IStaffService staffService, IFaultService faultService, IFaultTypeService faultTypeService,
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

        //Declare Fault property, this is needed for assign staff to the fault.
        [BindProperty]
        public Fault Fault { get; set; }

        //Declare Staff property, this is needed when getting all staff from the DB.
        [BindProperty]
        public List<Staff> Staff { get; set; }

        //Declare StaffToAssign property, this is needed for assign staff to the fault.
        [BindProperty]
        public Staff? StaffToAssign { get; set; }

        //Declare SearchString property, this is needed for searching staff.
        [DisplayName("Search for staff")]
        [BindProperty]
        public string? SearchString { get; set; }

        //Declare SearchID property, this is needed for searching staff.
        [BindProperty]
        public int SearchID { get; set; }

        //Declare UpdateSuccess property, this is needed for displaying the updated success message.
        public bool UpdateSuccess { get; set; }
        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is called when the page is loaded.
        //It checks if the current user is authenticated and if so, it gets the fault details from the DB.
        public async Task<IActionResult> OnGetAsync(int? ID)
        {
            //The contexts current user exists.
            if (_httpContextAccessor.HttpContext.User != null)
            {
                //The contexts current user has been authenticated and has admin role.
                if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true && HttpContext.User.IsInRole("StaffReadWrite"))
                {
                    //Get the ID from the contexts current user, needed for populating CurrentUser property from DB.
                    string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

                    //Get the JWT token claim from the contexts current user, needed for populating CurrentUser property from DB.
                    Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

                    //Set the jwtToken string to the JWT token claims value, needed for populating CurrentUser property from DB.
                    string? jwtToken = jwtTokenClaim.Value;

                    //Set the CurrentStaff property by calling the GetUser method in the _userService.
                    CurrentStaff = await _staffService.GetStaff(Convert.ToInt32(userID), jwtToken);

                    //Get fault from DB.
                    Fault = await _faultService.GetFault((int)ID, jwtToken);

                    //Get staff from DB.
                    Staff = await _staffService.GetAllStaff(jwtToken);

                    //Filter out inactive staff.
                    Staff = Staff.Where(s => s.Active == true).ToList();

                    //The fault has a staff member assigned.
                    if (Fault.StaffID != 0)
                    {
                        //Get the staff member assigned to the fault.
                        Staff faultStaff = Staff.Where(s => s.ID == Fault.StaffID).FirstOrDefault();

                        //Get the staff member assigned to the fault.
                        StaffToAssign = faultStaff;
                    }

                    //Set staff session values.
                    HttpContext.Session.SetInSession("Staff", Staff);
                    //Set Fault session values.
                    HttpContext.Session.SetInSession("Fault", Fault);

                    //Return the page.
                    return Page();
                }
                else
                {
                    //Redirect user to no permission.
                    return Redirect("/NoPermission");
                }
            }
            //The contexts current user has not been authenticated.
            else
            {
                //Redirect user to no permission.
                return Redirect("/NoPermission");
            }
        }
        #endregion Page Load

        #region Assign Staff
        //Method Summary:
        //This method is executed when a search item clicked.
        //When executed, it sets the staff to assign to the staff member selected in the search.
        public async Task<IActionResult> OnPostAssignToSearch()
        {
            //Get the JWT token claim from the contexts current user, needed for populating CurrentUser property from DB.
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

            //Set the jwtToken string to the JWT token claims value, needed for populating CurrentUser property from DB.
            string? jwtToken = jwtTokenClaim.Value;

            //User has entered a search string.
            if (SearchString != null && SearchString != string.Empty)
            {
                //Get staff from DB.
                List<Staff> staff = await _staffService.GetAllStaff(jwtToken);

                //Set the staff to assign to the staff member selected in the search.
                StaffToAssign = staff.Where(s => s.ID == SearchID).FirstOrDefault();

                //Set the StaffToAssign in session.
                HttpContext.Session.SetInSession("StaffToAssign", StaffToAssign);

                //Return the page.
                return Page();
            }
            //User has not entered a search string.
            else
            {
                //Set staff to null.
                StaffToAssign = null;

                return Page();
            }
        }

        //Method Summary:
        //This method is executed when the "Assign to me" button is clicked.
        //When executed, it sets the staff to assign to the current logged in staff member.
        public async Task<IActionResult> OnPostAssignToMe()
        {
            //Get the ID from the contexts current user, needed for populating CurrentUser property from DB.
            string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            //Get the JWT token claim from the contexts current user, needed for populating CurrentUser property from DB.
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

            //Set the jwtToken string to the JWT token claims value, needed for populating CurrentUser property from DB.
            string? jwtToken = jwtTokenClaim.Value;

            //Set the CurrentStaff property by calling the GetUser method in the _userService.
            CurrentStaff = await _staffService.GetStaff(Convert.ToInt32(userID), jwtToken);

            //Set the staff to assign to the current logged in staff member.
            StaffToAssign = CurrentStaff;

            //Set the StaffToAssign in session.
            HttpContext.Session.SetInSession("StaffToAssign", StaffToAssign);

            //Return the page.
            return Page();
        }

        //Method Summary:
        //This method is executed when the "Assign" button is clicked.
        //When executed, it updates the fault with the staff member selected to assign.
        public async Task<IActionResult> OnPostAssignStaff()
        {
            //Get the ID from the contexts current user, needed for populating CurrentUser property from DB.
            string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            //Get the JWT token claim from the contexts current user, needed for populating CurrentUser property from DB.
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

            //Set the jwtToken string to the JWT token claims value, needed for populating CurrentUser property from DB.
            string? jwtToken = jwtTokenClaim.Value;

            //Set the CurrentStaff property by calling the GetUser method in the _userService.
            CurrentStaff = await _staffService.GetStaff(Convert.ToInt32(userID), jwtToken);

            //Get the StaffToAssign from session.
            StaffToAssign = HttpContext.Session.GetFromSession<Staff>("StaffToAssign");

            //Get the Fault from session.
            Fault = HttpContext.Session.GetFromSession<Fault>("Fault");

            //Update the fault with the staff member selected to assign.
            Fault.StaffID = StaffToAssign.ID;
            Fault updatedFault = await _faultService.UpdateFault(Fault, jwtToken);

            //The staff member has been assigned.
            if (updatedFault != null)
            {
                //Set the UpdateSuccess property to true.
                UpdateSuccess = true;

                //Return the page.
                return Page();
            }
            //The staff member has not been assigned.
            else
            {
                //Return an error message.
                ModelState.AddModelError(string.Empty, "An error occurred while assigning the staff member");

                //Return the page.
                return Page();
            }
        }
        #endregion Assign Staff
    }
}
