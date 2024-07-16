using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DFI.FaultReporting.Models.Users;
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
        public Staff CurrentStaff { get; set; }

        [BindProperty]
        public List<Staff> Staff { get; set; }

        [BindProperty]
        public List<Staff> PagedStaff { get; set; }

        [BindProperty]
        public List<Role> Roles { get; set; }

        [BindProperty]
        public IEnumerable<SelectListItem> RolesList { get; set; }

        [DisplayName("Role")]
        [BindProperty]
        public int RoleFilter { get; set; }

        [BindProperty]
        public List<StaffRole> StaffRoles { get; set; }

        [DisplayName("Account locked")]
        [BindProperty]
        public int AccountLockedFilter { get; set; }

        [DisplayName("Search")]
        [BindProperty]
        public string SearchString { get; set; }

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
                //The contexts current user has been authenticated and has admin role
                if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true && HttpContext.User.IsInRole("StaffAdmin"))
                {
                    //Clear session to ensure fresh start.
                    HttpContext.Session.Clear();

                    await PopulateProperties();

                    await SetSessionData();

                    //Setup the Pager.
                    Pager.CurrentPage = 1;
                    Pager.PageSize = await _settingsService.GetSettingInt(DFI.FaultReporting.Common.Constants.Settings.PAGESIZE);
                    Pager.Count = Staff.Count;
                    PagedStaff = await _pagerService.GetPaginatedStaff(Staff, Pager.CurrentPage, Pager.PageSize);

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
        public async Task<IActionResult> OnPost()
        {
            await PopulateProperties();

            //Get the JWT token claim from the contexts current user.
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
            string? jwtToken = jwtTokenClaim.Value;

            //-------------------- SEARCH FILTER --------------------
            if (SearchString != null && SearchString != string.Empty)
            {
                Staff = Staff.Where(s => s.FirstName + " " + s.LastName == SearchString).ToList();
            }

            //-------------------- ROLE FILTER --------------------
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

                //User has entered a search string.
                if (SearchString != null && SearchString != string.Empty)
                {
                    //Declare a new list of staff.
                    Staff = new List<Staff>();

                    //Loop through each staff in the StaffInRole list.
                    foreach (Staff staff in StaffInRole)
                    {
                        //Check if the staffs first and last name matches the search string.
                        if (staff.FirstName + " " + staff.LastName == SearchString)
                        {
                            //Add the staff to the Staff list.
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

            //-------------------- ACCOUNT LOCKED FILTER --------------------
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

            //Order the staff by last name.
            Staff = Staff.OrderByDescending(s => s.LastName).ToList();

            //Setup pager for table.
            Pager.Count = Staff.Count;
            Pager.PageSize = await _settingsService.GetSettingInt(DFI.FaultReporting.Common.Constants.Settings.PAGESIZE);
            PagedStaff = await _pagerService.GetPaginatedStaff(Staff, Pager.CurrentPage, Pager.PageSize);
            PagedStaff = PagedStaff.OrderBy(s => s.ID).ToList();

            await SetSessionData();

            //Set the selected role and account locked status in TempData.
            TempData["RoleFilter"] = RoleFilter;
            TempData["AccountLockedFilter"] = AccountLockedFilter;
            TempData.Keep();

            return Page();
        }
        #endregion Dropdown Filter Change

        #region Pagination
        //Method Summary:
        //This method is excuted when the pagination buttons are clicked.
        //When executed the desired page of staff is displayed.
        public async Task OnGetPaging()
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

            await PopulateProperties();

            //Setup the Pager.
            Pager.Count = Staff.Count;
            Pager.PageSize = await _settingsService.GetSettingInt(DFI.FaultReporting.Common.Constants.Settings.PAGESIZE);
            PagedStaff = await _pagerService.GetPaginatedStaff(Staff, Pager.CurrentPage, Pager.PageSize);
        }
        #endregion Pagination

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

            Staff = await _staffService.GetAllStaff(jwtToken);
            Staff = Staff.OrderByDescending(s => s.LastName).ToList();

            Roles = await _roleService.GetRoles(jwtToken);
            Roles = Roles.Where(r => r.RoleDescription.Contains("Staff")).ToList();
            Roles = Roles.OrderBy(r => r.ID).ToList();

            RolesList = Roles.Select(r => new SelectListItem
            {
                Value = r.ID.ToString(),
                Text = r.RoleDescription
            });
        }

        //Method Summary:
        //This method is executed when a post occurs.
        //When executed, the page properties required in the charts javascript are set in session storage.
        public async Task SetSessionData()
        {
            HttpContext.Session.SetInSession("Roles", Roles);
            HttpContext.Session.SetInSession("Staff", Staff);
        }
        #endregion Data
    }
}