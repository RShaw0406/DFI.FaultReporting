namespace DFI.FaultReporting.Services.Tests.Admin
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Castle.Core.Configuration;
    using DFI.FaultReporting.Http.Admin;
    using DFI.FaultReporting.Http.Users;
    using DFI.FaultReporting.JWT.Requests;
    using DFI.FaultReporting.JWT.Response;
    using DFI.FaultReporting.Models.Admin;
    using DFI.FaultReporting.Services.Admin;
    using DFI.FaultReporting.Services.Interfaces.Settings;
    using DFI.FaultReporting.Services.Settings;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Xunit;

    public class ClaimStatusServiceTests
    {
        private ClaimStatusService _testClass;
        private ClaimStatusHttp _claimStatusHttp;
        private UserHttp _userHttp;

        public string jwtToken = "";


        public ClaimStatusServiceTests()
        {
            var mockFactory = new Mock<IHttpClientFactory>();

            var client = new HttpClient();

            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IHttpClientFactory factory = mockFactory.Object;

            var mockSettings = new Mock<ISettingsService>();

            ISettingsService settings = mockSettings.Object;

            _claimStatusHttp = new ClaimStatusHttp(factory, settings);
            _testClass = new ClaimStatusService(_claimStatusHttp);
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
            var instance = new ClaimStatusService(_claimStatusHttp);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public async Task CanCallGetClaimStatuses()
        {
            // Arrange
            var token = jwtToken;

            // Act
            var result = await _testClass.GetClaimStatuses(token);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetClaimStatusesWithInvalidToken(string value)
        {
            var result = await _testClass.GetClaimStatuses(value);

            Assert.Null(result);
        }

        [Fact]
        public async Task CanCallGetClaimStatus()
        {
            // Arrange
            var ID = 8;
            var token = jwtToken;

            // Act
            var result = await _testClass.GetClaimStatus(ID, token);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetClaimStatusWithInvalidToken(string value)
        {
            var result = await _testClass.GetClaimStatuses(value);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetClaimStatusPerformsMapping()
        {
            // Arrange
            var ID = 8;
            var token = jwtToken;

            // Act
            var result = await _testClass.GetClaimStatus(ID, token);

            // Assert
            Assert.Equal(ID, result.ID);
        }

        [Fact]
        public async Task CanCallCreateClaimStatus()
        {
            // Arrange
            var claimStatus = new ClaimStatus
            {
                ID = 0,
                ClaimStatusDescription = "TestValue1766930736",
                InputBy = "TestValue1193060530",
                InputOn = DateTime.UtcNow,
                Active = false
            };
            var token = jwtToken;

            // Act
            var result = await _testClass.CreateClaimStatus(claimStatus, token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallCreateClaimStatusWithNullClaimStatus()
        {
            var result = await _testClass.CreateClaimStatus(default(ClaimStatus), "TestValue2087941299");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallCreateClaimStatusWithInvalidToken(string value)
        {
            var result = await _testClass.CreateClaimStatus(new ClaimStatus
            {
                ID = 0,
                ClaimStatusDescription = "TestValue941574019",
                InputBy = "TestValue576365511",
                InputOn = DateTime.UtcNow,
                Active = true
            }, value);

            Assert.Null(result);
        }

        [Fact]
        public async Task CanCallUpdateClaimStatus()
        {
            // Arrange
            var claimStatus = new ClaimStatus
            {
                ID = 12,
                ClaimStatusDescription = "TestValue1850447006",
                InputBy = "TestValue79063285",
                InputOn = DateTime.UtcNow,
                Active = false
            };
            var token = jwtToken;

            // Act
            var result = await _testClass.UpdateClaimStatus(claimStatus, token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallUpdateClaimStatusWithNullClaimStatus()
        {
            var result = await _testClass.UpdateClaimStatus(default(ClaimStatus), "TestValue257629591");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallUpdateClaimStatusWithInvalidToken(string value)
        {
            var result = await _testClass.UpdateClaimStatus(new ClaimStatus
            {
                ID = 1151139484,
                ClaimStatusDescription = "TestValue742271667",
                InputBy = "TestValue530563790",
                InputOn = DateTime.UtcNow,
                Active = false
            }, value);

            Assert.Null(result);
        }

        [Fact]
        public void CanSetAndGetClaimStatuses()
        {
            // Arrange
            var testValue = new List<ClaimStatus>();

            // Act
            _testClass.ClaimStatuses = testValue;

            // Assert
            Assert.Same(testValue, _testClass.ClaimStatuses);
        }
    }
}