using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DFI.FaultReporting.Models.Roles;
using DFI.FaultReporting.SQL.Repository.Contexts;
using DFI.FaultReporting.Common.Pagination;
using DFI.FaultReporting.Interfaces.Admin;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Pagination;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Users;
using DFI.FaultReporting.Services.Interfaces.Roles;
using DFI.FaultReporting.Common.SessionStorage;
using DFI.FaultReporting.Services.Admin;
using System.Security.Claims;

namespace DFI.FaultReporting.Admin.Pages.Admin.RoleAdmin
{
    public class IndexModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<IndexModel> _logger;
        private readonly IRoleService _roleService;
        private readonly IStaffService _staffService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPagerService _pagerService;
        private readonly ISettingsService _settingsService;

        //Inject dependencies in constructor.
        public IndexModel(ILogger<IndexModel> logger, IRoleService roleService, IStaffService staffService, IHttpContextAccessor httpContextAccessor,
            IPagerService pagerService, ISettingsService settingsService)
        {
            _logger = logger;
            _roleService = roleService;
            _staffService = staffService;
            _httpContextAccessor = httpContextAccessor;
            _pagerService = pagerService;
            _settingsService = settingsService;
        }
        #endregion Dependency Injection

        #region Properties
        //Declare CurrentStaff property, this is needed when calling the _staffService.
        public Staff CurrentStaff { get; set; }

        //Declare Roles property, this is needed when getting all roles from the DB.
        [BindProperty]
        public List<Role> Roles { get; set; }

        //Declare PagedRoles property, this is needed for displaying roles in the table.
        [BindProperty]
        public List<Role> PagedRoles { get; set; }

        //Declare Pager property, this is needed for pagination.
        [BindProperty(SupportsGet = true)]
        public Pager Pager { get; set; } = new Pager();
        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is called when the page is loaded.
        //It checks if the current user is authenticated and if so, it gets the current user and all roles from the DB.
        public async Task<IActionResult> OnGetAsync()
        {
            //The contexts current user exists.
            if (_httpContextAccessor.HttpContext.User != null)
            {
                //The contexts current user has been authenticated and has admin role
                if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true && HttpContext.User.IsInRole("StaffAdmin"))
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

                    //Get all roles from the DB.
                    Roles = await _roleService.GetRoles(jwtToken);

                    //Order the roles by ID.
                    Roles = Roles.OrderBy(ft => ft.ID).ToList();

                    //Set the Roles in session.
                    HttpContext.Session.SetInSession("Roles", Roles);

                    //Set the current page to 1.
                    Pager.CurrentPage = 1;

                    //Set the page size to the value from the settings.
                    Pager.PageSize = await _settingsService.GetSettingInt(DFI.FaultReporting.Common.Constants.Settings.PAGESIZE);

                    //Set the pager to the count of roles.
                    Pager.Count = Roles.Count;

                    //Set the PagedRoles property by calling the GetPaginatedRoles method in the _pagerService.
                    PagedRoles = await _pagerService.GetPaginatedRoles(Roles, Pager.CurrentPage, Pager.PageSize);

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

        #region Pagination
        //Method Summary:
        //This method is excuted when the pagination buttons are clicked.
        //When executed the desired page of roles is displayed.
        public async void OnGetPaging()
        {
            //Get the roles from session.
            Roles = HttpContext.Session.GetFromSession<List<Role>>("Roles");

            //Set the pager to the count of roles.
            Pager.Count = Roles.Count;

            //Set the PagedRoles property by calling the GetPaginatedRoles method in the _pagerService.
            PagedRoles = await _pagerService.GetPaginatedRoles(Roles, Pager.CurrentPage, Pager.PageSize);
        }
        #endregion Pagination
    }
}
