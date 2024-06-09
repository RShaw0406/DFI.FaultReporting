using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Tokens;
using DFI.FaultReporting.SQL.Repository.Interfaces.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Tokens
{
    public class VerificationTokenService : IVerificationTokenService
    {
        public ISettingsService _settingsService;

        public VerificationTokenService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task<int> GenerateToken()
        {
            int minNum = 100000;

            int maxNum = 999999;

            byte[] bytes = new byte[4];

            using (RandomNumberGenerator randomNumberGenerator = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                randomNumberGenerator.GetBytes(bytes);
                int randomNum = BitConverter.ToInt32(bytes, 0);
                int verificationToken = Math.Abs(randomNum % (maxNum - minNum + 1)) + minNum;

                return verificationToken;
            }
        }
    }
}
