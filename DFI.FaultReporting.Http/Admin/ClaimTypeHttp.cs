using DFI.FaultReporting.Common.Constants;
using DFI.FaultReporting.Common.Exceptions;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Services.Interfaces.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Http.Admin
{
    public class ClaimTypeHttp
    {
        public IHttpClientFactory _client { get; }

        public ISettingsService _settings { get; }

        public ClaimTypeHttp(IHttpClientFactory client, ISettingsService settings)
        {
            _client = client;
            _settings = settings;
        }

        public List<ClaimType>? ClaimTypes { get; set; }

        public async Task<List<ClaimType>> GetClaimTypes(string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://localhost:7106" + APIEndPoints.ClaimType)
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                ClaimTypes = JsonConvert.DeserializeObject<List<ClaimType>>(response);

                return ClaimTypes;
            }
            else
            {
                return null;
            }
        }

        public async Task<ClaimType> GetClaimType(int ID, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://localhost:7106" + APIEndPoints.ClaimType + "/" + ID.ToString())
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                ClaimType claimType = JsonConvert.DeserializeObject<ClaimType>(response);

                return claimType;
            }
            else
            {
                return null;
            }
        }

        public async Task<ClaimType> CreateClaimType(ClaimType claimType, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var claimTypeJSON = JsonConvert.SerializeObject(claimType);

            var content = new StringContent(claimTypeJSON, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://localhost:7106" + APIEndPoints.ClaimType),
                Content = content
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                claimType = JsonConvert.DeserializeObject<ClaimType>(response);

                return claimType;
            }
            else
            {
                return null;
            }
        }

        public async Task<ClaimType> UpdateClaimType(ClaimType claimType, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var claimTypeJSON = JsonConvert.SerializeObject(claimType);

            var content = new StringContent(claimTypeJSON, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri("https://localhost:7106" + APIEndPoints.ClaimType),
                Content = content
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                claimType = JsonConvert.DeserializeObject<ClaimType>(response);

                return claimType;
            }
            else
            {
                return null;
            }
        }
    }
}
