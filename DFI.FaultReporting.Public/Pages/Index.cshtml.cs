using DFI.FaultReporting.Interfaces.Admin;
using DFI.FaultReporting.Models.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace DFI.FaultReporting.Public.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        private readonly IClaimStatusService _claimStatusService;

        public IndexModel(ILogger<IndexModel> logger, IClaimStatusService claimStatusService)
        {
            _logger = logger;
            _claimStatusService = claimStatusService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            //List<ClaimStatus> claimStatuses = await _claimStatusService.GetClaimStatuses();

            //Debug.WriteLine(claimStatuses.ToString());

            //ClaimStatus claimStatus = await _claimStatusService.GetClaimStatus(2);

            //Debug.WriteLine(claimStatus.ToString());

            //ClaimStatus claimStatusNew = new ClaimStatus { ClaimStatusDescription = "Test New", InputBy = "Me", InputOn = DateTime.Now, Active = true };

            //await _claimStatusService.CreateClaimStatus(claimStatusNew);

            //claimStatus.ClaimStatusDescription = "NEW";
            //claimStatus.InputBy = "Me";
            //claimStatus.InputOn = DateTime.Now;
            //claimStatus.Active = false;

            //await _claimStatusService.UpdateClaimStatus(claimStatus);

            //await _claimStatusService.DeleteClaimStatus(7);

            //await _claimStatusService.DeleteClaimStatus(8);

            return Page();
        }

        //public void OnGet()
        //{
            
        //}
    }
}
