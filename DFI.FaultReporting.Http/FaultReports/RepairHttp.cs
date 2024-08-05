using DFI.FaultReporting.Common.Constants;
using DFI.FaultReporting.Models.FaultReports;
using DFI.FaultReporting.Services.Interfaces.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Http.FaultReports
{
    public class RepairHttp
    {
        public IHttpClientFactory _client { get; }

        public ISettingsService _settings { get; }

        public RepairHttp(IHttpClientFactory client, ISettingsService settings)
        {
            _client = client;
            _settings = settings;
        }

        public List<Repair>? Repairs { get; set; }

        public async Task<List<Repair>> GetRepairs(string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://localhost:7106" + APIEndPoints.Repair)
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                Repairs = JsonConvert.DeserializeObject<List<Repair>>(response);

                return Repairs;
            }
            else
            {
                return null;
            }
        }

        public async Task<Repair> GetRepair(int ID, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://localhost:7106" + APIEndPoints.Repair + "/" + ID.ToString())
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                var repair = JsonConvert.DeserializeObject<Repair>(response);

                return repair;
            }
            else
            {
                return null;
            }
        }

        public async Task<Repair> CreateRepair(Repair repair, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://localhost:7106" + APIEndPoints.Repair),
                Content = new StringContent(JsonConvert.SerializeObject(repair), Encoding.UTF8, "application/json")
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                repair = JsonConvert.DeserializeObject<Repair>(response);

                return repair;
            }
            else
            {
                return null;
            }
        }

        public async Task<Repair> UpdateRepair(Repair repair, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri("https://localhost:7106" + APIEndPoints.Repair),
                Content = new StringContent(JsonConvert.SerializeObject(repair), Encoding.UTF8, "application/json")
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                repair = JsonConvert.DeserializeObject<Repair>(response);

                return repair;
            }
            else
            {
                return null;
            }
        }
    }
}
