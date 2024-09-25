using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DFI.FaultReporting.Models.FaultReports;
using DFI.FaultReporting.SQL.Repository.Contexts;
using DFI.FaultReporting.SQL.Repository.Interfaces.FaultReports;
using DFI.FaultReporting.SQL.Repository.FaultReports;
using NuGet.Protocol.Plugins;
using Microsoft.AspNetCore.Authorization;
using System.Net;

namespace DFI.FaultReporting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private IReportSQLRepository _reportSQLRepository;
        public ILogger<ReportsController> _logger;

        public ReportsController(IReportSQLRepository reportSQLRepository, ILogger<ReportsController> logger)
        {
            _reportSQLRepository = reportSQLRepository;
            _logger = logger;
        }

        public List<Report>? Reports { get; set; }

        // GET: api/Reports
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Report>>> GetReport()
        {
            Reports = await _reportSQLRepository.GetReports();
            return Reports;
        }

        // GET: api/Reports/5
        [HttpGet("{ID}")]
        [Authorize]
        public async Task<ActionResult<Report>> GetReport(int ID)
        {
            Report report = await _reportSQLRepository.GetReport(ID);

            if (report == null)
            {
                return NotFound();
            }

            return report;
        }

        // POST: api/Reports
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Report>> PostReport(Report report)
        {
            report = await _reportSQLRepository.CreateReport(report);

            return CreatedAtAction("GetReport", new { report.ID }, report);
        }

        // PUT: api/Reports/5
        [HttpPut]
        [Authorize]
        public async Task<ActionResult<Report>> PutReport(Report report)
        {
            try
            {
                report = await _reportSQLRepository.UpdateReport(report);

                return report;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Reports = await _reportSQLRepository.GetReports();

                if (!Reports.Any(cs => cs.ID == report.ID))
                {
                    return NotFound();
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, ex.Message.ToString());
                }
            }
        }
    }
}
