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
    public class ClaimTypeHttp
    {
        public IHttpClientFactory _client { get; }

        public ISettingsService _settings { get; }

        public ClaimTypeHttp(IHttpClientFactory client, ISettingsService settings)
        {
            _client = client;
            _settings = settings;
        }

        public List<ClaimType>? ClaimTypes { get; set; }

        public async Task<List<ClaimType>> GetClaimTypes()
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(baseURL + APIEndPoints.ClaimType)
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                ClaimTypes = JsonConvert.DeserializeObject<List<ClaimType>>(response);

                return ClaimTypes;
            }
            else
            {
                throw new CustomHttpException("Error when attempting to GET Claim Types data from API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "ClaimTypeHttp",
                    ExceptionFunction = "GetClaimType",
                };
            }
        }

        public async Task<ClaimType> GetClaimType(int ID)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(baseURL + APIEndPoints.ClaimType + "/" + ID.ToString())
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                ClaimType claimType = JsonConvert.DeserializeObject<ClaimType>(response);

                return claimType;
            }
            else
            {
                throw new CustomHttpException("Error when attempting to GET Claim Type data from API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "ClaimTypeHttp",
                    ExceptionFunction = "GetClaimType",
                };
            }
        }

        public async Task<ClaimType> CreateClaimType(ClaimType claimType)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            var claimTypeJSON = JsonConvert.SerializeObject(claimType);

            var content = new StringContent(claimTypeJSON, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(baseURL + APIEndPoints.ClaimType),
                Content = content
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                claimType = JsonConvert.DeserializeObject<ClaimType>(response);

                return claimType;
            }
            else
            {
                throw new CustomHttpException("Error when attempting to POST Claim Type data to API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "ClaimTypeHttp",
                    ExceptionFunction = "CreateClaimType",
                };
            }
        }

        public async Task<ClaimType> UpdateClaimType(ClaimType claimType)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            var claimTypeJSON = JsonConvert.SerializeObject(claimType);

            var content = new StringContent(claimTypeJSON, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(baseURL + APIEndPoints.ClaimType),
                Content = content
            };

            //request.Headers.Add("Accept", "application/json");

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                claimType = JsonConvert.DeserializeObject<ClaimType>(response);

                return claimType;
            }
            else
            {
                throw new CustomHttpException("Error when attempting to PUT Claim Type data to API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "ClaimTypeHttp",
                    ExceptionFunction = "PutClaimType",
                };
            }
        }

        public async Task<int> DeleteClaimType(int ID)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(baseURL + APIEndPoints.ClaimType + "/" + ID.ToString())
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                return ID;
            }
            else
            {
                throw new CustomHttpException("Error when attempting to DELETE Claim Type data from API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "ClaimTypeHttp",
                    ExceptionFunction = "DeleteClaimType",
                };
            }
        }
    }
}
