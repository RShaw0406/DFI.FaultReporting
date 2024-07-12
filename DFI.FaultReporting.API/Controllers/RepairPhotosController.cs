using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DFI.FaultReporting.Models.Files;
using DFI.FaultReporting.SQL.Repository.Contexts;
using DFI.FaultReporting.SQL.Repository.Interfaces.Files;
using Microsoft.AspNetCore.Authorization;
using DFI.FaultReporting.SQL.Repository.Files;
using System.Net;

namespace DFI.FaultReporting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RepairPhotosController : ControllerBase
    {
        private IRepairPhotoSQLRepository _repairPhotoSQLRepository;
        public ILogger<RepairPhotosController> _logger;

        public RepairPhotosController(IRepairPhotoSQLRepository repairPhotoSQLRepository, ILogger<RepairPhotosController> logger)
        {
            _repairPhotoSQLRepository = repairPhotoSQLRepository;
            _logger = logger;
        }

        public List<RepairPhoto>? RepairPhotos { get; set; }

        // GET: api/RepairPhotos
        [HttpGet]
        [Authorize (Roles = "Contractor, StaffReadWrite, StaffRead")]
        public async Task<ActionResult<IEnumerable<RepairPhoto>>> GetRepairPhoto()
        {
            RepairPhotos = await _repairPhotoSQLRepository.GetRepairPhotos();
            return RepairPhotos;
        }

        // GET: api/RepairPhotos/5
        [HttpGet("{ID}")]
        [Authorize(Roles = "Contractor, StaffReadWrite, StaffRead")]
        public async Task<ActionResult<RepairPhoto>> GetRepairPhoto(int ID)
        {
            RepairPhoto repairPhoto = await _repairPhotoSQLRepository.GetRepairPhoto(ID);

            if (repairPhoto == null)
            {
                return NotFound();
            }

            return repairPhoto;
        }

        // POST: api/RepairPhotos
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "Contractor")]
        public async Task<ActionResult<RepairPhoto>> PostRepairPhoto(RepairPhoto repairPhoto)
        {
            repairPhoto = await _repairPhotoSQLRepository.CreateRepairPhoto(repairPhoto);
            return CreatedAtAction("GetRepairPhoto", new { repairPhoto.ID }, repairPhoto);
        }

        // PUT: api/RepairPhotos/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        [Authorize(Roles = "Contractor")]
        public async Task<ActionResult<RepairPhoto>> PutRepairPhoto(RepairPhoto repairPhoto)
        {
            try
            {
                repairPhoto = await _repairPhotoSQLRepository.UpdateRepairPhoto(repairPhoto);

                return repairPhoto;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                RepairPhotos = await _repairPhotoSQLRepository.GetRepairPhotos();

                if (!RepairPhotos.Any(cs => cs.ID == repairPhoto.ID))
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
