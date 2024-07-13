using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.SQL.Repository.Contexts;
using DFI.FaultReporting.Services.Interfaces.Emails;
using DFI.FaultReporting.Services.Interfaces.Passwords;
using DFI.FaultReporting.Services.Interfaces.Roles;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Users;
using DFI.FaultReporting.Models.Roles;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Security.Claims;

namespace DFI.FaultReporting.Admin.Pages.Admin.StaffAdmin
{
    public class EditModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<EditModel> _logger;
        private readonly IStaffService _staffService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        //Inject dependencies in constructor.
        public EditModel(ILogger<EditModel> logger, IStaffService staffService, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _staffService = staffService;
            _httpContextAccessor = httpContextAccessor;
        }
        #endregion Dependency Injection

        #region Properties
        public Staff CurrentStaff { get; set; }

        [BindProperty]
        public StaffInputModel StaffInput { get; set; }

        public Staff Staff { get; set; }

        public bool UpdateSuccess { get; set; }

        public class StaffInputModel
        {
            public int ID { get; set; }

            [Required(ErrorMessage = "You must enter an email address")]
            [DisplayName("Email address")]
            [DataType(DataType.EmailAddress, ErrorMessage = "You must enter a valid email address")]
            public string? Email { get; set; }

            [Required(ErrorMessage = "You must enter a title")]
            [DisplayName("Title")]
            [RegularExpression(@"^[a-zA-Z''-'\s]{1,8}$", ErrorMessage = "Title must not contain special characters or numbers")]
            [StringLength(8, ErrorMessage = "Title must not be more than 8 characters")]
            public string? Prefix { get; set; }

            [Required(ErrorMessage = "You must enter a first name")]
            [DisplayName("First name")]
            [RegularExpression(@"^[a-zA-Z''-'\s]{1,125}$", ErrorMessage = "First name must not contain special characters or numbers")]
            [StringLength(125, ErrorMessage = "First name must not be more than 125 characters")]
            public string? FirstName { get; set; }

            [Required(ErrorMessage = "You must enter a last name")]
            [DisplayName("Last name")]
            [RegularExpression(@"^[a-zA-Z''-'\s]{1,125}$", ErrorMessage = "Last name must not contain special characters or numbers")]
            [StringLength(125, ErrorMessage = "Last name must not be more than 125 characters")]
            public string? LastName { get; set; }

            [Required(ErrorMessage = "You must provide an active")]
            public bool Active { get; set; }
        }
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
                    //Clear session to ensure fresh start.
                    HttpContext.Session.Clear();

                    await PopulateProperties();

                    //Get staff from the DB.
                    Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
                    string? jwtToken = jwtTokenClaim.Value;
                    Staff = await _staffService.GetStaff((int)ID, jwtToken);

                    //Set the StaffInput property with the values from the Staff property.
                    StaffInput = new StaffInputModel();
                    StaffInput.ID = Staff.ID;
                    StaffInput.Email = Staff.Email;
                    StaffInput.Prefix = Staff.Prefix;
                    StaffInput.FirstName = Staff.FirstName;
                    StaffInput.LastName = Staff.LastName;
                    StaffInput.Active = Staff.Active;

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

        #region Edit Staff
        //Method Summary:
        //This method is executed when the update button is clicked.
        //When executed, it updates the staff member in the DB.
        public async Task<IActionResult> OnPostAsync()
        {
            //Modelstate is not valid.
            if (!ModelState.IsValid)
            {
                //Display each of the model state errors.
                foreach (var item in ModelState)
                {
                    if (item.Value.Errors.Count > 0)
                    {
                        foreach (var error in item.Value.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.ErrorMessage);
                        }
                    }
                }

                return Page();
            }
            //Modelstate is valid.
            else
            {
                await PopulateProperties();

                //Get all staff from the DB.
                Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
                string? jwtToken = jwtTokenClaim.Value;
                List<Staff> allStaff = await _staffService.GetAllStaff(jwtToken);

                //Get the staff member from the DB.
                Staff = await _staffService.GetStaff(StaffInput.ID, jwtToken);

                //Email address has been changed.
                if (StaffInput.Email != Staff.Email)
                {
                    //Check if the email address already exists.
                    if (allStaff.Any(s => s.Email == StaffInput.Email))
                    {
                        ModelState.AddModelError(string.Empty, "Email address already used");
                        ModelState.AddModelError("StaffInput.Email", "Email address already used");

                        return Page();
                    }
                }

                //Set the Staff property with the values from the StaffInput property.
                Staff.Email = StaffInput.Email;
                Staff.Prefix = StaffInput.Prefix;
                Staff.FirstName = StaffInput.FirstName;
                Staff.LastName = StaffInput.LastName;
                Staff.Active = StaffInput.Active;

                //Update the staff member in the DB.
                Staff updatedStaff = await _staffService.UpdateStaff(Staff, jwtToken);

                //If the staff member was successfully updated, send an email to the new staff member.
                if (updatedStaff != null)
                {
                    UpdateSuccess = true;

                    return Page();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the staff member");

                    return Page();
                }
            }
        }
        #endregion Edit Staff

        #region Data
        //Method Summary:
        //This method is excuted when the a post occurs.
        //When excuted, it populates the page properties.
        public async Task PopulateProperties()
        {
            //Get the current user ID and JWT token.
            string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
            string? jwtToken = jwtTokenClaim.Value;
            CurrentStaff = await _staffService.GetStaff(Convert.ToInt32(userID), jwtToken);
        }
        #endregion Data
    }
}
