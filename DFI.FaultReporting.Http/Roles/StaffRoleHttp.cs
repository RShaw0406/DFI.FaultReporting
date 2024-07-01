using DFI.FaultReporting.Common.Constants;
using DFI.FaultReporting.Models.Roles;
using DFI.FaultReporting.Services.Interfaces.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Http.Roles
{
    public class StaffRoleHttp
    {
        public IHttpClientFactory _client { get; }

        public ISettingsService _settings { get; }

        public StaffRoleHttp(IHttpClientFactory client, ISettingsService settings)
        {
            _client = client;
            _settings = settings;
        }

        public List<StaffRole>? StaffRoles { get; set; }

        public async Task<List<StaffRole>> GetStaffRoles(string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(baseURL + APIEndPoints.StaffRole)
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                StaffRoles = JsonConvert.DeserializeObject<List<StaffRole>>(response);

                return StaffRoles;
            }
            else
            {
                return null;
            }
        }

        public async Task<StaffRole> GetStaffRole(int ID, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(baseURL + APIEndPoints.StaffRole + "/" + ID)
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                StaffRole staffRole = JsonConvert.DeserializeObject<StaffRole>(response);

                return staffRole;
            }
            else
            {
                return null;
            }
        }

        public async Task<StaffRole> CreateStaffRole(StaffRole staffRole, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(baseURL + APIEndPoints.StaffRole),
                Content = new StringContent(JsonConvert.SerializeObject(staffRole), Encoding.UTF8, "application/json")
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                StaffRole newStaffRole = JsonConvert.DeserializeObject<StaffRole>(response);

                return newStaffRole;
            }
            else
            {
                return null;
            }
        }

        public async Task<StaffRole> UpdateStaffRole(StaffRole staffRole, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(baseURL + APIEndPoints.StaffRole),
                Content = new StringContent(JsonConvert.SerializeObject(staffRole), Encoding.UTF8, "application/json")
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                StaffRole updatedStaffRole = JsonConvert.DeserializeObject<StaffRole>(response);

                return updatedStaffRole;
            }
            else
            {
                return null;
            }
        }

        public async Task<int> DeleteStaffRole(int ID, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(baseURL + APIEndPoints.StaffRole + "/" + ID)
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                return ID;
            }
            else
            {
                return 0;
            }
        }
    }
}
