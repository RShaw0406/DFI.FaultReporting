using DFI.FaultReporting.Common.Constants;
using DFI.FaultReporting.Common.Exceptions;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Models.Roles;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Http.Roles
{
    public class RoleHttp
    {
        public IHttpClientFactory _client { get; }

        public ISettingsService _settings { get; }

        public RoleHttp(IHttpClientFactory client, ISettingsService settings)
        {
            _client = client;
            _settings = settings;
        }

        public List<Role>? Roles { get; set; }

        public async Task<List<Role>> GetRoles(string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://localhost:7106" + APIEndPoints.Role)
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                Roles = JsonConvert.DeserializeObject<List<Role>>(response);

                return Roles;
            }
            else
            {
                return null;
            }
        }

        public async Task<Role> GetRole(int ID, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://localhost:7106" + APIEndPoints.Role + "/" + ID.ToString())
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                Role role = JsonConvert.DeserializeObject<Role>(response);

                return role;
            }
            else
            {
                return null;
            }
        }

        public async Task<Role> CreateRole(Role role, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            var roleJSON = JsonConvert.SerializeObject(role);

            var content = new StringContent(roleJSON, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://localhost:7106" + APIEndPoints.Role),
                Content = content
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                role = JsonConvert.DeserializeObject<Role>(response);

                return role;
            }
            else
            {
                return null;
            }
        }

        public async Task<Role> UpdateRole(Role role, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            var roleJSON = JsonConvert.SerializeObject(role);

            var content = new StringContent(roleJSON, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri("https://localhost:7106" + APIEndPoints.Role),
                Content = content
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                role = JsonConvert.DeserializeObject<Role>(response);

                return role;
            }
            else
            {
                return null;
            }
        }
    }
}
