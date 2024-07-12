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
    public class ClaimPhotoSQLRepository : IClaimPhotoSQLRepository
    {
        public DFIFaultReportingDataContext _context;

        public ClaimPhotoSQLRepository(DFIFaultReportingDataContext context)
        {
            _context = context;
        }

        public List<ClaimPhoto>? ClaimPhotos { get; set; }

        public async Task<List<ClaimPhoto>> GetClaimPhotos()
        {
            ClaimPhotos = await _context.ClaimPhoto.ToListAsync();
            return ClaimPhotos;
        }

        public async Task<ClaimPhoto> GetClaimPhoto(int ID)
        {
            ClaimPhoto claimPhoto = await _context.ClaimPhoto.FindAsync(ID);
            return claimPhoto;
        }

        public async Task<ClaimPhoto> CreateClaimPhoto(ClaimPhoto claimPhoto)
        {
            _context.ClaimPhoto.Add(claimPhoto);
            await _context.SaveChangesAsync();
            return claimPhoto;
        }

        public async Task<ClaimPhoto> UpdateClaimPhoto(ClaimPhoto claimPhoto)
        {
            _context.Entry(claimPhoto).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return claimPhoto;
        }
    }
}
