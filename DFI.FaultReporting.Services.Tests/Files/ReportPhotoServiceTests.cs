namespace DFI.FaultReporting.Services.Tests.Files
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using DFI.FaultReporting.Http.Files;
    using DFI.FaultReporting.Http.Users;
    using DFI.FaultReporting.JWT.Requests;
    using DFI.FaultReporting.JWT.Response;
    using DFI.FaultReporting.Models.Files;
    using DFI.FaultReporting.Services.Files;
    using DFI.FaultReporting.Services.Interfaces.Settings;
    using Moq;
    using Xunit;

    public class ReportPhotoServiceTests
    {
        private ReportPhotoService _testClass;
        private ReportPhotoHttp _reportPhotoHttp;
        private UserHttp _userHttp;

        public string jwtToken = "";

        public ReportPhotoServiceTests()
        {
            var mockFactory = new Mock<IHttpClientFactory>();

            var client = new HttpClient();

            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IHttpClientFactory factory = mockFactory.Object;

            var mockSettings = new Mock<ISettingsService>();

            ISettingsService settings = mockSettings.Object;

            _reportPhotoHttp = new ReportPhotoHttp(factory, settings);
            _testClass = new ReportPhotoService(_reportPhotoHttp);
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
            var instance = new ReportPhotoService(_reportPhotoHttp);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public async Task CanCallGetReportPhotos()
        {
            // Arrange
            var token = jwtToken;

            // Act
            var result = await _testClass.GetReportPhotos(token);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetReportPhotosWithInvalidToken(string value)
        {
            var result = await _testClass.GetReportPhotos(value);

            Assert.Null(result);
        }

        [Fact]
        public async Task CanCallGetReportPhoto()
        {
            // Arrange
            var ID = 34;
            var token = jwtToken;

            // Act
            var result = await _testClass.GetReportPhoto(ID, token);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetReportPhotoWithInvalidToken(string value)
        {
            var result = await _testClass.GetReportPhoto(34, value);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetReportPhotoPerformsMapping()
        {
            // Arrange
            var ID = 34;
            var token = jwtToken;

            // Act
            var result = await _testClass.GetReportPhoto(ID, token);

            // Assert
            Assert.Equal(ID, result.ID);
        }

        [Fact]
        public async Task CanCallCreateReportPhoto()
        {
            // Arrange
            var reportPhoto = new ReportPhoto
            {
                ID = 0,
                ReportID = 31,
                Description = "TestValue125325449",
                Type = "Test",
                Data = "TestValue1839977966",
                InputBy = "TestValue483370190",
                InputOn = DateTime.UtcNow,
                Active = false
            };
            var token = jwtToken;

            // Act
            var result = await _testClass.CreateReportPhoto(reportPhoto, token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallCreateReportPhotoWithNullReportPhoto()
        {
            var result = await _testClass.CreateReportPhoto(default(ReportPhoto), "TestValue247353000");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallCreateReportPhotoWithInvalidToken(string value)
        {
            var result = await _testClass.CreateReportPhoto(new ReportPhoto
            {
                ID = 0,
                ReportID = 31,
                Description = "TestValue125325449",
                Type = "Test",
                Data = "TestValue1839977966",
                InputBy = "TestValue483370190",
                InputOn = DateTime.UtcNow,
                Active = false
            }, value);

            Assert.Null(result);
        }

        [Fact]
        public async Task CanCallUpdateReportPhoto()
        {
            // Arrange
            var reportPhoto = new ReportPhoto
            {
                ID = 34,
                ReportID = 31,
                Description = "TestValue125325449",
                Type = "Test",
                Data = "TestValue1839977966",
                InputBy = "TestValue483370190",
                InputOn = DateTime.UtcNow,
                Active = false
            };
            var token = jwtToken;

            // Act
            var result = await _testClass.UpdateReportPhoto(reportPhoto, token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallUpdateReportPhotoWithNullReportPhoto()
        {
            var result = await _testClass.UpdateReportPhoto(default(ReportPhoto), "TestValue1773963806");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallUpdateReportPhotoWithInvalidToken(string value)
        {
            var result = await _testClass.UpdateReportPhoto(new ReportPhoto
            {
                ID = 34,
                ReportID = 31,
                Description = "TestValue125325449",
                Type = "Test",
                Data = "TestValue1839977966",
                InputBy = "TestValue483370190",
                InputOn = DateTime.UtcNow,
                Active = false
            }, value);

            Assert.Null(result);
        }

        [Fact]
        public void CanSetAndGetReportPhotos()
        {
            // Arrange
            var testValue = new List<ReportPhoto>();

            // Act
            _testClass.ReportPhotos = testValue;

            // Assert
            Assert.Same(testValue, _testClass.ReportPhotos);
        }
    }
}