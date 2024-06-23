using DFI.FaultReporting.Common.Constants;
using DFI.FaultReporting.Common.Exceptions;
using DFI.FaultReporting.JWT;
using DFI.FaultReporting.JWT.Requests;
using DFI.FaultReporting.JWT.Response;
using DFI.FaultReporting.Models.Roles;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Settings;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Http.Users
{
    public class UserHttp
    {
        public IHttpClientFactory _client { get; }

        public ISettingsService _settings { get; }

        public UserHttp(IHttpClientFactory client, ISettingsService settings)
        {
            _client = client;
            _settings = settings;
        }

        public List<User>? Users { get; set; }

        public async Task<AuthResponse> Register(RegistrationRequest registrationRequest)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            var registrationRequestJSON = JsonConvert.SerializeObject(registrationRequest);

            var content = new StringContent(registrationRequestJSON, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(baseURL + APIEndPoints.AuthRegister),
                Content = content
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                AuthResponse authResponse = JsonConvert.DeserializeObject<AuthResponse>(response);

                return authResponse;
            }
            else
            {
                return null;
            }
        }

        public async Task<AuthResponse> Login(JWT.Requests.LoginRequest loginRequest)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            var loginRequestJSON = JsonConvert.SerializeObject(loginRequest);

            var content = new StringContent(loginRequestJSON, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(baseURL + APIEndPoints.AuthLogin),
                Content = content
            };

            var result = await client.SendAsync(request);

            var response = await result.Content.ReadAsStringAsync();

            AuthResponse authResponse = JsonConvert.DeserializeObject<AuthResponse>(response);

            return authResponse;
        }

        public async Task<AuthResponse> Lock(string emailAddress)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            var lockJSON = JsonConvert.SerializeObject(emailAddress);

            var content = new StringContent(lockJSON, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(baseURL + APIEndPoints.AuthLock),
                Content = content
            };

            var result = await client.SendAsync(request);

            var response = await result.Content.ReadAsStringAsync();

            AuthResponse authResponse = JsonConvert.DeserializeObject<AuthResponse>(response);

            return authResponse;
        }

        public async Task<List<User>> GetUsers(string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(baseURL + APIEndPoints.User)
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                Users = JsonConvert.DeserializeObject<List<User>>(response);

                return Users;
            }
            else
            {
                return null;
            }
        }

        public async Task<User> GetUser(int ID, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(baseURL + APIEndPoints.User + "/" + ID.ToString())
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                User user = JsonConvert.DeserializeObject<User>(response);

                return user;
            }
            else
            {
                return null;
            }
        }

        public async Task<User> CreateUser(User user)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            var userJSON = JsonConvert.SerializeObject(user);

            var content = new StringContent(userJSON, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(baseURL + APIEndPoints.User),
                Content = content
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                user = JsonConvert.DeserializeObject<User>(response);

                return user;
            }
            else
            {
                return null;
            }
        }

        public async Task<User> UpdateUser(User user, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var userJSON = JsonConvert.SerializeObject(user);

            var content = new StringContent(userJSON, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(baseURL + APIEndPoints.User),
                Content = content
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                user = JsonConvert.DeserializeObject<User>(response);

                return user;
            }
            else
            {
                return null;
            }
        }

        public async Task<int> DeleteUser(int ID, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(baseURL + APIEndPoints.User + "/" + ID.ToString())
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
