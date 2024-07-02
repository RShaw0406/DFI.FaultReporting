using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.SQL.Repository.Contexts;
using DFI.FaultReporting.Services.Interfaces.Emails;
using DFI.FaultReporting.Services.Interfaces.Pagination;
using DFI.FaultReporting.Services.Interfaces.Roles;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Users;
using DFI.FaultReporting.Common.Pagination;
using DFI.FaultReporting.Models.Roles;
using System.ComponentModel;
using System.Security.Claims;
using DFI.FaultReporting.Common.SessionStorage;
using DFI.FaultReporting.Services.Interfaces.Passwords;
using SendGrid.Helpers.Mail;
using SendGrid;
using System.ComponentModel.DataAnnotations;

namespace DFI.FaultReporting.Admin.Pages.Admin.StaffAdmin
{
    public class CreateModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<CreateModel> _logger;
        private readonly IStaffService _staffService;
        private readonly IRoleService _roleService;
        private readonly IStaffRoleService _staffRoleService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISettingsService _settingsService;
        private readonly IEmailService _emailService;
        private readonly IPasswordService _passwordService;

        //Inject dependencies in constructor.
        public CreateModel(ILogger<CreateModel> logger, IStaffService staffService, IRoleService roleService, IStaffRoleService staffRoleService,
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
        //Declare CurrentStaff property, this is needed when calling the _staffService.
        public Staff CurrentStaff { get; set; }

        //Declare StaffInput property, this is needed when creating a new staff member.
        [BindProperty]
        public StaffInputModel StaffInput { get; set; }

        //Declare Staff property, this is needed when getting all staff from the DB.
        [BindProperty]
        public List<Staff> Staff { get; set; }

        //Declare Roles property, this is needed when getting all roles from the DB.
        [BindProperty]
        public List<Role> Roles { get; set; }

        //Declare RolesInput property, this is needed for assigning roles to staff members.
        [BindProperty]
        public RoleInputModel RolesInput { get; set; }

        //Declare RoleInputs property, this is needed for assigning roles to staff members.
        [BindProperty]
        public List<RoleInputModel> RoleInputs { get; set; } = new List<RoleInputModel>();

        //Declare StaffRoles property, this is needed for filtering staff by assigned roles.
        [BindProperty]
        public List<StaffRole> StaffRoles { get; set; }

        //Declare StaffInputModel, this is needed for validating the input when creating a new staff member.
        public class StaffInputModel
        {
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
        }

        public class RoleInputModel
        {
            public int RoleID { get; set; }

            public string? Role { get; set; }

            public bool Selected { get; set; }
        }
        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is called when the page is loaded.
        //It checks if the current user is authenticated and if so, it gets the current user and staff roles from the DB.
        public async Task<IActionResult> OnGetAsync()
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
                    Staff = await _staffService.GetAllStaff(jwtToken);

                    //Filter out inactive staff.
                    Staff = Staff.Where(s => s.Active == true).ToList();

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

        #region Create Staff
        //Method Summary:
        //This method is executed when the "Submit" button is clicked.
        //When executed this method attempts to create a new staff member and returns the user to the roles page.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            else
            {
                //Get the ID from the contexts current user, needed for populating CurrentUser property from DB.
                string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

                //Get the JWT token claim from the contexts current user, needed for populating CurrentUser property from DB.
                Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

                //Set the jwtToken string to the JWT token claims value, needed for populating CurrentUser property from DB.
                string? jwtToken = jwtTokenClaim.Value;

                //Set the CurrentStaff property by calling the GetUser method in the _userService.
                CurrentStaff = await _staffService.GetStaff(Convert.ToInt32(userID), jwtToken);

                //Loop through all staff and check if the email address already exists.
                foreach (Staff staff in Staff)
                {
                    //If the email address already exists, return an error message.
                    if (staff.Email == StaffInput.Email)
                    {
                        //Return an error message.
                        ModelState.AddModelError(string.Empty, "Email address already used");

                        //Return the page.
                        return Page();
                    }
                }

                //Create a new Staff object and assign the values from the StaffInputModel.
                Staff newStaff = new Staff();
                newStaff.Email = StaffInput.Email;
                newStaff.Prefix = StaffInput.Prefix;
                newStaff.FirstName = StaffInput.FirstName;
                newStaff.LastName = StaffInput.LastName;
                newStaff.Password = await _passwordService.GenerateRandomPassword();
                newStaff.InputBy = CurrentStaff.Email;

                //Create the new staff member in the DB.
                Staff insertedStaff = await _staffService.CreateStaff(newStaff, jwtToken);

                //If the staff member was successfully created, send an email to the new staff member.
                if (insertedStaff != null)
                {
                    //Send an email to the new staff member.
                    Response emailResponse = await SendAccountCreationEmail(newStaff.Email, newStaff.Password);

                    //If the email was successfully sent, return the user to the index page.
                    return RedirectToPage("./Index");
                }
                //If the staff member was not successfully created, return an error message.
                else
                {
                    //Return an error message.
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the staff member");

                    //Return the page.
                    return Page();
                }
            }
        }

        //Method Summary:
        //This method is executed when the "Submit" button is clicked.
        //When executed this method attempts to send an email to the create staff and returns the response from the _emailService.
        public async Task<SendGrid.Response> SendAccountCreationEmail(string emailAddress, string password)
        {
            //Set the subject of the email explaining that the user has deleted their account.
            string subject = "DFI Fault Reporting Administration: Account created";

            //Declare a new EmailAddress object and assign the users email address as the value.
            EmailAddress to = new EmailAddress(emailAddress);

            //Set textContent to empty string as it will not be used here.
            string textContent = string.Empty;

            //Set the htmlContent to a message explaining to the user that their account has been successfully deleted.
            string htmlContent = "<p>Hello,</p><p>A system administrator has created your account.</p>" +
                "<p>You can now use your this email address and your temporary password below to login.</p><br/>" +
                "<p><strong>Temporary password:</strong></p>" +
                password + "<br/>" +
                "<p>We recommend that you updated your password after logging in for the first time.</p>";

            //Set the attachment to null as it will not be used here.
            SendGrid.Helpers.Mail.Attachment? attachment = null;

            //Call the SendEmail in the _emailService and return the response.
            return await _emailService.SendEmail(subject, to, textContent, htmlContent, attachment);
        }
        #endregion
    }
}
