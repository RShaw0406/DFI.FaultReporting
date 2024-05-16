using DFI.FaultReporting.Models.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Interfaces.Files
{
    public interface IReportPhotoService
    {
        Task<List<ReportPhoto>> GetReportPhotos();

        Task<ReportPhoto> GetReportPhoto(int ID);

        Task<ReportPhoto> CreateReportPhoto(ReportPhoto reportPhoto);

        Task<ReportPhoto> UpdateReportPhoto(ReportPhoto reportPhoto);

        Task<ReportPhoto> DeleteReportPhoto(int ID);
    }
}
