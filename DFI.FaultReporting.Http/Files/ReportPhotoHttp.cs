using DFI.FaultReporting.Common.Constants;
using DFI.FaultReporting.Common.Exceptions;
using DFI.FaultReporting.Models.FaultReports;
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
    public class ReportPhotoHttp
    {
        public IHttpClientFactory _client { get; }

        public ISettingsService _settings { get; }

        public ReportPhotoHttp(IHttpClientFactory client, ISettingsService settings)
        {
            _client = client;
            _settings = settings;
        }

        public List<ReportPhoto>? ReportPhotos { get; set; }

        public async Task<List<ReportPhoto>> GetReportPhotos(string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://localhost:7106" + APIEndPoints.ReportPhoto)
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                ReportPhotos = JsonConvert.DeserializeObject<List<ReportPhoto>>(response);

                return ReportPhotos;
            }
            else
            {
                return null;
            }
        }

        public async Task<ReportPhoto> GetReportPhoto(int ID, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://localhost:7106" + APIEndPoints.ReportPhoto + "/" + ID.ToString())
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                ReportPhoto reportPhoto = JsonConvert.DeserializeObject<ReportPhoto>(response);

                return reportPhoto;
            }
            else
            {
                return null;
            }
        }

        public async Task<ReportPhoto> CreateReportPhoto(ReportPhoto reportPhoto, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var reportPhotoJSON = JsonConvert.SerializeObject(reportPhoto);

            var content = new StringContent(reportPhotoJSON, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://localhost:7106" + APIEndPoints.ReportPhoto),
                Content = content
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                reportPhoto = JsonConvert.DeserializeObject<ReportPhoto>(response);

                return reportPhoto;
            }
            else
            {
                return null;
            }
        }

        public async Task<ReportPhoto> UpdateReportPhoto(ReportPhoto reportPhoto, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var reportPhotoJSON = JsonConvert.SerializeObject(reportPhoto);

            var content = new StringContent(reportPhotoJSON, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri("https://localhost:7106" + APIEndPoints.ReportPhoto),
                Content = content
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                reportPhoto = JsonConvert.DeserializeObject<ReportPhoto>(response);

                return reportPhoto;
            }
            else
            {
                return null;
            }
        }
    }
}
