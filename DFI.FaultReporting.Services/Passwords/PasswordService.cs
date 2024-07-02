using DFI.FaultReporting.Services.Interfaces.Passwords;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;

namespace DFI.FaultReporting.Services.Passwords
{
    public class PasswordService : IPasswordService
    {
        public class Requirements
        {
            public bool Digits { get; set; } = true;
            public bool Special { get; set; } = true;
            public bool Uppercase { get; set; } = true;
            public bool Lowercase { get; set; } = true;
            public int Length { get; set; } = 8;
        }


        public async Task<string> GenerateRandomPassword()
        {
            Requirements requirements = new Requirements
            {
                Digits = true,
                Special = true,
                Uppercase = true,
                Lowercase = true,
                Length = 8
            };

            string[] availableCharacters = new[]
            {
                "ABCDEFGHJKLMNOPQRSTUVWXYZ",
                "abcdefghijkmnopqrstuvwxyz",
                "0123456789",
                "!@$?_-"
            };

            Random random = new Random();

            List<char> password = new List<char>();

            if (requirements.Uppercase)
            {
                password.Insert(random.Next(0, password.Count), availableCharacters[0][random.Next(0, availableCharacters[0].Length)]);
            }

            if (requirements.Lowercase)
            {
                password.Insert(random.Next(0, password.Count), availableCharacters[1][random.Next(0, availableCharacters[1].Length)]);
            }

            if (requirements.Digits)
            {
                password.Insert(random.Next(0, password.Count), availableCharacters[2][random.Next(0, availableCharacters[2].Length)]);
            }

            if (requirements.Special)
            {
                password.Insert(random.Next(0, password.Count), availableCharacters[3][random.Next(0, availableCharacters[3].Length)]);
            }

            for (int i = password.Count; i < requirements.Length; i++)
            {
                string rcs = availableCharacters[random.Next(0, availableCharacters.Length)];
                password.Insert(random.Next(0, password.Count),
                    rcs[random.Next(0, rcs.Length)]);
            }

            return new string(password.ToArray());
        }
    }
}
