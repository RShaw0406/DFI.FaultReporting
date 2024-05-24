using DFI.FaultReporting.Common.Constants;
using DFI.FaultReporting.Common.Exceptions;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Services.Interfaces.Settings;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Http.Admin
{
    public class ClaimStatusHttp
    {
        public IHttpClientFactory _client { get; }

        public ISettingsService _settings { get; }

        public List<ClaimStatus>? ClaimStatuses { get; set; }

        public ClaimStatusHttp(IHttpClientFactory client, ISettingsService settings)
        {
            _client = client;
            _settings = settings;
        }

        public async Task<List<ClaimStatus>> GetClaimStatuses()
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(baseURL + APIEndPoints.ClaimStatus)
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                ClaimStatuses = JsonConvert.DeserializeObject<List<ClaimStatus>>(response);

                return ClaimStatuses;
            }
            else
            {
                throw new CustomHttpException("Error when attempting to GET Claim Statuses data from API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "ClaimStatusHttp",
                    ExceptionFunction = "GetClaimStatuses",
                };
            }
        }

        public async Task<ClaimStatus> GetClaimStatus(int ID)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(baseURL + APIEndPoints.ClaimStatus + "/" + ID.ToString())
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode) 
            {
                var response = await result.Content.ReadAsStringAsync();

                ClaimStatus claimStatus = JsonConvert.DeserializeObject<ClaimStatus>(response);

                return claimStatus;
            }
            else
            {
                throw new CustomHttpException("Error when attempting to GET Claim Status data from API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "ClaimStatusHttp",
                    ExceptionFunction = "GetClaimStatus",
                };
            }
        }

        public async Task<ClaimStatus> CreateClaimStatus(ClaimStatus claimStatus)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            var claimStatusJSON = JsonConvert.SerializeObject(claimStatus);

            var content = new StringContent(claimStatusJSON, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(baseURL + APIEndPoints.ClaimStatus),
                Content = content
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                claimStatus = JsonConvert.DeserializeObject<ClaimStatus>(response);

                return claimStatus;
            }
            else
            {
                throw new CustomHttpException("Error when attempting to POST Claim Status data to API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "ClaimStatusHttp",
                    ExceptionFunction = "CreateClaimStatus",
                };
            }
        }

        public async Task<ClaimStatus> UpdateClaimStatus(ClaimStatus claimStatus)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            var claimStatusJSON = JsonConvert.SerializeObject(claimStatus);

            var content = new StringContent(claimStatusJSON, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(baseURL + APIEndPoints.ClaimStatus),
                Content = content
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                claimStatus = JsonConvert.DeserializeObject<ClaimStatus>(response);

                return claimStatus;
            }
            else
            {
                throw new CustomHttpException("Error when attempting to PUT Claim Status data to API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "ClaimStatusHttp",
                    ExceptionFunction = "UpdateClaimStatus",
                };
            }
        }

        public async Task<int> DeleteClaimStatus(int ID)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(baseURL + APIEndPoints.ClaimStatus + "/" + ID.ToString())
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                return ID;
            }
            else
            {
                throw new CustomHttpException("Error when attempting to DELETE Claim Status data from API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "ClaimStatusHttp",
                    ExceptionFunction = "DeleteClaimStatus",
                };
            }
        }
    }
}
