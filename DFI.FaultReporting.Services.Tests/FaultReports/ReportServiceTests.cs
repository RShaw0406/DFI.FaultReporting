namespace DFI.FaultReporting.Services.Tests.FaultReports
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using DFI.FaultReporting.Http.FaultReports;
    using DFI.FaultReporting.Http.Users;
    using DFI.FaultReporting.JWT.Requests;
    using DFI.FaultReporting.JWT.Response;
    using DFI.FaultReporting.Models.FaultReports;
    using DFI.FaultReporting.Services.FaultReports;
    using DFI.FaultReporting.Services.Interfaces.Settings;
    using Moq;
    using Xunit;

    public class ReportServiceTests
    {
        private ReportService _testClass;
        private ReportHttp _reportHttp;
        private UserHttp _userHttp;

        public string jwtToken = "";

        public ReportServiceTests()
        {
            var mockFactory = new Mock<IHttpClientFactory>();

            var client = new HttpClient();

            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IHttpClientFactory factory = mockFactory.Object;

            var mockSettings = new Mock<ISettingsService>();

            ISettingsService settings = mockSettings.Object;

            _reportHttp = new ReportHttp(factory, settings);
            _testClass = new ReportService(_reportHttp);
            _userHttp = new UserHttp(factory, settings);

            LoginRequest loginRequest = new LoginRequest
            {
                Email = "test@test.com",
                Password = "Now&then0406"
            };

            AuthResponse authResponse = _userHttp.Login(loginRequest).Result;

            jwtToken = authResponse.Token;
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new ReportService(_reportHttp);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public async Task CanCallGetReports()
        {
            // Act
            var result = await _testClass.GetReports();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CanCallGetReport()
        {
            // Arrange
            var ID = 31;
            var token = jwtToken;

            // Act
            var result = await _testClass.GetReport(ID, token);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetReportWithInvalidToken(string value)
        {
            var result = await _testClass.GetReport(31, value);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetReportPerformsMapping()
        {
            // Arrange
            var ID = 31;
            var token = jwtToken;

            // Act
            var result = await _testClass.GetReport(ID, token);

            // Assert
            Assert.Equal(ID, result.ID);
        }

        [Fact]
        public async Task CanCallCreateReport()
        {
            // Arrange
            var report = new Report
            {
                ID = 0,
                FaultID = 24,
                AdditionalInfo = "TestValue1362532168",
                UserID = 2,
                InputBy = "TestValue1253868792",
                InputOn = DateTime.UtcNow,
                Active = true
            };
            var token = jwtToken;

            // Act
            var result = await _testClass.CreateReport(report, token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallCreateReportWithNullReport()
        {
            var result = await _testClass.CreateReport(default(Report), "TestValue647095899");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallCreateReportWithInvalidToken(string value)
        {
            var result = await _testClass.CreateReport(new Report
            {
                ID = 0,
                FaultID = 24,
                AdditionalInfo = "TestValue1362532168",
                UserID = 2,
                InputBy = "TestValue1253868792",
                InputOn = DateTime.UtcNow,
                Active = true
            }, value);

            Assert.Null(result);
        }

        [Fact]
        public async Task CanCallUpdateReport()
        {
            // Arrange
            var report = new Report
            {
                ID = 31,
                FaultID = 24,
                AdditionalInfo = "TestValue652223427",
                UserID = 2,
                InputBy = "TestValue732703412",
                InputOn = DateTime.UtcNow,
                Active = true
            };
            var token = jwtToken;

            // Act
            var result = await _testClass.UpdateReport(report, token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallUpdateReportWithNullReport()
        {
            var result = await _testClass.UpdateReport(default(Report), "TestValue984476300");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallUpdateReportWithInvalidToken(string value)
        {
            var result = await _testClass.UpdateReport(new Report
            {
                ID = 31,
                FaultID = 24,
                AdditionalInfo = "TestValue652223427",
                UserID = 2,
                InputBy = "TestValue732703412",
                InputOn = DateTime.UtcNow,
                Active = true
            }, value);

            Assert.Null(result);
        }

        [Fact]
        public void CanSetAndGetReports()
        {
            // Arrange
            var testValue = new List<Report>();

            // Act
            _testClass.Reports = testValue;

            // Assert
            Assert.Same(testValue, _testClass.Reports);
        }
    }
}