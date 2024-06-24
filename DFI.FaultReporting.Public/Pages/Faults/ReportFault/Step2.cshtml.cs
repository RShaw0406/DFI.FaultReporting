using DFI.FaultReporting.Common.SessionStorage;
using DFI.FaultReporting.Models.FaultReports;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DFI.FaultReporting.Public.Pages.Faults.ReportFault
{
    public class Step2Model : PageModel
    {
        public void OnGet()
        {
            //Get the fault from "Fault" object stored in session.
            Fault? sessionFault = HttpContext.Session.GetFromSession<Fault>("Fault");

            return;
        }
    }
}
