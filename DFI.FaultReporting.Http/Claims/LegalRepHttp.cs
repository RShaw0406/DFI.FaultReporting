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
    public class LegalRepHttp
    {
        public IHttpClientFactory _client { get; }

        public ISettingsService _settings { get; }

        public List<LegalRep>? LegalReps { get; set; }

        public LegalRepHttp(IHttpClientFactory client, ISettingsService settings)
        {
            _client = client;
            _settings = settings;
        }

        public async Task<List<LegalRep>> GetLegalReps(string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(baseURL + APIEndPoints.LegalRep)
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                LegalReps = JsonConvert.DeserializeObject<List<LegalRep>>(response);

                return LegalReps;
            }
            else
            {
                return null;
            }
        }

        public async Task<LegalRep> GetLegalRep(int ID, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(baseURL + APIEndPoints.LegalRep + "/" + ID.ToString())
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                LegalRep legalRep = JsonConvert.DeserializeObject<LegalRep>(response);

                return legalRep;
            }
            else
            {
                return null;
            }
        }

        public async Task<LegalRep> CreateLegalRep(LegalRep legalRep, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(baseURL + APIEndPoints.LegalRep),
                Content = new StringContent(JsonConvert.SerializeObject(legalRep), Encoding.UTF8, "application/json")
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                legalRep = JsonConvert.DeserializeObject<LegalRep>(response);

                return legalRep;
            }
            else
            {
                return null;
            }
        }

        public async Task<LegalRep> UpdateLegalRep(LegalRep legalRep, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(baseURL + APIEndPoints.LegalRep),
                Content = new StringContent(JsonConvert.SerializeObject(legalRep), Encoding.UTF8, "application/json")
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                legalRep = JsonConvert.DeserializeObject<LegalRep>(response);

                return LegalReps.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }
    }
}
