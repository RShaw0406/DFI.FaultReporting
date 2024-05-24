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
                throw new CustomHttpException("Error when attempting to GET Fault Statuses data from API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "FaultStatusHttp",
                    ExceptionFunction = "GetFaultStatuses",
                };
            }
        }

        public async Task<FaultStatus> GetFaultStatus(int ID)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

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
                throw new CustomHttpException("Error when attempting to GET Fault Status data from API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "FaultStatusHttp",
                    ExceptionFunction = "GetFaultStatus",
                };
            }
        }

        public async Task<FaultStatus> CreateFaultStatus(FaultStatus faultStatus)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

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
                throw new CustomHttpException("Error when attempting to POST Fault Status data to API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "FaultStatusHttp",
                    ExceptionFunction = "CreateFaultStatus",
                };
            }
        }

        public async Task<FaultStatus> UpdateFaultStatus(FaultStatus faultStatus)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

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
                throw new CustomHttpException("Error when attempting to PUT Fault Status data to API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "FaultStatusHttp",
                    ExceptionFunction = "UpdateFaultStatus",
                };
            }
        }

        public async Task<int> DeleteFaultStatus(int ID)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(baseURL + APIEndPoints.FaultStatus + "/" + ID.ToString())
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                return ID;
            }
            else
            {
                throw new CustomHttpException("Error when attempting to DELETE Fault Status data from API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "FaultStatusHttp",
                    ExceptionFunction = "DeleteFaultStatus",
                };
            }
        }
    }
}
