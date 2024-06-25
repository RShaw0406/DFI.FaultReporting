using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Interfaces.Files
{
    public interface IFileDetectorService
    {
        bool IsSafe(byte[] documentBytes);
    }
}
