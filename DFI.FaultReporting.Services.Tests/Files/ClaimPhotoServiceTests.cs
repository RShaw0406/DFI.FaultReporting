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

    public class ClaimPhotoServiceTests
    {
        private ClaimPhotoService _testClass;
        private ClaimPhotoHttp _claimPhotoHttp;
        private UserHttp _userHttp;

        public string jwtToken = "";

        public ClaimPhotoServiceTests()
        {
            var mockFactory = new Mock<IHttpClientFactory>();

            var client = new HttpClient();

            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IHttpClientFactory factory = mockFactory.Object;

            var mockSettings = new Mock<ISettingsService>();

            ISettingsService settings = mockSettings.Object;

            _claimPhotoHttp = new ClaimPhotoHttp(factory, settings);
            _testClass = new ClaimPhotoService(_claimPhotoHttp);
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
            var instance = new ClaimPhotoService(_claimPhotoHttp);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public async Task CanCallGetClaimPhotos()
        {
            // Arrange
            var token = jwtToken;

            // Act
            var result = await _testClass.GetClaimPhotos(token);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetClaimPhotosWithInvalidToken(string value)
        {
            var result = await _testClass.GetClaimPhotos(value);

            Assert.Null(result);
        }

        [Fact]
        public async Task CanCallGetClaimPhoto()
        {
            // Arrange
            var ID = 7;
            var token = jwtToken;

            // Act
            var result = await _testClass.GetClaimPhoto(ID, token);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetClaimPhotoWithInvalidToken(string value)
        {
            var result = await _testClass.GetClaimPhoto(7, value);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetClaimPhotoPerformsMapping()
        {
            // Arrange
            var ID = 7;
            var token = jwtToken;

            // Act
            var result = await _testClass.GetClaimPhoto(ID, token);

            // Assert
            Assert.Equal(ID, result.ID);
        }

        [Fact]
        public async Task CanCallCreateClaimPhoto()
        {
            // Arrange
            var claimPhoto = new ClaimPhoto
            {
                ID = 0,
                ClaimID = 7,
                Description = "TestValue350894881",
                Type = "Test",
                Data = "TestValue289693940",
                InputBy = "TestValue78832740",
                InputOn = DateTime.UtcNow,
                Active = false
            };

            var token = jwtToken;

            // Act
            var result = await _testClass.CreateClaimPhoto(claimPhoto, token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallCreateClaimPhotoWithNullClaimPhoto()
        {
            var result = await _testClass.CreateClaimPhoto(default(ClaimPhoto), "TestValue1412321795");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallCreateClaimPhotoWithInvalidToken(string value)
        {
            var result = await _testClass.CreateClaimPhoto(new ClaimPhoto
            {
                ID = 0,
                ClaimID = 7,
                Description = "TestValue350894881",
                Type = "Test",
                Data = "TestValue289693940",
                InputBy = "TestValue78832740",
                InputOn = DateTime.UtcNow,
                Active = false
            }, value);

            Assert.Null(result);
        }

        [Fact]
        public async Task CanCallUpdateClaimPhoto()
        {
            // Arrange
            var claimPhoto = new ClaimPhoto
            {
                ID = 7,
                ClaimID = 7,
                Description = "TestValue350894881",
                Type = "Test",
                Data = "TestValue289693940",
                InputBy = "TestValue78832740",
                InputOn = DateTime.UtcNow,
                Active = false
            };
            var token = jwtToken;

            // Act
            var result = await _testClass.UpdateClaimPhoto(claimPhoto, token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallUpdateClaimPhotoWithNullClaimPhoto()
        {
            var result = await _testClass.UpdateClaimPhoto(default(ClaimPhoto), "TestValue672534973");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallUpdateClaimPhotoWithInvalidToken(string value)
        {
            var result = await _testClass.UpdateClaimPhoto(new ClaimPhoto
            {
                ID = 7,
                ClaimID = 7,
                Description = "TestValue350894881",
                Type = "Test",
                Data = "TestValue289693940",
                InputBy = "TestValue78832740",
                InputOn = DateTime.UtcNow,
                Active = false
            }, value);

            Assert.Null(result);
        }

        [Fact]
        public void CanSetAndGetClaimPhotos()
        {
            // Arrange
            var testValue = new List<ClaimPhoto>();

            // Act
            _testClass.ClaimPhotos = testValue;

            // Assert
            Assert.Same(testValue, _testClass.ClaimPhotos);
        }
    }
}