﻿using DFI.FaultReporting.Common.Constants;
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
                RequestUri = new Uri("https://localhost:7106" + APIEndPoints.FaultPriority)
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
                return null;    
            }
        }

        public async Task<FaultPriority> GetFaultPriority(int ID, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://localhost:7106" + APIEndPoints.FaultPriority + "/" + ID.ToString())
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
                return null;
            }
        }

        public async Task<FaultPriority> CreateFaultPriority(FaultPriority faultPriority, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var faultPriorityJSON = JsonConvert.SerializeObject(faultPriority);

            var content = new StringContent(faultPriorityJSON, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://localhost:7106" + APIEndPoints.FaultPriority),
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
                return null;
            }
        }

        public async Task<FaultPriority> UpdateFaultPriority(FaultPriority faultPriority, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var faultPriorityJSON = JsonConvert.SerializeObject(faultPriority);

            var content = new StringContent(faultPriorityJSON, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri("https://localhost:7106" + APIEndPoints.FaultPriority),
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
                return null;
            }
        }
    }
}
