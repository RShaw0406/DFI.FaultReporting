using DFI.FaultReporting.Models.Roles;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Emails;
using DFI.FaultReporting.Services.Interfaces.Passwords;
using DFI.FaultReporting.Services.Interfaces.Roles;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Security.Claims;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Rendering;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using Newtonsoft.Json.Linq;
using DFI.FaultReporting.Common.SessionStorage;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace DFI.FaultReporting.Admin.Pages.Admin.StaffAdmin
{
    public class RolesModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<RolesModel> _logger;
        private readonly IStaffService _staffService;
        private readonly IRoleService _roleService;
        private readonly IStaffRoleService _staffRoleService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISettingsService _settingsService;
        private readonly IEmailService _emailService;
        private readonly IPasswordService _passwordService;

        //Inject dependencies in constructor.
        public RolesModel(ILogger<RolesModel> logger, IStaffService staffService, IRoleService roleService, IStaffRoleService staffRoleService,
            IHttpContextAccessor httpContextAccessor, ISettingsService settingsService, IEmailService emailService, IPasswordService passwordService)
        {
            _logger = logger;
            _staffService = staffService;
            _roleService = roleService;
            _staffRoleService = staffRoleService;
            _httpContextAccessor = httpContextAccessor;
            _settingsService = settingsService;
            _emailService = emailService;
            _passwordService = passwordService;
        }
        #endregion Dependency Injection

        #region Properties
        [BindProperty]
        public Staff CurrentStaff { get; set; }

        [BindProperty]
        public Staff Staff { get; set; }

        [BindProperty]
        public List<Role> Roles { get; set; }

        [BindProperty]
        public List<Role> AssignedRoles { get; set; }

        [BindProperty]
        public RoleInputModel RolesInput { get; set; }

        [BindProperty]
        public List<RoleInputModel>SelectedRoles { get; set; }

        [BindProperty]
        public List<StaffRole> StaffRoles { get; set; }

        public bool UpdateSuccess { get; set; }

        public class RoleInputModel
        {
            [BindProperty]
            public int RoleID { get; set; }

            [BindProperty]
            public string? Role { get; set; }

            [BindProperty]
            public bool isSelected { get; set; }

            [BindProperty]
            public int? StaffRoleID { get; set; }
        }
        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is called when the page is loaded.
        //It checks if the current user is authenticated and if so, it gets the current user and staff roles from the DB.
        public async Task<IActionResult> OnGetAsync(int ID)
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

                    //Clear TempData to ensure fresh start.
                    TempData.Clear();

                    //Get all staff from the DB.
                    Staff = await _staffService.GetStaff(ID, jwtToken);

                    //Get all roles from the DB.
                    Roles = await _roleService.GetRoles(jwtToken);

                    //Filter out roles that are not staff roles.
                    Roles = Roles.Where(r => r.RoleDescription.Contains("Staff")).ToList();

                    //Get all staff roles by calling the GetStaffRoles method from the _staffRoleService.
                    StaffRoles = await _staffRoleService.GetStaffRoles(jwtToken);

                    //Create a new list of Role objects to store the staff members assigned roles.
                    AssignedRoles = new List<Role>();

                    //Loop through all roles.
                    foreach (Role role in Roles)
                    {
                        //Loop through all staff roles.
                        foreach (StaffRole staffRole in StaffRoles)
                        {
                            //If the staff member ID and role ID match, add the role to the AssignedRoles list.
                            if (staffRole.StaffID == Staff.ID && staffRole.RoleID == role.ID)
                            {
                                //Add the role to the AssignedRoles list.
                                AssignedRoles.Add(role);
                            }
                        }
                    }

                    //Create a new list of RoleInputModel objects.
                    SelectedRoles = new List<RoleInputModel>();

                    //Loop through all roles and add them to the RoleInputs list.
                    foreach (Role role in Roles)
                    {
                        //Create a new RoleInputModel object.
                        RolesInput = new RoleInputModel();
                        RolesInput.RoleID = role.ID;
                        RolesInput.Role = role.RoleDescription;
                        RolesInput.isSelected = false;
                        RolesInput.StaffRoleID = 0;

                        //Add the RoleInputModel object to the RoleInputs list.
                        SelectedRoles.Add(RolesInput);
                    }

                    //Loop through all roles.
                    foreach (RoleInputModel roleInputModel in SelectedRoles)
                    {
                        //Loop through all assigned roles.
                        foreach (Role role in AssignedRoles)
                        {
                            //If the role ID matches the assigned role ID, set the isSelected property to true.
                            if (roleInputModel.RoleID == role.ID)
                            {
                                //Set the isSelected property to true.
                                roleInputModel.isSelected = true;

                                //Set the StaffRoleID property to the staff role ID.
                                roleInputModel.StaffRoleID = StaffRoles.Where(sr => sr.RoleID == role.ID && sr.StaffID == ID).FirstOrDefault().ID;
                            }
                        }
                    }

                    //Set the AssignedRoles list in session.
                    HttpContext.Session.SetInSession("AssignedRoles", AssignedRoles);

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

        #region Roles

        //Method Summary:
        //This method is executed when the "Submit" button is clicked.
        //When executed, it assigns roles to staff member or removes roles.
        public async Task<IActionResult> OnPost()
        {
            //Get the ID from the contexts current user, needed for populating CurrentUser property from DB.
            string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            //Get the JWT token claim from the contexts current user, needed for populating CurrentUser property from DB.
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

            //Set the jwtToken string to the JWT token claims value, needed for populating CurrentUser property from DB.
            string? jwtToken = jwtTokenClaim.Value;

            //Set the CurrentStaff property by calling the GetUser method in the _userService.
            CurrentStaff = await _staffService.GetStaff(Convert.ToInt32(userID), jwtToken);

            //Get the assigned roles from session.
            AssignedRoles = HttpContext.Session.GetFromSession<List<Role>>("AssignedRoles");

            //Create a new list of Role objects to store the roles to insert.
            List<Role> rolesToInsert = new List<Role>();

            //Create a new list of Role objects to store the roles to delete.
            List<Role> rolesToDelete = new List<Role>();

            //Set the rolesToInsert by getting the roles from the SelectedRoles list where isSelected is true.
            rolesToInsert = SelectedRoles.Where(sr => sr.isSelected == true)
                .Select(r => new Role { ID = r.RoleID, RoleDescription = r.Role }).ToList();

            //Filter out roles that are already assigned.
            rolesToInsert = rolesToInsert.Where(ir => !AssignedRoles.Any(ar => ar.ID == ir.ID)).ToList();

            //Filter out roles that are not selected.
            rolesToInsert = rolesToInsert.Where(ir => SelectedRoles.Any(sr => sr.RoleID == ir.ID && sr.isSelected == true)).ToList();

            //Set the rolesToDelete by getting the roles from the SelectedRoles list where isSelected is false.
            rolesToDelete = SelectedRoles.Where(sr => sr.isSelected == false)
                .Select(r => new Role { ID = r.RoleID, RoleDescription = r.Role }).ToList();

            //Filter out roles that are not assigned.
            rolesToDelete = rolesToDelete.Where(ir => AssignedRoles.Any(ar => ar.ID == ir.ID)).ToList();

            //Filter out roles that are selected.
            rolesToDelete = rolesToDelete.Where(ir => SelectedRoles.Any(sr => sr.RoleID == ir.ID && sr.isSelected == false)).ToList();

            //Create a new list of StaffRole objects to store the inserted staff roles.
            List<StaffRole> insertedStaffRoles = new List<StaffRole>();

            //User has selected roles to insert.
            if (rolesToInsert.Count > 0)
            {
                //Loop through all roles to insert.
                foreach (Role role in rolesToInsert)
                {
                    //Create a new StaffRole object.
                    StaffRole staffRole = new StaffRole();
                    staffRole.RoleID = role.ID;
                    staffRole.StaffID = Staff.ID;
                    staffRole.InputBy = CurrentStaff.Email;
                    staffRole.InputOn = DateTime.Now;
                    staffRole.Active = true;

                    //Insert the staff role by calling the CreateStaffRole method from the _staffRoleService.
                    StaffRole insertedStaffRole = await _staffRoleService.CreateStaffRole(staffRole, jwtToken);

                    //Add the inserted staff role to the insertedStaffRoles list.
                    insertedStaffRoles.Add(insertedStaffRole);
                }

                //If no roles have been inserted, add a model error.
                if (insertedStaffRoles.Count == 0)
                {
                    //Add a model error.
                    ModelState.AddModelError(string.Empty, "Error inserting roles");

                    //Return the page.
                    return Page();
                }
            }

            //Create a new list of int objects to store the deleted staff roles IDs.
            List<int> deletedStaffRoles = new List<int>();

            //User has selected roles to delete.
            if (rolesToDelete.Count > 0)
            {
                //Loop through all roles to delete.
                foreach (Role role in rolesToDelete)
                {
                    //Get the staff role ID by getting the StaffRoleID from the SelectedRoles list where the RoleID matches the role ID.
                    int staffRoleID = (int)SelectedRoles.Where(sr => sr.RoleID == role.ID).FirstOrDefault().StaffRoleID; 

                    //Delete the staff role by calling the DeleteStaffRole method from the _staffRoleService.
                    int deletedStaffRoleID = await _staffRoleService.DeleteStaffRole(staffRoleID, jwtToken);

                    //Add the deleted staff role ID to the deletedStaffRoles list.
                    deletedStaffRoles.Add(deletedStaffRoleID);
                }

                //If no roles have been deleted, add a model error.
                if (deletedStaffRoles.Count == 0)
                {
                    //Add a model error.
                    ModelState.AddModelError(string.Empty, "Error deleting roles");

                    //Return the page.
                    return Page();
                }
            }

            //Roles have been updated successfully.
            if (deletedStaffRoles.Count > 0 || insertedStaffRoles.Count > 0)
            {
                //Set the UpdateSuccess property to true.
                UpdateSuccess = true;
            }

            //Get all roles from the DB.
            Roles = await _roleService.GetRoles(jwtToken);

            //Filter out roles that are not staff roles.
            Roles = Roles.Where(r => r.RoleDescription.Contains("Staff")).ToList();

            //Get all staff roles by calling the GetStaffRoles method from the _staffRoleService.
            StaffRoles = await _staffRoleService.GetStaffRoles(jwtToken);

            //Create a new list of Role objects to store the staff members assigned roles.
            AssignedRoles = new List<Role>();

            //Loop through all roles.
            foreach (Role role in Roles)
            {
                //Loop through all staff roles.
                foreach (StaffRole staffRole in StaffRoles)
                {
                    //If the staff member ID and role ID match, add the role to the AssignedRoles list.
                    if (staffRole.StaffID == Staff.ID && staffRole.RoleID == role.ID)
                    {
                        //Add the role to the AssignedRoles list.
                        AssignedRoles.Add(role);
                    }
                }
            }

            //Create a new list of RoleInputModel objects.
            SelectedRoles = new List<RoleInputModel>();

            //Loop through all roles and add them to the RoleInputs list.
            foreach (Role role in Roles)
            {
                //Create a new RoleInputModel object.
                RolesInput = new RoleInputModel();
                RolesInput.RoleID = role.ID;
                RolesInput.Role = role.RoleDescription;
                RolesInput.isSelected = false;
                RolesInput.StaffRoleID = 0;

                //Add the RoleInputModel object to the RoleInputs list.
                SelectedRoles.Add(RolesInput);
            }

            //Loop through all roles.
            foreach (RoleInputModel roleInputModel in SelectedRoles)
            {
                //Loop through all assigned roles.
                foreach (Role role in AssignedRoles)
                {
                    //If the role ID matches the assigned role ID, set the isSelected property to true.
                    if (roleInputModel.RoleID == role.ID)
                    {
                        //Set the isSelected property to true.
                        roleInputModel.isSelected = true;

                        //Set the StaffRoleID property to the staff role ID.
                        roleInputModel.StaffRoleID = StaffRoles.Where(sr => sr.RoleID == role.ID && sr.StaffID == Staff.ID).FirstOrDefault().ID;
                    }
                }
            }

            //Set the AssignedRoles list in session.
            HttpContext.Session.SetInSession("AssignedRoles", AssignedRoles);

            //Return the page.
            return Page();
        }
        #endregion Roles
    }
}
