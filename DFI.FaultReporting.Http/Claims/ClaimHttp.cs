using DFI.FaultReporting.Common.Constants;
using DFI.FaultReporting.Models.Claims;
using DFI.FaultReporting.Services.Interfaces.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Http.Claims
{
    public class ClaimHttp
    {
        public IHttpClientFactory _client { get; }

        public ISettingsService _settings { get; }

        public List<Claim>? Claims { get; set; }

        public ClaimHttp(IHttpClientFactory client, ISettingsService settings)
        {
            _client = client;
            _settings = settings;
        }

        public async Task<List<Claim>> GetClaims(string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://localhost:7106" + APIEndPoints.Claim)
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                Claims = JsonConvert.DeserializeObject<List<Claim>>(response);

                return Claims;
            }
            else
            {
                return null;
            }
        }

        public async Task<Claim> GetClaim(int ID, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://localhost:7106" + APIEndPoints.Claim + "/" + ID.ToString())
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                Claim claim = JsonConvert.DeserializeObject<Claim>(response);

                return claim;
            }
            else
            {
                return null;
            }
        }

        public async Task<Claim> CreateClaim(Claim claim, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://localhost:7106" + APIEndPoints.Claim),
                Content = new StringContent(JsonConvert.SerializeObject(claim), Encoding.UTF8, "application/json")
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                claim = JsonConvert.DeserializeObject<Claim>(response);

                return claim;
            }
            else
            {
                return null;
            }
        }

        public async Task<Claim> UpdateClaim(Claim claim, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri("https://localhost:7106" + APIEndPoints.Claim),
                Content = new StringContent(JsonConvert.SerializeObject(claim), Encoding.UTF8, "application/json")
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                claim = JsonConvert.DeserializeObject<Claim>(response);

                return claim;
            }
            else
            {
                return null;
            }
        }
    }
}
