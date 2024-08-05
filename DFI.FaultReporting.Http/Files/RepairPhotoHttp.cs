using DFI.FaultReporting.Common.Constants;
using DFI.FaultReporting.Models.Files;
using DFI.FaultReporting.Services.Interfaces.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Http.Files
{
    public class RepairPhotoHttp
    {
        public IHttpClientFactory _client { get; }

        public ISettingsService _settings { get; }

        public RepairPhotoHttp(IHttpClientFactory client, ISettingsService settings)
        {
            _client = client;
            _settings = settings;
        }

        public List<RepairPhoto>? RepairPhotos { get; set; }

        public async Task<List<RepairPhoto>> GetRepairPhotos(string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://localhost:7106" + APIEndPoints.RepairPhoto)
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                RepairPhotos = JsonConvert.DeserializeObject<List<RepairPhoto>>(response);

                return RepairPhotos;
            }
            else
            {
                return null;
            }
        }

        public async Task<RepairPhoto> GetRepairPhoto(int ID, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://localhost:7106" + APIEndPoints.RepairPhoto + "/" + ID.ToString())
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                RepairPhoto repairPhoto = JsonConvert.DeserializeObject<RepairPhoto>(response);

                return repairPhoto;
            }
            else
            {
                return null;
            }
        }

        public async Task<RepairPhoto> CreateRepairPhoto(RepairPhoto repairPhoto, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://localhost:7106" + APIEndPoints.RepairPhoto),
                Content = new StringContent(JsonConvert.SerializeObject(repairPhoto), Encoding.UTF8, "application/json")
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                repairPhoto = JsonConvert.DeserializeObject<RepairPhoto>(response);

                return repairPhoto;
            }
            else
            {
                return null;
            }
        }

        public async Task<RepairPhoto> UpdateRepairPhoto(RepairPhoto repairPhoto, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri("https://localhost:7106" + APIEndPoints.RepairPhoto),
                Content = new StringContent(JsonConvert.SerializeObject(repairPhoto), Encoding.UTF8, "application/json")
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                repairPhoto = JsonConvert.DeserializeObject<RepairPhoto>(response);

                return repairPhoto;
            }
            else
            {
                return null;
            }
        }
    }
}
