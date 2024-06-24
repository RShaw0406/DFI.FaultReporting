using DFI.FaultReporting.Common.Constants;
using DFI.FaultReporting.Common.Exceptions;
using DFI.FaultReporting.Models.Admin;
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
    public class FaultHttp
    {
        public IHttpClientFactory _client { get; }

        public ISettingsService _settings { get; }

        public FaultHttp(IHttpClientFactory client, ISettingsService settings)
        {
            _client = client;
            _settings = settings;
        }

        public List<Fault>? Faults { get; set; }

        public async Task<List<Fault>> GetFaults()
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(baseURL + APIEndPoints.Fault)
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                Faults = JsonConvert.DeserializeObject<List<Fault>>(response);

                return Faults;
            }
            else
            {
                throw new CustomHttpException("Error when attempting to GET Faults data from API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "FaultHttp",
                    ExceptionFunction = "GetFaults",
                };
            }
        }

        public async Task<Fault> GetFault(int ID, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(baseURL + APIEndPoints.Fault + "/" + ID.ToString())
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                Fault fault = JsonConvert.DeserializeObject<Fault>(response);

                return fault;
            }
            else
            {
                throw new CustomHttpException("Error when attempting to GET Fault data from API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "FaultHttp",
                    ExceptionFunction = "GetFault",
                };
            }
        }

        public async Task<Fault> CreateFault(Fault fault, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var faultJSON = JsonConvert.SerializeObject(fault);

            var content = new StringContent(faultJSON, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(baseURL + APIEndPoints.Fault),
                Content = content
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                fault = JsonConvert.DeserializeObject<Fault>(response);

                return fault;
            }
            else
            {
                throw new CustomHttpException("Error when attempting to POST Fault data to API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "FaultHttp",
                    ExceptionFunction = "CreateFault",
                };
            }
        }

        public async Task<Fault> UpdateFault(Fault fault, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var faultJSON = JsonConvert.SerializeObject(fault);

            var content = new StringContent(faultJSON, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(baseURL + APIEndPoints.Fault),
                Content = content
            };

            //request.Headers.Add("Accept", "application/json");

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                fault = JsonConvert.DeserializeObject<Fault>(response);

                return fault;
            }
            else
            {
                throw new CustomHttpException("Error when attempting to PUT Fault data to API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "FaultHttp",
                    ExceptionFunction = "UpdateFault",
                };
            }
        }

        public async Task<int> DeleteFault(int ID, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(baseURL + APIEndPoints.Fault + "/" + ID.ToString())
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                return ID;
            }
            else
            {
                throw new CustomHttpException("Error when attempting to DELETE Fault data from API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "FaultHttp",
                    ExceptionFunction = "DeleteFault",
                };
            }
        }
    }
}
