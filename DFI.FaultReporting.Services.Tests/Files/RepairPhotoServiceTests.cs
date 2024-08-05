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

    public class RepairPhotoServiceTests
    {
        private RepairPhotoService _testClass;
        private RepairPhotoHttp _repairPhotoHttp;
        private UserHttp _userHttp;

        public string jwtToken = "";

        public RepairPhotoServiceTests()
        {
            var mockFactory = new Mock<IHttpClientFactory>();

            var client = new HttpClient();

            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IHttpClientFactory factory = mockFactory.Object;

            var mockSettings = new Mock<ISettingsService>();

            ISettingsService settings = mockSettings.Object;

            _repairPhotoHttp = new RepairPhotoHttp(factory, settings);
            _testClass = new RepairPhotoService(_repairPhotoHttp);
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
            var instance = new RepairPhotoService(_repairPhotoHttp);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public async Task CanCallGetRepairPhotos()
        {
            // Arrange
            var token = jwtToken;

            // Act
            var result = await _testClass.GetRepairPhotos(token);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetRepairPhotosWithInvalidToken(string value)
        {
            var result = await _testClass.GetRepairPhotos(value);

            Assert.Null(result);
        }

        [Fact]
        public async Task CanCallGetRepairPhoto()
        {
            // Arrange
            var ID = 4;
            var token = jwtToken;

            // Act
            var result = await _testClass.GetRepairPhoto(ID, token);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetRepairPhotoWithInvalidToken(string value)
        {
            var result = await _testClass.GetRepairPhoto(4, value);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetRepairPhotoPerformsMapping()
        {
            // Arrange
            var ID = 4;
            var token = jwtToken;

            // Act
            var result = await _testClass.GetRepairPhoto(ID, token);

            // Assert
            Assert.Equal(ID, result.ID);
        }

        [Fact]
        public async Task CanCallCreateRepairPhoto()
        {
            // Arrange
            var repairPhoto = new RepairPhoto
            {
                ID = 0,
                RepairID = 7,
                Description = "TestValue2059240619",
                Type = "Test",
                Data = "TestValue1721530287",
                InputBy = "TestValue700055000",
                InputOn = DateTime.UtcNow,
                Active = false
            };
            var token = jwtToken;

            // Act
            var result = await _testClass.CreateRepairPhoto(repairPhoto, token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallCreateRepairPhotoWithNullRepairPhoto()
        {
            var result = await _testClass.CreateRepairPhoto(default(RepairPhoto), "TestValue1805319127");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallCreateRepairPhotoWithInvalidToken(string value)
        {
            var result = await _testClass.CreateRepairPhoto(new RepairPhoto
            {
                ID = 0,
                RepairID = 7,
                Description = "TestValue2059240619",
                Type = "Test",
                Data = "TestValue1721530287",
                InputBy = "TestValue700055000",
                InputOn = DateTime.UtcNow,
                Active = false
            }, value);

            Assert.Null(result);
        }

        [Fact]
        public async Task CanCallUpdateRepairPhoto()
        {
            // Arrange
            var repairPhoto = new RepairPhoto
            {
                ID = 4,
                RepairID = 7,
                Description = "TestValue280154538",
                Type = "Test",
                Data = "TestValue855644056",
                InputBy = "TestValue1306741110",
                InputOn = DateTime.UtcNow,
                Active = false
            };
            var token = jwtToken;

            // Act
            var result = await _testClass.UpdateRepairPhoto(repairPhoto, token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallUpdateRepairPhotoWithNullRepairPhoto()
        {
            var result = await _testClass.UpdateRepairPhoto(default(RepairPhoto), "TestValue1994436583");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallUpdateRepairPhotoWithInvalidToken(string value)
        {
            var result = await _testClass.UpdateRepairPhoto(new RepairPhoto
            {
                ID = 4,
                RepairID = 7,
                Description = "TestValue280154538",
                Type = "Test",
                Data = "TestValue855644056",
                InputBy = "TestValue1306741110",
                InputOn = DateTime.UtcNow,
                Active = false
            }, value);

            Assert.Null(result);
        }

        [Fact]
        public void CanSetAndGetRepairPhotos()
        {
            // Arrange
            var testValue = new List<RepairPhoto>();

            // Act
            _testClass.RepairPhotos = testValue;

            // Assert
            Assert.Same(testValue, _testClass.RepairPhotos);
        }
    }
}