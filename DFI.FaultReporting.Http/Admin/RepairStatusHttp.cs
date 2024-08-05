using DFI.FaultReporting.Common.Constants;
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
    public class RepairStatusHttp
    {

        public IHttpClientFactory _client { get; }

        public ISettingsService _settings { get; }

        public List<RepairStatus>? RepairStatuses { get; set; }

        public RepairStatusHttp(IHttpClientFactory client, ISettingsService settings)
        {
            _client = client;
            _settings = settings;
        }

        public async Task<List<RepairStatus>> GetRepairStatuses(string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://localhost:7106" + APIEndPoints.RepairStatus)
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                RepairStatuses = JsonConvert.DeserializeObject<List<RepairStatus>>(response);

                return RepairStatuses;
            }
            else
            {
                return null;
            }
        }

        public async Task<RepairStatus> GetRepairStatus(int ID, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://localhost:7106" + APIEndPoints.RepairStatus + "/" + ID.ToString())
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                var repairStatus = JsonConvert.DeserializeObject<RepairStatus>(response);

                return repairStatus;
            }
            else
            {
                return null;
            }
        }

        public async Task<RepairStatus> CreateRepairStatus(RepairStatus repairStatus, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://localhost:7106" + APIEndPoints.RepairStatus),
                Content = new StringContent(JsonConvert.SerializeObject(repairStatus), Encoding.UTF8, "application/json")
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                var newRepairStatus = JsonConvert.DeserializeObject<RepairStatus>(response);

                return newRepairStatus;
            }
            else
            {
                return null;
            }
        }

        public async Task<RepairStatus> UpdateRepairStatus(RepairStatus repairStatus, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri("https://localhost:7106" + APIEndPoints.RepairStatus),
                Content = new StringContent(JsonConvert.SerializeObject(repairStatus), Encoding.UTF8, "application/json")
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                var updatedRepairStatus = JsonConvert.DeserializeObject<RepairStatus>(response);

                return updatedRepairStatus;
            }
            else
            {
                return null;
            }
        }
    }
}
