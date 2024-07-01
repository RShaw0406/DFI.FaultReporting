using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.SQL.Repository.Contexts;
using DFI.FaultReporting.Interfaces.Admin;
using DFI.FaultReporting.Interfaces.FaultReports;
using DFI.FaultReporting.Services.Interfaces.Admin;
using DFI.FaultReporting.Services.Interfaces.Emails;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Tokens;
using DFI.FaultReporting.Services.Interfaces.Users;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Models.FaultReports;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using DFI.FaultReporting.Services.Users;
using System.Security.Claims;
using DFI.FaultReporting.Common.Pagination;
using NuGet.Protocol.Plugins;
using DFI.FaultReporting.Services.Interfaces.Pagination;
using DFI.FaultReporting.Services.Interfaces.Roles;
using DFI.FaultReporting.Models.Roles;
using DFI.FaultReporting.Services.Admin;
using DFI.FaultReporting.Services.FaultReports;
using DocumentFormat.OpenXml.Bibliography;
using DFI.FaultReporting.Common.SessionStorage;

namespace DFI.FaultReporting.Admin.Pages.Admin.StaffAdmin
{
    public class IndexModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<IndexModel> _logger;
        private readonly IStaffService _staffService;
        private readonly IRoleService _roleService;
        private readonly IStaffRoleService _staffRoleService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISettingsService _settingsService;
        private readonly IEmailService _emailService;
        private readonly IPagerService _pagerService;

        //Inject dependencies in constructor.
        public IndexModel(ILogger<IndexModel> logger, IStaffService staffService, IRoleService roleService, IStaffRoleService staffRoleService,
            IHttpContextAccessor httpContextAccessor, ISettingsService settingsService, IEmailService emailService, IPagerService pagerService)
        {
            _logger = logger;
            _staffService = staffService;
            _roleService = roleService;
            _staffRoleService = staffRoleService;
            _httpContextAccessor = httpContextAccessor;
            _settingsService = settingsService;
            _emailService = emailService;
            _pagerService = pagerService;
        }
        #endregion Dependency Injection

        #region Properties
        //Declare CurrentStaff property, this is needed when calling the _staffService.
        public Staff CurrentStaff { get; set; }

        //Declare Staff property, this is needed when getting all staff from the DB.
        [BindProperty]
        public List<Staff> Staff { get; set; }

        //Declare PagedStaff property, this is needed for displaying staff in the table.
        [BindProperty]
        public List<Staff> PagedStaff { get; set; }

        //Declare Roles property, this is needed for displaying roles in the dropdown.
        [BindProperty]
        public List<Role> Roles { get; set; }

        //Declare RolesList property, this is needed for displaying roles in the dropdown.
        [BindProperty]
        public IEnumerable<SelectListItem> RolesList { get; set; }

        //Declare RoleFilter property, this is needed for filtering staff by assigned roles.
        [DisplayName("Role")]
        [BindProperty]
        public int RoleFilter { get; set; }

        //Declare StaffRoles property, this is needed for filtering staff by assigned roles.
        [BindProperty]
        public List<StaffRole> StaffRoles { get; set; }

        //Declare AccountLockedFilter property, this is needed for filtering staff by account locked status.
        [DisplayName("Account locked")]
        [BindProperty]
        public int AccountLockedFilter { get; set; }

        [DisplayName("Search")]
        [BindProperty]
        public string SearchString { get; set; }

        //Declare Pager property, this is needed for pagination.
        [BindProperty(SupportsGet = true)]
        public Pager Pager { get; set; } = new Pager();
        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is called when the page is loaded.
        //It checks if the current user is authenticated and if so, it gets the current user and all staff from the DB.
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

                    //Set the CurrentStaff property by calling the GetUser method in the _userService.
                    CurrentStaff = await _staffService.GetStaff(Convert.ToInt32(userID), jwtToken);

                    //Clear session to ensure fresh start.
                    HttpContext.Session.Clear();

                    //Clear TempData to ensure fresh start.
                    TempData.Clear();

                    //Get all staff from the DB.
                    Staff = await _staffService.GetAllStaff(jwtToken);

                    //Filter out inactive staff.
                    Staff = Staff.Where(s => s.Active == true).ToList();

                    //Order the staff by last name.
                    Staff = Staff.OrderByDescending(s => s.LastName).ToList();

                    //Set the current page to 1.
                    Pager.CurrentPage = 1;

                    //Set the page size to the value from the settings.
                    Pager.PageSize = await _settingsService.GetSettingInt(DFI.FaultReporting.Common.Constants.Settings.PAGESIZE);

                    //Set the pager count to the number of staff.
                    Pager.Count = Staff.Count;

                    //Get the paginated staff by calling the GetPaginatedStaff method from the _pagerService.
                    PagedStaff = await _pagerService.GetPaginatedStaff(Staff, Pager.CurrentPage, Pager.PageSize);

                    //Get all roles from the DB.
                    Roles = await _roleService.GetRoles(jwtToken);

                    //Filter out roles that are not staff roles.
                    Roles = Roles.Where(r => r.RoleDescription.Contains("Staff")).ToList();

                    //Populate the roles dropdown.
                    RolesList = Roles.Select(r => new SelectListItem
                    {
                        Value = r.ID.ToString(),
                        Text = r.RoleDescription
                    });

