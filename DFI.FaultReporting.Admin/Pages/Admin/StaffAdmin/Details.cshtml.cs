using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.SQL.Repository.Contexts;
using DFI.FaultReporting.Services.Interfaces.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Security.Claims;

namespace DFI.FaultReporting.Admin.Pages.Admin.StaffAdmin
{
    public class DetailsModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<DetailsModel> _logger;
        private readonly IStaffService _staffService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        //Inject dependencies in constructor.
        public DetailsModel(ILogger<DetailsModel> logger, IStaffService staffService, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _staffService = staffService;
            _httpContextAccessor = httpContextAccessor;
        }
        #endregion Dependency Injection

        #region Properties
        //Declare CurrentStaff property, this is needed when calling the _staffService.
        public Staff CurrentStaff { get; set; }

        //Declare Staff property, this is needed when getting staff member from the DB.
        [BindProperty]
        public Staff Staff { get; set; }
        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is called when the page is loaded.
        //It checks if the current user is authenticated and if so, it gets the staff details from the DB.
        public async Task<IActionResult> OnGetAsync(int? ID)
        {
            //The contexts current user exists.
            if (_httpContextAccessor.HttpContext.User != null)
            {
                //The contexts current user has been authenticated and has admin role.
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

                    //Get staff from the DB.
                    Staff = await _staffService.GetStaff((int)ID, jwtToken);

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
    }
}
