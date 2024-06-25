using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Interfaces.Files
{
    public interface IFileValidationService
    {
        Task<bool> ValidateDocument(IFormFile document);

        Task<bool> SanitizeImage(IFormFile document);
    }
}
