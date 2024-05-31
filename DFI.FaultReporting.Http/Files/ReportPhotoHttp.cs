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

        public async Task<List<ReportPhoto>> GetReportPhotos()
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(baseURL + APIEndPoints.ReportPhoto)
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
                throw new CustomHttpException("Error when attempting to GET Report Photos data from API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "ReportPhotoHttp",
                    ExceptionFunction = "GetReportPhotos",
                };
            }
        }

        public async Task<ReportPhoto> GetReportPhoto(int ID)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(baseURL + APIEndPoints.ReportPhoto + "/" + ID.ToString())
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
                throw new CustomHttpException("Error when attempting to GET Report Photo data from API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "ReportPhotoHttp",
                    ExceptionFunction = "GetReportPhoto",
                };
            }
        }

        public async Task<ReportPhoto> CreateReportPhoto(ReportPhoto reportPhoto)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            var reportPhotoJSON = JsonConvert.SerializeObject(reportPhoto);

            var content = new StringContent(reportPhotoJSON, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(baseURL + APIEndPoints.ReportPhoto),
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
                throw new CustomHttpException("Error when attempting to POST Report Photo data to API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "ReportPhotoHttp",
                    ExceptionFunction = "CreateReportPhoto",
                };
            }
        }

        public async Task<ReportPhoto> UpdateReportPhoto(ReportPhoto reportPhoto)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            var reportPhotoJSON = JsonConvert.SerializeObject(reportPhoto);

            var content = new StringContent(reportPhotoJSON, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(baseURL + APIEndPoints.ReportPhoto),
                Content = content
            };

            //request.Headers.Add("Accept", "application/json");

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                reportPhoto = JsonConvert.DeserializeObject<ReportPhoto>(response);

                return reportPhoto;
            }
            else
            {
                throw new CustomHttpException("Error when attempting to PUT Report Photo data to API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "ReportPhotoHttp",
                    ExceptionFunction = "UpdateReportPhoto",
                };
            }
        }

        public async Task<int> DeleteReportPhoto(int ID)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(baseURL + APIEndPoints.ReportPhoto + "/" + ID.ToString())
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                return ID;
            }
            else
            {
                throw new CustomHttpException("Error when attempting to DELETE Report Photo data from API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "ReportPhotoHttp",
                    ExceptionFunction = "DeleteReportPhoto",
                };
            }
        }
    }
}
