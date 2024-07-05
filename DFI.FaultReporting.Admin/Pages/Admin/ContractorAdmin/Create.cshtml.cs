using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.SQL.Repository.Contexts;
using DFI.FaultReporting.Common.Pagination;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Admin;
using DFI.FaultReporting.Services.Interfaces.Pagination;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Users;
using System.Security.Claims;
using SendGrid.Helpers.Mail;
using DFI.FaultReporting.Services.Interfaces.Emails;
using SendGrid;

namespace DFI.FaultReporting.Admin.Pages.Admin.ContractorAdmin
{
    public class CreateModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<CreateModel> _logger;
        private readonly IContractorService _contractorService;
        private readonly IStaffService _staffService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPagerService _pagerService;
        private readonly ISettingsService _settingsService;
        private readonly IEmailService _emailService;

        //Inject dependencies in constructor.
        public CreateModel(ILogger<CreateModel> logger, IContractorService contractorService, IStaffService staffService, IHttpContextAccessor httpContextAccessor,
            IPagerService pagerService, ISettingsService settingsService, IEmailService emailService)
        {
            _logger = logger;
            _contractorService = contractorService;
            _staffService = staffService;
            _httpContextAccessor = httpContextAccessor;
            _pagerService = pagerService;
            _settingsService = settingsService;
            _emailService = emailService;
        }
        #endregion Dependency Injection

        #region Properties
        //Declare CurrentStaff property, this is needed when calling the _staffService.
        public Staff CurrentStaff { get; set; }

        //Declare Contractor property, this is needed when getting contractor from the DB.
        [BindProperty]
        public Contractor Contractor { get; set; }
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

                    //Created and populate a new Contractor object.
                    Contractor = new Contractor();
                    Contractor.InputBy = CurrentStaff.Email;
                    Contractor.InputOn = DateTime.Now;
                    Contractor.Active = true;

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

        #region Create Contractor
        //Method Summary:
        //This method is executed when the "Submit" button is clicked.
        //When executed this method attempts to create a new contractor and returns the user to the index page.
        public async Task<IActionResult> OnPostAsync()
        {
            //Model state is not valid.
            if (!ModelState.IsValid)
            {
                //Loop through all model state items.
                foreach (var item in ModelState)
                {
                    //If the model state error has errors, add them to the model state.
                    if (item.Value.Errors.Count > 0)
                    {
                        //Loop through all errors and add them to the model state.
                        foreach (var error in item.Value.Errors)
                        {
                            //Add the error to the model state.
                            ModelState.AddModelError(string.Empty, error.ErrorMessage);
                        }
                    }
                }

                //Return the page.
                return Page();
            }
            //ModelState is valid.
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

                //Get all contractors from the DB.
                List<Contractor> contractors = await _contractorService.GetContractors(jwtToken);

                //Contractor email already exists.
                if (contractors.Any(c => c.Email == Contractor.Email))
                {
                    //Add model state error.
                    ModelState.AddModelError(string.Empty, "Contractor email already exists");
                    ModelState.AddModelError("Contractor.Email", "Contractor email already exists");

                    //Return the page.
                    return Page();
                }

                //Create the new contractor in the DB.
                Contractor insertedContractor = await _contractorService.CreateContractor(Contractor, jwtToken);

                //If the contractor was successfully created.
                if (insertedContractor != null)
                {

                    //Send an email to the approved contractor email address.
                    //Response emailResponse = await SendApprovedContractorEmail(insertedContractor.Email);

                    //Return to the index page.
                    return RedirectToPage("./Index");
                }
                //Contractor was not successfully created.
                else
                {
                    //Return an error message.
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the contractor");

                    //Return the page.
                    return Page();
                }
            }
        }

        //Method Summary:
        //This method is executed when the "Submit" button is clicked.
        //When executed this method attempts to send an email to the approved contractor and returns the response from the _emailService.
        public async Task<SendGrid.Response> SendApprovedContractorEmail(string emailAddress)
        {
            //Set the subject of the email explaining that the users email has been added to the approved contractors list
            string subject = "DFI Fault Reporting Administration: Approved Contractor";

            //Declare a new EmailAddress object and assign the users email address as the value.
            EmailAddress to = new EmailAddress(emailAddress);

            //Set textContent to empty string as it will not be used here.
            string textContent = string.Empty;

            //Set the htmlContent to a message explaining to the user that they can now register to use the public application.
            string htmlContent = "<p>Hello,</p><p>A system administrator has added this email address to the DFI Faulting Reporting approved contractors list.</p>" +
                "<p>You can now use your this email address to register and log in to the DFI Fault Reporting application to view your assigned jobs.</p><br/>";

            //Set the attachment to null as it will not be used here.
            SendGrid.Helpers.Mail.Attachment? attachment = null;

            //Call the SendEmail in the _emailService and return the response.
            return await _emailService.SendEmail(subject, to, textContent, htmlContent, attachment);
        }
        #endregion Create Contractor
    }
}
