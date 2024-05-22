using DFI.FaultReporting.Services.Interfaces.Settings;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Settings
{
    public class SettingsService : ISettingsService
    {
        public SettingsService(IConfiguration config)
        {
            _config = config;
        }

        public IConfiguration _config { get; }

        public async Task<bool> GetSettingBoolean(string key, bool? defaultValue = null)
        {
            return await Task.Run(() => Convert.ToBoolean(_config.GetSection(key).Value));
        }

        public async Task<int> GetSettingInt(string key, int? defaultValue = null)
        {
            return await Task.Run(() => Convert.ToInt32(_config.GetSection(key).Value));
        }

        public async Task<string> GetSettingString(string key, string defaultValue = null)
        {
            return await Task.Run(() => _config.GetSection(key).Value);
        }
    }
}
