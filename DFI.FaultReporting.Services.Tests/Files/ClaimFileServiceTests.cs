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

    public class ClaimFileServiceTests
    {
        private ClaimFileService _testClass;
        private ClaimFileHttp _claimFileHttp;
        private UserHttp _userHttp;

        public string jwtToken = "";

        public ClaimFileServiceTests()
        {
            var mockFactory = new Mock<IHttpClientFactory>();

            var client = new HttpClient();

            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IHttpClientFactory factory = mockFactory.Object;

            var mockSettings = new Mock<ISettingsService>();

            ISettingsService settings = mockSettings.Object;

            _claimFileHttp = new ClaimFileHttp(factory, settings);
            _testClass = new ClaimFileService(_claimFileHttp);
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
            var instance = new ClaimFileService(_claimFileHttp);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public async Task CanCallGetClaimFiles()
        {
            // Arrange
            var token = jwtToken;

            // Act
            var result = await _testClass.GetClaimFiles(token);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetClaimFilesWithInvalidToken(string value)
        {
            var result = await _testClass.GetClaimFiles(value);

            Assert.Null(result);
        }

        [Fact]
        public async Task CanCallGetClaimFile()
        {
            // Arrange
            var ID = 13;
            var token = jwtToken;

            // Act
            var result = await _testClass.GetClaimFile(ID, token);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetClaimFileWithInvalidToken(string value)
        {
            var result = await _testClass.GetClaimFile(13, value);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetClaimFilePerformsMapping()
        {
            // Arrange
            var ID = 13;
            var token = jwtToken;

            // Act
            var result = await _testClass.GetClaimFile(ID, token);

            // Assert
            Assert.Equal(ID, result.ID);
        }

        [Fact]
        public async Task CanCallCreateClaimFile()
        {
            // Arrange
            var claimFile = new ClaimFile
            {
                ID = 0,
                ClaimID = 7,
                Description = "TestValue226489433",
                Type = "Test",
                Data = "TestValue1535224611",
                InputBy = "TestValue1869998083",
                InputOn = DateTime.UtcNow,
                Active = true
            };
            var token = jwtToken;

            // Act
            var result = await _testClass.CreateClaimFile(claimFile, token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallCreateClaimFileWithNullClaimFile()
        {
            var result = await _testClass.CreateClaimFile(default(ClaimFile), "TestValue1753422685");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallCreateClaimFileWithInvalidToken(string value)
        {
            var result = await _testClass.CreateClaimFile(new ClaimFile
            {
                ID = 0,
                ClaimID = 7,
                Description = "TestValue226489433",
                Type = "Test",
                Data = "TestValue1535224611",
                InputBy = "TestValue1869998083",
                InputOn = DateTime.UtcNow,
                Active = true
            }, value);

            Assert.Null(result);
        }

        [Fact]
        public async Task CanCallUpdateClaimFile()
        {
            // Arrange
            var claimFile = new ClaimFile
            {
                ID = 13,
                ClaimID = 7,
                Description = "TestValue646628511",
                Type = "Test",
                Data = "TestValue1626592908",
                InputBy = "TestValue1474223725",
                InputOn = DateTime.UtcNow,
                Active = false
            };
            var token = jwtToken;

            // Act
            var result = await _testClass.UpdateClaimFile(claimFile, token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallUpdateClaimFileWithNullClaimFile()
        {
            var result = await _testClass.UpdateClaimFile(default(ClaimFile), "TestValue655159962");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallUpdateClaimFileWithInvalidToken(string value)
        {
            var result = await _testClass.UpdateClaimFile(new ClaimFile
            {
                ID = 13,
                ClaimID = 7,
                Description = "TestValue646628511",
                Type = "Test",
                Data = "TestValue1626592908",
                InputBy = "TestValue1474223725",
                InputOn = DateTime.UtcNow,
                Active = false
            }, value);

            Assert.Null(result);
        }

        [Fact]
        public void CanSetAndGetClaimFiles()
        {
            // Arrange
            var testValue = new List<ClaimFile>();

            // Act
            _testClass.ClaimFiles = testValue;

            // Assert
            Assert.Same(testValue, _testClass.ClaimFiles);
        }
    }
}