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
    public class ClaimPhotoHttp
    {
        public IHttpClientFactory _client { get; }

        public ISettingsService _settings { get; }

        public ClaimPhotoHttp(IHttpClientFactory client, ISettingsService settings)
        {
            _client = client;
            _settings = settings;
        }

        public List<ClaimPhoto>? ClaimPhotos { get; set; }

        public async Task<List<ClaimPhoto>> GetClaimPhotos(string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://localhost:7106" + APIEndPoints.ClaimPhoto)
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                ClaimPhotos = JsonConvert.DeserializeObject<List<ClaimPhoto>>(response);

                return ClaimPhotos;
            }
            else
            {
                return null;
            }
        }

        public async Task<ClaimPhoto> GetClaimPhoto(int ID, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://localhost:7106" + APIEndPoints.ClaimPhoto + "/" + ID.ToString())
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                ClaimPhoto claimPhoto = JsonConvert.DeserializeObject<ClaimPhoto>(response);

                return claimPhoto;
            }
            else
            {
                return null;
            }
        }

        public async Task<ClaimPhoto> CreateClaimPhoto(ClaimPhoto claimPhoto, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://localhost:7106" + APIEndPoints.ClaimPhoto),
                Content = new StringContent(JsonConvert.SerializeObject(claimPhoto), Encoding.UTF8, "application/json")
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                ClaimPhoto newClaimPhoto = JsonConvert.DeserializeObject<ClaimPhoto>(response);

                return newClaimPhoto;
            }
            else
            {
                return null;
            }
        }

        public async Task<ClaimPhoto> UpdateClaimPhoto(ClaimPhoto claimPhoto, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri("https://localhost:7106" + APIEndPoints.ClaimPhoto),
                Content = new StringContent(JsonConvert.SerializeObject(claimPhoto), Encoding.UTF8, "application/json")
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                ClaimPhoto updatedClaimPhoto = JsonConvert.DeserializeObject<ClaimPhoto>(response);

                return updatedClaimPhoto;
            }
            else
            {
                return null;
            }
        }
    }
}
