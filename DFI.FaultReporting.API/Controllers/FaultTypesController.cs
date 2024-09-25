using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.SQL.Repository.Contexts;
using DFI.FaultReporting.SQL.Repository.Interfaces.Admin;
using DFI.FaultReporting.SQL.Repository.Admin;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Net;

namespace DFI.FaultReporting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FaultTypesController : ControllerBase
    {
        private IFaultTypeSQLRepository _faultTypeSQLRepository;
        public ILogger<FaultTypesController> _logger;

        public FaultTypesController(IFaultTypeSQLRepository faultTypeSQLRepository, ILogger<FaultTypesController> logger)
        {
            _faultTypeSQLRepository = faultTypeSQLRepository;
            _logger = logger;
        }

        public List<FaultType>? FaultTypes { get; set; }

        // GET: api/FaultTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FaultType>>> GetFaultType()
        {
            FaultTypes = await _faultTypeSQLRepository.GetFaultTypes();
            return FaultTypes;
        }

        // GET: api/FaultTypes/5
        [HttpGet("{ID}")]
        [Authorize]
        public async Task<ActionResult<FaultType>> GetFaultType(int ID)
        {
            FaultType faultType = await _faultTypeSQLRepository.GetFaultType(ID);

            if (faultType == null)
            {
                return NotFound();
            }

            return faultType;
        }

        // POST: api/FaultTypes
        [HttpPost]
        [Authorize(Roles = "StaffAdmin")]
        public async Task<ActionResult<FaultType>> PostFaultType(FaultType faultType)
        {
            faultType = await _faultTypeSQLRepository.CreateFaultType(faultType);

            return CreatedAtAction("GetFaultType", new { faultType.ID }, faultType);
        }

        // PUT: api/FaultTypes
        [HttpPut]
        [Authorize(Roles = "StaffAdmin")]
        public async Task<ActionResult<FaultType>> PutFaultType(FaultType faultType)
        {
            try
            {
                faultType = await _faultTypeSQLRepository.UpdateFaultType(faultType);

                return faultType;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                FaultTypes = await _faultTypeSQLRepository.GetFaultTypes();

                if (!FaultTypes.Any(cs => cs.ID == faultType.ID))
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
