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
using DFI.FaultReporting.Models.Admin;

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
        public Staff CurrentStaff { get; set; }

        [BindProperty]
        public StaffInputModel StaffInput { get; set; }

        [BindProperty]
        public List<Staff> Staff { get; set; }

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
        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is called when the page is loaded.
        //It checks if the current user is authenticated and if so, it gets the current user and returns the page.
        public async Task<IActionResult> OnGetAsync()
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

        #region Create Staff
        //Method Summary:
        //This method is executed when the "Submit" button is clicked.
        //When executed this method attempts to create a new staff member and returns the user to the index page.
        public async Task<IActionResult> OnPostAsync()
        {
            //ModelState is not valid.
            if (!ModelState.IsValid)
            {
                //Display each of the model errors.
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
            //ModelState is valid.
            else
            {
                await PopulateProperties();

                //Check if the email address already exists.
                if (Staff.Any(s => s.Email == StaffInput.Email))
                {
                    ModelState.AddModelError(string.Empty, "Email address already used");
                    ModelState.AddModelError("StaffInput.Email", "Email address already used");

                    return Page();
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
                Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
                string? jwtToken = jwtTokenClaim.Value;
                Staff insertedStaff = await _staffService.CreateStaff(newStaff, jwtToken);

                //If the staff member was successfully created, send an email to the new staff member.
                if (insertedStaff != null)
                {
                    //Send an email to the new staff member.
                    Response emailResponse = await SendAccountCreationEmail(newStaff.Email, newStaff.Password);

                    return RedirectToPage("./Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the staff member");

                    return Page();
                }
            }
        }
        #endregion Create Staff

        #region Email
        //Method Summary:
        //This method is executed when the "Submit" button is clicked.
        //When executed this method attempts to send an email to the create staff and returns the response from the _emailService.
        public async Task<SendGrid.Response> SendAccountCreationEmail(string emailAddress, string password)
        {
            //Construct the email.
            string subject = "DFI Fault Reporting Administration: Account created";
            EmailAddress to = new EmailAddress(emailAddress);
            string textContent = string.Empty;
            string htmlContent = "<p>Hello,</p><p>A system administrator has created your account.</p>" +
                "<p>You can now use your this email address and your temporary password below to login.</p><br/>" +
                "<p><strong>Temporary password:</strong></p>" +
                password + "<br/>" +
                "<p>We recommend that you updated your password after logging in for the first time.</p>";
            SendGrid.Helpers.Mail.Attachment? attachment = null;

            //Call the SendEmail in the _emailService and return the response.
            return await _emailService.SendEmail(subject, to, textContent, htmlContent, attachment);
        }
        #endregion Email

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

            Staff = await _staffService.GetAllStaff(jwtToken);
        }
        #endregion Data
    }
}
