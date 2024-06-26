using DFI.FaultReporting.Common.SessionStorage;
using DFI.FaultReporting.Interfaces.Admin;
using DFI.FaultReporting.Interfaces.FaultReports;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Models.FaultReports;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Public.Pages.Faults.ReportFault;
using DFI.FaultReporting.Services.Interfaces.Admin;
using DFI.FaultReporting.Services.Interfaces.Emails;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Tokens;
using DFI.FaultReporting.Services.Interfaces.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using static DFI.FaultReporting.Public.Pages.Faults.ReportFault.Step1Model;

namespace DFI.FaultReporting.Public.Pages.Faults
{
    public class FaultsModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<FaultsModel> _logger;
        private readonly IUserService _userService;
        private readonly IFaultService _faultService;
        private readonly IFaultPriorityService _faultPriorityService;
        private readonly IFaultStatusService _faultStatusService;
        private readonly IFaultTypeService _faultTypeService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISettingsService _settingsService;
        private readonly IEmailService _emailService;
        private readonly IVerificationTokenService _verificationTokenService;

        //Inject dependencies in constructor.
        public FaultsModel(ILogger<FaultsModel> logger, IUserService userService, IFaultService faultService, IFaultTypeService faultTypeService,
            IFaultPriorityService faultPriorityService, IFaultStatusService faultStatusService,
            IHttpContextAccessor httpContextAccessor, ISettingsService settingsService, IEmailService emailService,
            IVerificationTokenService verificationTokenService)
        {
            _logger = logger;
            _userService = userService;
            _faultService = faultService;
            _faultPriorityService = faultPriorityService;
            _faultStatusService = faultStatusService;
            _faultTypeService = faultTypeService;
            _httpContextAccessor = httpContextAccessor;
            _settingsService = settingsService;
            _emailService = emailService;
            _verificationTokenService = verificationTokenService;
        }
        #endregion Dependency Injection

        #region Properties
        //Declare CurrentUser property, this is needed when calling the _userService.
        public User CurrentUser { get; set; }

        //Declare Faults property, this is needed for displaying faults on the map.
        public List<Fault> Faults { get; set; }

        //Declare FaultPriorities property, this is needed for displaying on map.
        public List<FaultPriority> FaultPriorities { get; set; }

        //Declare FaultStatuses property, this is needed for displaying on map.
        public List<FaultStatus> FaultStatuses { get; set; }

        //Declare FaultTypes property, this is needed for populating fault types dropdown list.
        public List<FaultType> FaultTypes { get; set; }

        //Declare FaultTypesList property, this is needed for populating fault types dropdown list.
        public IEnumerable<SelectListItem> FaultTypesList { get; set; }
        #endregion Properties

        #region Page Load
        public async Task<IActionResult> OnGetAsync()
        {
            //Clear session to ensure fresh start.
            HttpContext.Session.Clear();

            //Get fault types for dropdown.
            FaultTypes = await GetFaultTypes();

            //Populate fault types dropdown.
            FaultTypesList = FaultTypes.Select(ft => new SelectListItem()
            {
                Text = ft.FaultTypeDescription,
                Value = ft.ID.ToString()
            });

            //Get all current faults by calling the GetFaults method from the _faultService.
            Faults = await _faultService.GetFaults();

            //Get all fault priorities by calling the GetFaultPriorities method from the _faultPriorityService.
            FaultPriorities = await _faultPriorityService.GetFaultPriorities();

            //Get all fault statuses by calling the GetFaultStatuses method from the _faultStatusService.
            FaultStatuses = await _faultStatusService.GetFaultStatuses();

            //Set the Faults in session, needed for displaying on map.
            HttpContext.Session.SetInSession("Faults", Faults);
            HttpContext.Session.SetInSession("FaultTypes", FaultTypes);
            HttpContext.Session.SetInSession("FaultPriorities", FaultPriorities);
            HttpContext.Session.SetInSession("FaultStatuses", FaultStatuses);

            //Return the page.
            return Page();
        }
        #endregion Page Load

        #region Fault Types
        //Method Summary:
        //This method is executed when the page loads and when an error occurs.
        //When executed this method returns a list of fault types to populate the fault types dropdown list.
        public async Task<List<FaultType>> GetFaultTypes()
        {
            //Get fault types by calling the GetFaultTypes method from the _faultTypeService.
            List<FaultType> faultTypes = await _faultTypeService.GetFaultTypes();

            //Return fault types.
            return faultTypes;
        }

        public void OnGetAddReport()
        {
            Redirect("/Faults/ReportFault/ReportFault");
        }
        #endregion Fault Types
    }
}
