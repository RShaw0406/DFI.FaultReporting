using DFI.FaultReporting.Common.Constants;
using DFI.FaultReporting.Common.Exceptions;
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
    public class UserRoleHttp
    {
        public IHttpClientFactory _client { get; }

        public ISettingsService _settings { get; }

        public UserRoleHttp(IHttpClientFactory client, ISettingsService settings)
        {
            _client = client;
            _settings = settings;
        }

        public List<UserRole>? UserRoles { get; set; }

        public async Task<List<UserRole>> GetUserRoles()
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(baseURL + APIEndPoints.UserRole)
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                UserRoles = JsonConvert.DeserializeObject<List<UserRole>>(response);

                return UserRoles;
            }
            else
            {
                throw new CustomHttpException("Error when attempting to GET User Roles data from API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "UserRoleHttp",
                    ExceptionFunction = "GetUserRoles",
                };
            }
        }

        public async Task<UserRole> GetUserRole(int ID)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(baseURL + APIEndPoints.UserRole + "/" + ID.ToString())
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                UserRole userRole = JsonConvert.DeserializeObject<UserRole>(response);

                return userRole;
            }
            else
            {
                throw new CustomHttpException("Error when attempting to GET User Role data from API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "UserRoleHttp",
                    ExceptionFunction = "GetUserRole",
                };
            }
        }

        public async Task<UserRole> CreateUserRole(UserRole userRole)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            var userRoleJSON = JsonConvert.SerializeObject(userRole);

            var content = new StringContent(userRoleJSON, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(baseURL + APIEndPoints.UserRole),
                Content = content
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                userRole = JsonConvert.DeserializeObject<UserRole>(response);

                return userRole;
            }
            else
            {
                throw new CustomHttpException("Error when attempting to POST User Role data to API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "UserRoleHttp",
                    ExceptionFunction = "CreateUserRole",
                };
            }
        }

        public async Task<UserRole> UpdateUserRole(UserRole userRole)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            var userRoleJSON = JsonConvert.SerializeObject(userRole);

            var content = new StringContent(userRoleJSON, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(baseURL + APIEndPoints.UserRole),
                Content = content
            };

            //request.Headers.Add("Accept", "application/json");

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                userRole = JsonConvert.DeserializeObject<UserRole>(response);

                return userRole;
            }
            else
            {
                throw new CustomHttpException("Error when attempting to PUT User Role data to API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "UserRoleHttp",
                    ExceptionFunction = "UpdateUserRole",
                };
            }
        }

        public async Task<int> DeleteUserRole(int ID)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(baseURL + APIEndPoints.UserRole + "/" + ID.ToString())
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                return ID;
            }
            else
            {
                throw new CustomHttpException("Error when attempting to DELETE User Role data from API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "UserRoleHttp",
                    ExceptionFunction = "DeleteUserRole",
                };
            }
        }
    }
}
