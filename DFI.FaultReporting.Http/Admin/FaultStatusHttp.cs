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
    public class FaultStatusHttp
    {
        public IHttpClientFactory _client { get; }

        public ISettingsService _settings { get; }

        public List<FaultStatus>? FaultStatuses { get; set; }

        public FaultStatusHttp(IHttpClientFactory client, ISettingsService settings)
        {
            _client = client;
            _settings = settings;
        }

        public async Task<List<FaultStatus>> GetFaultStatuses()
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(baseURL + APIEndPoints.FaultStatus)
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                FaultStatuses = JsonConvert.DeserializeObject<List<FaultStatus>>(response);

                return FaultStatuses;
            }
            else
            {
                return null;
            }
        }

        public async Task<FaultStatus> GetFaultStatus(int ID, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(baseURL + APIEndPoints.FaultStatus + "/" + ID.ToString())
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                FaultStatus faultStatus = JsonConvert.DeserializeObject<FaultStatus>(response);

                return faultStatus;
            }
            else
            {
                return null;
            }
        }

        public async Task<FaultStatus> CreateFaultStatus(FaultStatus faultStatus, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var faultStatusJSON = JsonConvert.SerializeObject(faultStatus);

            var content = new StringContent(faultStatusJSON, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(baseURL + APIEndPoints.FaultStatus),
                Content = content
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                faultStatus = JsonConvert.DeserializeObject<FaultStatus>(response);

                return faultStatus;
            }
            else
            {
                return null;
            }
        }

        public async Task<FaultStatus> UpdateFaultStatus(FaultStatus faultStatus, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var faultStatusJSON = JsonConvert.SerializeObject(faultStatus);

            var content = new StringContent(faultStatusJSON, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(baseURL + APIEndPoints.FaultStatus),
                Content = content
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                faultStatus = JsonConvert.DeserializeObject<FaultStatus>(response);

                return faultStatus;
            }
            else
            {
                return null;
            }
        }
    }
}
