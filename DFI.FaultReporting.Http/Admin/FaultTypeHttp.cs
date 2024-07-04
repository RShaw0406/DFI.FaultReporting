using DFI.FaultReporting.Common.Constants;
using DFI.FaultReporting.Common.Exceptions;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Services.Interfaces.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Http.Admin
{
    public class FaultTypeHttp
    {
        public IHttpClientFactory _client { get; }

        public ISettingsService _settings { get; }

        public FaultTypeHttp(IHttpClientFactory client, ISettingsService settings)
        {
            _client = client;
            _settings = settings;
        }

        public List<FaultType>? FaultTypes { get; set; }

        public async Task<List<FaultType>> GetFaultTypes()
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(baseURL + APIEndPoints.FaultType)
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                FaultTypes = JsonConvert.DeserializeObject<List<FaultType>>(response);

                return FaultTypes;
            }
            else
            {
                return null;
            }
        }

        public async Task<FaultType> GetFaultType(int ID, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(baseURL + APIEndPoints.FaultType + "/" + ID.ToString())
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                FaultType faultType = JsonConvert.DeserializeObject<FaultType>(response);

                return faultType;
            }
            else
            {
                return null;
            }
        }

        public async Task<FaultType> CreateFaultType(FaultType faultType, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var faultTypeJSON = JsonConvert.SerializeObject(faultType);

            var content = new StringContent(faultTypeJSON, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(baseURL + APIEndPoints.FaultType),
                Content = content
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                faultType = JsonConvert.DeserializeObject<FaultType>(response);

                return faultType;
            }
            else
            {
                return null;
            }
        }

        public async Task<FaultType> UpdateFaultType(FaultType faultType, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var faultTypeJSON = JsonConvert.SerializeObject(faultType);

            var content = new StringContent(faultTypeJSON, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(baseURL + APIEndPoints.FaultType),
                Content = content
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                faultType = JsonConvert.DeserializeObject<FaultType>(response);

                return faultType;
            }
            else
            {
                return null;
            }
        }
    }
}
