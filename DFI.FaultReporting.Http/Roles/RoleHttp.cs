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
                RequestUri = new Uri(baseURL + APIEndPoints.Role)
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
                throw new CustomHttpException("Error when attempting to GET Roles data from API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "RoleHttp",
                    ExceptionFunction = "GetRoles",
                };
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
                RequestUri = new Uri(baseURL + APIEndPoints.Role + "/" + ID.ToString())
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
                throw new CustomHttpException("Error when attempting to GET Role data from API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "RoleHttp",
                    ExceptionFunction = "GetRole",
                };
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
                RequestUri = new Uri(baseURL + APIEndPoints.Role),
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
                throw new CustomHttpException("Error when attempting to POST Role data to API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "RoleHttp",
                    ExceptionFunction = "CreateRole",
                };
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
                RequestUri = new Uri(baseURL + APIEndPoints.Role),
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
                throw new CustomHttpException("Error when attempting to PUT Role data to API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "RoleHttp",
                    ExceptionFunction = "UpdateRole",
                };
            }
        }
    }
}
