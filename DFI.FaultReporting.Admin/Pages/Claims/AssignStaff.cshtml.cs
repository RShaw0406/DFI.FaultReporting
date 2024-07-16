using DFI.FaultReporting.Interfaces.Admin;
using DFI.FaultReporting.Interfaces.Claims;
using DFI.FaultReporting.Interfaces.FaultReports;
using DFI.FaultReporting.Models.Claims;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.FaultReports;
using DFI.FaultReporting.Services.Interfaces.Admin;
using DFI.FaultReporting.Services.Interfaces.Pagination;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.Security.Claims;
using DFI.FaultReporting.Common.SessionStorage;

namespace DFI.FaultReporting.Admin.Pages.Claims
{
    public class AssignStaffModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<AssignStaffModel> _logger;
        private readonly IStaffService _staffService;
        private readonly IClaimService _claimService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPagerService _pagerService;
        private readonly ISettingsService _settingsService;

        //Inject dependencies in constructor.
        public AssignStaffModel(ILogger<AssignStaffModel> logger, IStaffService staffService, IClaimService claimService,
            IHttpContextAccessor httpContextAccessor, IPagerService pagerService, ISettingsService settingsService)
        {
            _logger = logger;
            _staffService = staffService;
            _claimService = claimService;
            _httpContextAccessor = httpContextAccessor;
            _pagerService = pagerService;
            _settingsService = settingsService;
        }
        #endregion Dependency Injection

        #region Properties
        public Staff CurrentStaff { get; set; }

        [BindProperty]
        public Models.Claims.Claim Claim { get; set; }

        [BindProperty]
        public List<Staff> Staff { get; set; }

        [BindProperty]
        public Staff? StaffToAssign { get; set; }

        [DisplayName("Search for staff")]
        [BindProperty]
        public string? SearchString { get; set; }

        [BindProperty]
        public int SearchID { get; set; }

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
                    await PopulateProperties((int)ID);

                    //Set Staff session value, this is needed for the javascript search.
                    HttpContext.Session.SetInSession("Staff", Staff);

                    //Set the ID in TempData.
                    TempData["ID"] = ID;
                    TempData.Keep();

                    //The claim has a staff member assigned.
                    if (Claim.StaffID != 0)
                    {
                        //Get the staff member assigned to the claim.
                        Staff claimStaff = Staff.Where(s => s.ID == Claim.StaffID).FirstOrDefault();
                        StaffToAssign = claimStaff;
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

        #region Assign Staff
        //Method Summary:
        //This method is executed when a search item clicked.
        //When executed, it sets the staff to assign to the staff member selected in the search.
        public async Task<IActionResult> OnPostAssignToSearch()
        {
            await PopulateProperties((int)TempData["ID"]);

            //User has entered a search string.
            if (SearchString != null && SearchString != string.Empty)
            {
                //Set the staff to assign to the staff member selected in the search.
                StaffToAssign = Staff.Where(s => s.ID == SearchID).FirstOrDefault();

                //Set the StaffToAssign in session.
                HttpContext.Session.SetInSession("StaffToAssign", StaffToAssign);

                TempData.Keep();

                return Page();
            }
            else
            {
                StaffToAssign = null;

                TempData.Keep();

                return Page();
            }
        }

        //Method Summary:
        //This method is executed when the "Assign to me" button is clicked.
        //When executed, it sets the staff to assign to the current logged in staff member.
        public async Task<IActionResult> OnPostAssignToMe()
        {
            await PopulateProperties((int)TempData["ID"]);

            //Set the staff to assign to the current logged in staff member.
            StaffToAssign = CurrentStaff;

            //Set the StaffToAssign in session.
            HttpContext.Session.SetInSession("StaffToAssign", StaffToAssign);

            TempData.Keep();

            return Page();
        }

        //Method Summary:
        //This method is executed when the "Assign" button is clicked.
        //When executed, it updates the fault with the staff member selected to assign.
        public async Task<IActionResult> OnPostAssignStaff()
        {
            await PopulateProperties((int)TempData["ID"]);

            //Get the StaffToAssign from session.
            StaffToAssign = HttpContext.Session.GetFromSession<Staff>("StaffToAssign");

            //Update the claim with the staff member selected to assign.
            Claim.StaffID = StaffToAssign.ID;
            System.Security.Claims.Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
            string? jwtToken = jwtTokenClaim.Value;
            Models.Claims.Claim updatedClaim = await _claimService.UpdateClaim(Claim, jwtToken);

            //The staff member has been assigned.
            if (updatedClaim != null)
            {
                UpdateSuccess = true;

                TempData.Keep();

                return Page();
            }
            else
            {
                ModelState.AddModelError(string.Empty, "An error occurred while assigning the staff member");

                TempData.Keep();

                return Page();
            }
        }
        #endregion Assign Staff

        #region Data
        //Method Summary:
        //This method is excuted when the a post occurs.
        //When excuted, it populates the page properties.
        public async Task PopulateProperties(int ID)
        {
            //Get the current user ID and JWT token.
            string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            System.Security.Claims.Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
            string? jwtToken = jwtTokenClaim.Value;
            CurrentStaff = await _staffService.GetStaff(Convert.ToInt32(userID), jwtToken);

            Staff = await _staffService.GetAllStaff(jwtToken);
            Staff = Staff.Where(s => s.Active == true).ToList();

            Claim = await _claimService.GetClaim((int)ID, jwtToken);
        }
        #endregion Data
    }
}
