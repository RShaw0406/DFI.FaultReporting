using DFI.FaultReporting.Models.FaultReports;
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
    public class ReportPhotoSQLRepository : IReportPhotoSQLRepository
    {
        public DFIFaultReportingDataContext _context;

        public ReportPhotoSQLRepository(DFIFaultReportingDataContext context)
        {
            _context = context;
        }

        public List<ReportPhoto>? ReportPhotos { get; set; }

        public async Task<List<ReportPhoto>> GetReportPhotos()
        {
            ReportPhotos = await _context.ReportPhoto.ToListAsync();
            return ReportPhotos;
        }

        public async Task<ReportPhoto> GetReportPhoto(int ID)
        {
            ReportPhoto reportPhoto = await _context.ReportPhoto.FindAsync(ID);
            return reportPhoto;
        }

        public async Task<ReportPhoto> CreateReportPhoto(ReportPhoto reportPhoto)
        {
            _context.ReportPhoto.Add(reportPhoto);
            await _context.SaveChangesAsync();
            return reportPhoto;
        }

        public async Task<ReportPhoto> UpdateReportPhoto(ReportPhoto reportPhoto)
        {
            _context.Entry(reportPhoto).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return reportPhoto;
        }

        public async Task<int> DeleteReportPhoto(int ID)
        {
            ReportPhoto reportPhoto = await _context.ReportPhoto.Where(cs => cs.ID == ID).FirstOrDefaultAsync();
            _context.ReportPhoto.Remove(reportPhoto);
            await _context.SaveChangesAsync();
            return ID;
        }
    }
}
