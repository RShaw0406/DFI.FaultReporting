using DFI.FaultReporting.Common.Constants;
using DFI.FaultReporting.Common.Exceptions;
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
    public class ReportHttp
    {
        public IHttpClientFactory _client { get; }

        public ISettingsService _settings { get; }

        public ReportHttp(IHttpClientFactory client, ISettingsService settings)
        {
            _client = client;
            _settings = settings;
        }

        public List<Report>? Reports { get; set; }

        public async Task<List<Report>> GetReports()
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(baseURL + APIEndPoints.Report)
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                Reports = JsonConvert.DeserializeObject<List<Report>>(response);

                return Reports;
            }
            else
            {
                throw new CustomHttpException("Error when attempting to GET Reports data from API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "ReportHttp",
                    ExceptionFunction = "GetReports",
                };
            }
        }

        public async Task<Report> GetReport(int ID, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(baseURL + APIEndPoints.Report + "/" + ID.ToString())
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                Report report = JsonConvert.DeserializeObject<Report>(response);

                return report;
            }
            else
            {
                throw new CustomHttpException("Error when attempting to GET Report data from API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "ReportHttp",
                    ExceptionFunction = "GetReport",
                };
            }
        }

        public async Task<Report> CreateReport(Report report, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var reportJSON = JsonConvert.SerializeObject(report);

            var content = new StringContent(reportJSON, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(baseURL + APIEndPoints.Report),
                Content = content
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                report = JsonConvert.DeserializeObject<Report>(response);

                return report;
            }
            else
            {
                throw new CustomHttpException("Error when attempting to POST Report data to API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "ReportHttp",
                    ExceptionFunction = "CreateReport",
                };
            }
        }

        public async Task<Report> UpdateReport(Report report, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var reportJSON = JsonConvert.SerializeObject(report);

            var content = new StringContent(reportJSON, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(baseURL + APIEndPoints.Report),
                Content = content
            };

            //request.Headers.Add("Accept", "application/json");

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                report = JsonConvert.DeserializeObject<Report>(response);

                return report;
            }
            else
            {
                throw new CustomHttpException("Error when attempting to PUT Report data to API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "ReportHttp",
                    ExceptionFunction = "UpdateReport",
                };
            }
        }

        public async Task<int> DeleteReport(int ID, string token)
        {
            var baseURL = await _settings.GetSettingString(Settings.APIURL);

            var client = _client.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(baseURL + APIEndPoints.Report + "/" + ID.ToString())
            };

            var result = await client.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                return ID;
            }
            else
            {
                throw new CustomHttpException("Error when attempting to DELETE Report data from API")
                {
                    ResponseStatus = result.StatusCode,
                    ExceptionClass = "ReportHttp",
                    ExceptionFunction = "DeleteReport",
                };
            }
        }
    }
}
