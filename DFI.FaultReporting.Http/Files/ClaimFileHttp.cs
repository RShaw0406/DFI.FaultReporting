using DFI.FaultReporting.Common.Constants;
using DFI.FaultReporting.Models.Files;
using DFI.FaultReporting.Services.Interfaces.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Http.Files
{
    public class ClaimFileHttp
    {
        public IHttpClientFactory _client { get; }

        public ISettingsService _settings { get; }

        public ClaimFileHttp(IHttpClientFactory client, ISettingsService settings)
        {
            _client = client;
            _settings = settings;
        }

        public List<ClaimFile>? ClaimFiles { get; set; }

        public async Task<List<ClaimFile>> GetClaimFiles(string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(baseURL + APIEndPoints.ClaimFile)
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                ClaimFiles = JsonConvert.DeserializeObject<List<ClaimFile>>(response);

                return ClaimFiles;
            }
            else
            {
                return null;
            }
        }

        public async Task<ClaimFile> GetClaimFile(int ID, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(baseURL + APIEndPoints.ClaimFile + "/" + ID.ToString())
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                ClaimFile claimFile = JsonConvert.DeserializeObject<ClaimFile>(response);

                return claimFile;
            }
            else
            {
                return null;
            }
        }

        public async Task<ClaimFile> CreateClaimFile(ClaimFile claimFile, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(baseURL + APIEndPoints.ClaimFile),
                Content = new StringContent(JsonConvert.SerializeObject(claimFile), Encoding.UTF8, "application/json")
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                ClaimFile newClaimFile = JsonConvert.DeserializeObject<ClaimFile>(response);

                return newClaimFile;
            }
            else
            {
                return null;
            }
        }

        public async Task<ClaimFile> UpdateClaimFile(ClaimFile claimFile, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(baseURL + APIEndPoints.ClaimFile),
                Content = new StringContent(JsonConvert.SerializeObject(claimFile), Encoding.UTF8, "application/json")
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                ClaimFile updatedClaimFile = JsonConvert.DeserializeObject<ClaimFile>(response);

                return updatedClaimFile;
            }
            else
            {
                return null;
            }
        }

    }
}
