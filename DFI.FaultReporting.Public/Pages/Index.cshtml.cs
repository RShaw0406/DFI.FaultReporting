using DFI.FaultReporting.Interfaces.Admin;
using DFI.FaultReporting.Interfaces.Files;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Models.Files;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using System.Text;

namespace DFI.FaultReporting.Public.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        private readonly IReportPhotoService _reportPhotoService;

        public IndexModel(ILogger<IndexModel> logger, IReportPhotoService reportPhotoService)
        {
            _logger = logger;
            _reportPhotoService = reportPhotoService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            //List<ClaimStatus> claimStatuses = await _claimStatusService.GetClaimStatuses();

            //Debug.WriteLine(claimStatuses.ToString());

            //ClaimStatus claimStatus = await _claimStatusService.GetClaimStatus(2);

            //Debug.WriteLine(claimStatus.ToString());

            //byte[] bytes = Encoding.ASCII.GetBytes("0x0123456789ABCDEF");

            //ReportPhoto reportPhotoNew = new ReportPhoto { ReportID = 1, Description = "Test", Type = "TEST", Data = bytes, InputBy = "Me", InputOn = DateTime.Now, Active = true };

            //await _reportPhotoService.CreateReportPhoto(reportPhotoNew);

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
