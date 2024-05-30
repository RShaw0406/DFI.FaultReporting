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
    public class FaultPriorityHttp
    {
        public IHttpClientFactory _client { get; }

        public ISettingsService _settings { get; }

        public FaultPriorityHttp(IHttpClientFactory client, ISettingsService settings)
        {
            _client = client;
            _settings = settings;
        }

        public List<FaultPriority>? FaultPriorities { get; set; }

        public async Task<List<FaultPriority>> GetFaultPriorities()
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(baseURL + APIEndPoints.FaultPriority)
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                FaultPriorities = JsonConvert.DeserializeObject<List<FaultPriority>>(response);

                return FaultPriorities;
            }
            else
            {
                throw new CustomHttpException("Error when attempting to GET Fault Priorities data from API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "FaultPriorityHttp",
                    ExceptionFunction = "GetFaultPriorities",
                };
            }
        }

        public async Task<FaultPriority> GetFaultPriority(int ID)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(baseURL + APIEndPoints.FaultPriority + "/" + ID.ToString())
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                FaultPriority faultPriority = JsonConvert.DeserializeObject<FaultPriority>(response);

                return faultPriority;
            }
            else
            {
                throw new CustomHttpException("Error when attempting to GET Fault Priority data from API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "FaultPriorityHttp",
                    ExceptionFunction = "GetFaultPriority",
                };
            }
        }

        public async Task<FaultPriority> CreateFaultPriority(FaultPriority faultPriority)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            var faultPriorityJSON = JsonConvert.SerializeObject(faultPriority);

            var content = new StringContent(faultPriorityJSON, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(baseURL + APIEndPoints.FaultPriority),
                Content = content
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                faultPriority = JsonConvert.DeserializeObject<FaultPriority>(response);

                return faultPriority;
            }
            else
            {
                throw new CustomHttpException("Error when attempting to POST Fault Priority data to API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "FaultPriorityHttp",
                    ExceptionFunction = "CreateFaultPriority",
                };
            }
        }

        public async Task<FaultPriority> UpdateFaultPriority(FaultPriority faultPriority)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            var faultPriorityJSON = JsonConvert.SerializeObject(faultPriority);

            var content = new StringContent(faultPriorityJSON, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(baseURL + APIEndPoints.FaultPriority),
                Content = content
            };

            //request.Headers.Add("Accept", "application/json");

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                faultPriority = JsonConvert.DeserializeObject<FaultPriority>(response);

                return faultPriority;
            }
            else
            {
                throw new CustomHttpException("Error when attempting to PUT Fault Priority data to API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "FaultPriorityHttp",
                    ExceptionFunction = "UpdateFaultPriority",
                };
            }
        }

        public async Task<int> DeleteFaultPriority(int ID)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(baseURL + APIEndPoints.FaultPriority + "/" + ID.ToString())
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                return ID;
            }
            else
            {
                throw new CustomHttpException("Error when attempting to DELETE Fault Priority data from API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "FaultPriorityHttp",
                    ExceptionFunction = "DeleteFaultPriority",
                };
            }
        }
    }
}
