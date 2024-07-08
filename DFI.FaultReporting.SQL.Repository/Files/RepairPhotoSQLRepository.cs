using DFI.FaultReporting.Models.Files;
using DFI.FaultReporting.SQL.Repository.Contexts;
using DFI.FaultReporting.SQL.Repository.Interfaces.Files;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.SQL.Repository.Files
{
    public class RepairPhotoSQLRepository : IRepairPhotoSQLRepository
    {
        public DFIFaultReportingDataContext _context;

        public RepairPhotoSQLRepository(DFIFaultReportingDataContext context)
        {
            _context = context;
        }

        public List<RepairPhoto>? RepairPhotos { get; set; }

        public async Task<List<RepairPhoto>> GetRepairPhotos()
        {
            RepairPhotos = await _context.RepairPhoto.ToListAsync();
            return RepairPhotos;
        }

        public async Task<RepairPhoto> GetRepairPhoto(int ID)
        {
            RepairPhoto repairPhoto = await _context.RepairPhoto.FindAsync(ID);
            return repairPhoto;
        }

        public async Task<RepairPhoto> CreateRepairPhoto(RepairPhoto repairPhoto)
        {
            _context.RepairPhoto.Add(repairPhoto);
            await _context.SaveChangesAsync();
            return repairPhoto;
        }

        public async Task<RepairPhoto> UpdateRepairPhoto(RepairPhoto repairPhoto)
        {
            _context.Entry(repairPhoto).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return repairPhoto;
        }
    }
}
