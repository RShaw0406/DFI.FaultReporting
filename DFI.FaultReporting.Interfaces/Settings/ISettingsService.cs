using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Interfaces.Settings
{
    public interface ISettingsService
    {
        Task<string> GetSettingString(string key, string? defaultValue = null);
        Task<int> GetSettingInt(string key, int? defaultValue = null);
        Task<bool> GetSettingBoolean(string key, bool? defaultValue = null);
    }
}
