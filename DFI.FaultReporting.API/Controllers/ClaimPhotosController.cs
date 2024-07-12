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
    public class ClaimPhotosController : ControllerBase
    {
        private IClaimPhotoSQLRepository _claimPhotoSQLRepository;
        public ILogger<ClaimPhotosController> _logger;

        public ClaimPhotosController(IClaimPhotoSQLRepository claimPhotoSQLRepository, ILogger<ClaimPhotosController> logger)
        {
            _claimPhotoSQLRepository = claimPhotoSQLRepository;
            _logger = logger;
        }

        public List<ClaimPhoto>? ClaimPhotos { get; set; }

        // GET: api/ClaimPhotoes
        [HttpGet]
        [Authorize(Roles = "User, StaffReadWrite, StaffRead")]
        public async Task<ActionResult<IEnumerable<ClaimPhoto>>> GetClaimPhoto()
        {
            ClaimPhotos = await _claimPhotoSQLRepository.GetClaimPhotos();
            return ClaimPhotos;
        }

        // GET: api/ClaimPhotoes/5
        [HttpGet("{ID}")]
        [Authorize(Roles = "User, StaffReadWrite, StaffRead")]
        public async Task<ActionResult<ClaimPhoto>> GetClaimPhoto(int ID)
        {
            ClaimPhoto claimPhoto = await _claimPhotoSQLRepository.GetClaimPhoto(ID);

            if (claimPhoto == null)
            {
                return NotFound();
            }

            return claimPhoto;
        }

        // POST: api/ClaimPhotoes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<ClaimPhoto>> PostClaimPhoto(ClaimPhoto claimPhoto)
        {
            await _claimPhotoSQLRepository.CreateClaimPhoto(claimPhoto);
            return CreatedAtAction("GetClaimPhoto", new { claimPhoto.ID }, claimPhoto);
        }

        // PUT: api/ClaimPhotoes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<ClaimPhoto>> PutClaimPhoto(ClaimPhoto claimPhoto)
        {
            try
            {
                claimPhoto = await _claimPhotoSQLRepository.UpdateClaimPhoto(claimPhoto);

                return claimPhoto;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                ClaimPhotos = await _claimPhotoSQLRepository.GetClaimPhotos();

                if (!ClaimPhotos.Any(cs => cs.ID == claimPhoto.ID))
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
