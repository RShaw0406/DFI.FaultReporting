using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Interfaces.Files
{
    public interface IMimeSnifferService
    {
        Task<bool> IsPdf(byte[] documentBytes);

        Task<bool> IsWord(byte[] documentBytes);

        Task<bool> IsImage(byte[] documentBytes);

        Task<bool> VerifyMimeType(string fileName, byte[] documentBytes);

        Task<string> SniffMimeType(byte[] documentBytes);

    }
}
