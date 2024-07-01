using DFI.FaultReporting.Common.Constants;
using DFI.FaultReporting.JWT.Response;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Http.Users
{
    public class StaffHttp
    {
        public IHttpClientFactory _client { get; }

        public ISettingsService _settings { get; }

        public StaffHttp(IHttpClientFactory client, ISettingsService settings)
        {
            _client = client;
            _settings = settings;
        }

        public List<Staff>? Staff { get; set; }

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

        public async Task<List<Staff>> GetStaff(string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(baseURL + APIEndPoints.Staff)
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                Staff = JsonConvert.DeserializeObject<List<Staff>>(response);

                return Staff;
            }
            else
            {
                return null;
            }
        }

        public async Task<Staff> GetStaff(int ID, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(baseURL + APIEndPoints.Staff + "/" + ID)
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                Staff = JsonConvert.DeserializeObject<List<Staff>>(response);

                return Staff.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        public async Task<Staff> CreateStaff(Staff staff, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            var staffJSON = JsonConvert.SerializeObject(staff);

            var content = new StringContent(staffJSON, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(baseURL + APIEndPoints.Staff),
                Content = content
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                Staff = JsonConvert.DeserializeObject<List<Staff>>(response);

                return Staff.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        public async Task<Staff> UpdateStaff(Staff staff, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            var staffJSON = JsonConvert.SerializeObject(staff);

            var content = new StringContent(staffJSON, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(baseURL + APIEndPoints.Staff),
                Content = content
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                Staff = JsonConvert.DeserializeObject<List<Staff>>(response);

                return Staff.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        public async Task<int> DeleteStaff(int ID, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(baseURL + APIEndPoints.Staff + "/" + ID)
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
