using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using DFI.FaultReporting.Models.Admin;
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
        public Staff CurrentStaff { get; set; }

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
                    //Clear session to ensure fresh start.
                    HttpContext.Session.Clear();

                    await PopulateProperties();

                    //Created and populate a new Contractor object.
                    Contractor = new Contractor();
                    Contractor.InputBy = CurrentStaff.Email;
                    Contractor.InputOn = DateTime.Now;
                    Contractor.Active = true;

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

        #region Create Contractor
        //Method Summary:
        //This method is executed when the "Submit" button is clicked.
        //When executed this method attempts to create a new contractor and returns the user to the index page.
        public async Task<IActionResult> OnPostAsync()
        {
            //Model state is not valid.
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

                //Get all contractors from the DB.
                Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
                string? jwtToken = jwtTokenClaim.Value;
                List<Contractor> contractors = await _contractorService.GetContractors(jwtToken);

                //Contractor email already exists.
                if (contractors.Any(c => c.Email == Contractor.Email))
                {
                    ModelState.AddModelError(string.Empty, "Contractor email already exists");
                    ModelState.AddModelError("Contractor.Email", "Contractor email already exists");

                    return Page();
                }

                //Create the new contractor in the DB.
                Contractor insertedContractor = await _contractorService.CreateContractor(Contractor, jwtToken);

                //If the contractor was successfully created.
                if (insertedContractor != null)
                {

                    //Send an email to the approved contractor email address.
                    Response emailResponse = await SendApprovedContractorEmail(insertedContractor.Email);

                    return RedirectToPage("./Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the contractor");

                    return Page();
                }
            }
        }
        #endregion Create Contractor

        #region Email
        //Method Summary:
        //This method is executed when the "Submit" button is clicked.
        //When executed this method attempts to send an email to the approved contractor and returns the response from the _emailService.
        public async Task<SendGrid.Response> SendApprovedContractorEmail(string emailAddress)
        {
            //Construct the email.
            string subject = "DFI Fault Reporting Administration: Approved Contractor";
            EmailAddress to = new EmailAddress(emailAddress);
            string textContent = string.Empty;
            string htmlContent = "<p>Hello,</p><p>A system administrator has added this email address to the DFI Faulting Reporting approved contractors list.</p>" +
                "<p>You can now use your this email address to register and log in to the DFI Fault Reporting application to view your assigned jobs.</p><br/>";
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
        }
        #endregion Data
    }
}