                    //Set session values.
                    HttpContext.Session.SetInSession("Staff", Staff);
                    HttpContext.Session.SetInSession("Roles", Roles);

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
            //The contexts current user has not been authenticated.
            else
            {
                //Redirect user to no permission.
                return Redirect("/NoPermission");
            }
        }
        #endregion Page Load

        #region Dropdown Filter Change
        //This method is executed when either of the dropdown filters are changed.
        //When executed the Faults property is filtered based on the selected filter.
        public async Task<IActionResult> OnPost()
        {
            //Get all required data from session.
            GetSessionData();

            //Get the JWT token claim from the contexts current user, needed for populating CurrentUser property from DB.
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

            //Set the jwtToken string to the JWT token claims value, needed for populating CurrentUser property from DB.
            string? jwtToken = jwtTokenClaim.Value;

            if (SearchString != null && SearchString != string.Empty)
            {
                Staff = Staff.Where(s => s.FirstName + " " + s.LastName == SearchString).ToList();
            }

            //User has selected a role.
            if (RoleFilter != 0)
            {
                //Declare a new list of staff.
                List<Staff> StaffInRole = new List<Staff>();

                //Get all staff roles by calling the GetStaffRoles method from the _staffRoleService.
                StaffRoles = await _staffRoleService.GetStaffRoles(jwtToken);

                //Loop through each staff role and get the staff for each role.
                foreach (StaffRole staffRole in StaffRoles)
                {
                    //Check if the staff role has the selected role.
                    if (staffRole.RoleID == RoleFilter)
                    {
                        //Get the staff by calling the GetStaff method from the _staffService.
                        Staff staff = await _staffService.GetStaff(staffRole.StaffID, jwtToken);

                        //Add the staff to the Staff list.
                        StaffInRole.Add(staff);
                    }
                }

                if (SearchString != null && SearchString != string.Empty)
                {
                    Staff = new List<Staff>();

                    foreach (Staff staff in StaffInRole)
                    {
                        if (staff.FirstName + " " + staff.LastName == SearchString)
                        {
                            Staff.Add(staff);
                        }
                    }
                }
                else
                {
                    //Set the Staff list to the StaffInRole list.
                    Staff = StaffInRole;
                }
            }

            //User has selected all roles.
            //else
            //{
            //    //Get all staff from the DB.
            //    Staff = await _staffService.GetAllStaff(jwtToken);              
            //}

            //User has selected a account locked status.
            if (AccountLockedFilter != 0)
            {
                //User has selected to view locked staff.
                if (AccountLockedFilter == 1)
                {
                    //Filter out unlocked staff.
                    Staff = Staff.Where(s => s.AccountLocked == true).ToList();
                }
                //User has selected to view unlocked staff.
                else
                {
                    //Filter out locked staff.
                    Staff = Staff.Where(s => s.AccountLocked == false).ToList();
                }
            }

            //Filter out inactive staff.
            Staff = Staff.Where(s => s.Active == true).ToList();

            //Order the staff by last name.
            Staff = Staff.OrderByDescending(s => s.LastName).ToList();

            //Get the paged staff by calling the GetPaginatedStaff method from the _pagerService.
            PagedStaff = await _pagerService.GetPaginatedStaff(Staff, Pager.CurrentPage, Pager.PageSize);

            //Set the selected role in TempData.
            TempData["RoleFilter"] = RoleFilter;

            //Set the selected account locked status in TempData.
            TempData["AccountLockedFilter"] = AccountLockedFilter;

            //Keep the TempData.
            TempData.Keep();

            //Set the Faults in session, needed for displaying on map.
            //HttpContext.Session.SetInSession("Staff", Staff);

            //Return the page and show the map, this is needed to ensure the paging is reset.
            return Page();
        }
        #endregion Dropdown Filter Change

        #region Pagination
        //Method Summary:
        //This method is excuted when the pagination buttons are clicked.
        //When executed the desired page of staff is displayed.
        public async void OnGetPaging()
        {
            //Set the RoleFilter value to the value from TempData.
            if (TempData["RoleFilter"] != null)
            {
                RoleFilter = (int)TempData["RoleFilter"];
            }

            //Set the AccountLockedFilter value to the value from TempData.
            if (TempData["AccountLockedFilter"] != null)
            {
                AccountLockedFilter = (int)TempData["AccountLockedFilter"];
            }

            //Keep the TempData.
            TempData.Keep();

            //Get all required data from session.
            GetSessionData();

            //Set the pager count to the number of staff.
            Pager.Count = Staff.Count;

            //Get the first page of staff by calling the GetPaginatedStaff method from the _pagerService.
            PagedStaff = await _pagerService.GetPaginatedStaff(Staff, Pager.CurrentPage, Pager.PageSize);
        }
        #endregion Pagination

        #region Session Data
        //Method Summary:
        //This method is excuted when any of filter options change or any of the pagination buttons are clicked.
        //When executed the required data is retrieved from session.
        public async void GetSessionData()
        {
            //Get the staff from session.
            Staff = HttpContext.Session.GetFromSession<List<Staff>>("Staff");

            //Get the roles from session.
            Roles = HttpContext.Session.GetFromSession<List<Role>>("Roles");

            //Populate the roles dropdown.
            RolesList = Roles.Select(r => new SelectListItem
            {
                Value = r.ID.ToString(),
                Text = r.RoleDescription
            });
        }
        #endregion Session Data
    }
}