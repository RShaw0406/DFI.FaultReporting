using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Common.Constants
{
    public static class APIEndPoints
    {
        public const string ClaimStatus = "/api/claimstatus";
        public const string ClaimType = "/api/claimtypes";
        public const string FaultPriority = "/api/faultpriorities";
        public const string FaultStatus = "/api/faultstatus";
        public const string FaultType = "/api/faulttypes";
        public const string Fault = "/api/faults";
        public const string Report = "/api/reports";
        public const string ReportPhoto = "/api/reportphotos";
        public const string Role = "/api/roles";
        public const string User = "/api/users";
        public const string Staff = "/api/staffs";
        public const string UserRole = "/api/userroles";
        public const string StaffRole = "/api/staffroles";
        public const string AuthRegister = "/api/auth/register";
        public const string AuthLogin = "/api/auth/login";
        public const string AuthLock = "/api/auth/lock";
        public const string Contractor = "/api/contractors";
        public const string Repair = "/api/repairs";
        public const string RepairPhoto = "/api/repairphotos";
        public const string RepairStatus = "/api/repairstatus";
        public const string Claim = "/api/claims";
        public const string ClaimFile = "/api/claimfiles";
        public const string ClaimPhoto = "/api/claimphotos";
        public const string LegalRep = "/api/legalreps";
    }
}
