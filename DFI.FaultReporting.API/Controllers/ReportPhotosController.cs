using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DFI.FaultReporting.Models.Files;
using DFI.FaultReporting.SQL.Repository.Contexts;
using DFI.FaultReporting.Models.FaultReports;
using DFI.FaultReporting.SQL.Repository.Interfaces.FaultReports;
using DFI.FaultReporting.SQL.Repository.Interfaces.Files;
using DFI.FaultReporting.SQL.Repository.FaultReports;
using System.Composition;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using System.Net;

namespace DFI.FaultReporting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportPhotosController : ControllerBase
    {
        private IReportPhotoSQLRepository _reportPhotoSQLRepository;
        public ILogger<ReportPhotosController> _logger;

        public ReportPhotosController(IReportPhotoSQLRepository reportPhotoSQLRepository, ILogger<ReportPhotosController> logger)
        {
            _reportPhotoSQLRepository = reportPhotoSQLRepository;
            _logger = logger;
        }

        public List<ReportPhoto>? ReportPhotos { get; set; }

        // GET: api/ReportPhotos
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ReportPhoto>>> GetReportPhoto()
        {
            ReportPhotos = await _reportPhotoSQLRepository.GetReportPhotos();
            return ReportPhotos;
        }

        // GET: api/ReportPhotos/5
        [HttpGet("{ID}")]
        [Authorize]
        public async Task<ActionResult<ReportPhoto>> GetReportPhoto(int ID)
        {
            ReportPhoto reportPhoto = await _reportPhotoSQLRepository.GetReportPhoto(ID);

            if (reportPhoto == null)
            {
                return NotFound();
            }

            return reportPhoto;
        }

        // POST: api/ReportPhotos
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ReportPhoto>> PostReportPhoto(ReportPhoto reportPhoto)
        {
            reportPhoto = await _reportPhotoSQLRepository.CreateReportPhoto(reportPhoto);
            return CreatedAtAction("GetReportPhoto", new { reportPhoto.ID }, reportPhoto);
        }

        // PUT: api/ReportPhotos
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        [Authorize]
        public async Task<ActionResult<ReportPhoto>> PutReportPhoto(ReportPhoto reportPhoto)
        {
            try
            {
                reportPhoto = await _reportPhotoSQLRepository.UpdateReportPhoto(reportPhoto);

                return reportPhoto;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                ReportPhotos = await _reportPhotoSQLRepository.GetReportPhotos();

                if (!ReportPhotos.Any(cs => cs.ID == reportPhoto.ID))
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
