using DFI.FaultReporting.Common.Constants;
using DFI.FaultReporting.Common.Exceptions;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Services.Interfaces.Settings;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Http.Admin
{
    public class ClaimStatusHttp
    {
        public IHttpClientFactory _client { get; }

        public ISettingsService _settings { get; }

        public List<ClaimStatus>? ClaimStatuses { get; set; }

        public ClaimStatusHttp(IHttpClientFactory client, ISettingsService settings)
        {
            _client = client;
            _settings = settings;
        }

        public async Task<List<ClaimStatus>> GetClaimStatuses(string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://localhost:7106" + APIEndPoints.ClaimStatus)
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                ClaimStatuses = JsonConvert.DeserializeObject<List<ClaimStatus>>(response);

                return ClaimStatuses;
            }
            else
            {
                return null;
            }
        }

        public async Task<ClaimStatus> GetClaimStatus(int ID, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://localhost:7106" + APIEndPoints.ClaimStatus + "/" + ID.ToString())
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode) 
            {
                var response = await result.Content.ReadAsStringAsync();

                ClaimStatus claimStatus = JsonConvert.DeserializeObject<ClaimStatus>(response);

                return claimStatus;
            }
            else
            {
                return null;
            }
        }

        public async Task<ClaimStatus> CreateClaimStatus(ClaimStatus claimStatus, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var claimStatusJSON = JsonConvert.SerializeObject(claimStatus);

            var content = new StringContent(claimStatusJSON, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://localhost:7106" + APIEndPoints.ClaimStatus),
                Content = content
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                claimStatus = JsonConvert.DeserializeObject<ClaimStatus>(response);

                return claimStatus;
            }
            else
            {
                return null;
            }
        }

        public async Task<ClaimStatus> UpdateClaimStatus(ClaimStatus claimStatus, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var claimStatusJSON = JsonConvert.SerializeObject(claimStatus);

            var content = new StringContent(claimStatusJSON, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri("https://localhost:7106" + APIEndPoints.ClaimStatus),
                Content = content
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                claimStatus = JsonConvert.DeserializeObject<ClaimStatus>(response);

                return claimStatus;
            }
            else
            {
                return null;
            }
        }
    }
}
