namespace DFI.FaultReporting.Services.Tests.Admin
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using DFI.FaultReporting.Http.Admin;
    using DFI.FaultReporting.Http.Users;
    using DFI.FaultReporting.JWT.Requests;
    using DFI.FaultReporting.JWT.Response;
    using DFI.FaultReporting.Models.Admin;
    using DFI.FaultReporting.Services.Admin;
    using DFI.FaultReporting.Services.Interfaces.Settings;
    using Moq;
    using Xunit;

    public class FaultStatusServiceTests
    {
        private FaultStatusService _testClass;
        private FaultStatusHttp _faultStatusHttp;
        private UserHttp _userHttp;

        public string jwtToken = "";

        public FaultStatusServiceTests()
        {
            var mockFactory = new Mock<IHttpClientFactory>();

            var client = new HttpClient();

            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IHttpClientFactory factory = mockFactory.Object;

            var mockSettings = new Mock<ISettingsService>();

            ISettingsService settings = mockSettings.Object;

            _faultStatusHttp = new FaultStatusHttp(factory, settings);
            _testClass = new FaultStatusService(_faultStatusHttp);
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
            var instance = new FaultStatusService(_faultStatusHttp);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public async Task CanCallGetFaultStatuses()
        {
            // Act
            var result = await _testClass.GetFaultStatuses();

            Assert.NotNull(result);
        }

        [Fact]
        public async Task CanCallGetFaultStatus()
        {
            // Arrange
            var ID = 6;
            var token = jwtToken;

            // Act
            var result = await _testClass.GetFaultStatus(ID, token);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetFaultStatusWithInvalidToken(string value)
        {
            var result = await _testClass.GetFaultStatus(6, value);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetFaultStatusPerformsMapping()
        {
            // Arrange
            var ID = 6;
            var token = jwtToken;

            // Act
            var result = await _testClass.GetFaultStatus(ID, token);

            // Assert
            Assert.Equal(ID, result.ID);
        }

        [Fact]
        public async Task CanCallCreateFaultStatus()
        {
            // Arrange
            var faultStatus = new FaultStatus
            {
                ID = 0,
                FaultStatusDescription = "TestValue874361144",
                InputBy = "TestValue1748089690",
                InputOn = DateTime.UtcNow,
                Active = true
            };
            var token = jwtToken;

            // Act
            var result = await _testClass.CreateFaultStatus(faultStatus, token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallCreateFaultStatusWithNullFaultStatus()
        {
            var result = await _testClass.CreateFaultStatus(default(FaultStatus), "TestValue1022528481");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallCreateFaultStatusWithInvalidToken(string value)
        {
            var result = await _testClass.CreateFaultStatus(new FaultStatus
            {
                ID = 0,
                FaultStatusDescription = "TestValue1994221696",
                InputBy = "TestValue1533836892",
                InputOn = DateTime.UtcNow,
                Active = true
            }, value);

            Assert.Null(result);
        }

        [Fact]
        public async Task CanCallUpdateFaultStatus()
        {
            var faultStatus = new FaultStatus
            {
                ID = 6,
                FaultStatusDescription = "TestValue365076843",
                InputBy = "TestValue2010127012",
                InputOn = DateTime.UtcNow,
                Active = true
            };

            var token = jwtToken;

            // Act
            var result = await _testClass.UpdateFaultStatus(faultStatus, token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallUpdateFaultStatusWithNullFaultStatus()
        {
            var result = await _testClass.UpdateFaultStatus(default(FaultStatus), "TestValue1840155941");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallUpdateFaultStatusWithInvalidToken(string value)
        {
            var result = await _testClass.UpdateFaultStatus(new FaultStatus
            {
                ID = 6,
                FaultStatusDescription = "TestValue365076843",
                InputBy = "TestValue2010127012",
                InputOn = DateTime.UtcNow,
                Active = true
            }, value);

            Assert.Null(result);
        }

        [Fact]
        public void CanSetAndGetFaultStatuses()
        {
            // Arrange
            var testValue = new List<FaultStatus>();

            // Act
            _testClass.FaultStatuses = testValue;

            // Assert
            Assert.Same(testValue, _testClass.FaultStatuses);
        }
    }
}