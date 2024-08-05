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

    public class FaultPriorityServiceTests
    {
        private FaultPriorityService _testClass;
        private FaultPriorityHttp _faultPriorityHttp;
        private UserHttp _userHttp;

        public string jwtToken = "";

        public FaultPriorityServiceTests()
        {
            var mockFactory = new Mock<IHttpClientFactory>();

            var client = new HttpClient();

            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IHttpClientFactory factory = mockFactory.Object;

            var mockSettings = new Mock<ISettingsService>();

            ISettingsService settings = mockSettings.Object;

            _faultPriorityHttp = new FaultPriorityHttp(factory, settings);
            _testClass = new FaultPriorityService(_faultPriorityHttp);
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
            var instance = new FaultPriorityService(_faultPriorityHttp);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public async Task CanCallGetFaultPriorities()
        {
            // Arrange
            var token = jwtToken;

            // Act
            var result = await _testClass.GetFaultPriorities();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CanCallGetFaultPriority()
        {
            // Arrange
            var ID = 14;
            var token = jwtToken;

            // Act
            var result = await _testClass.GetFaultPriority(ID, token);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetFaultPriorityWithInvalidToken(string value)
        {
            var result = await _testClass.GetFaultPriority(14, value);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetFaultPriorityPerformsMapping()
        {
            // Arrange
            var ID = 14;
            var token = jwtToken;

            // Act
            var result = await _testClass.GetFaultPriority(ID, token);

            // Assert
            Assert.Equal(ID, result.ID);
        }

        [Fact]
        public async Task CanCallCreateFaultPriority()
        {
            // Arrange
            var faultPriority = new FaultPriority
            {
                ID = 0,
                InputBy = "TestValue1106791762",
                InputOn = DateTime.UtcNow,
                Active = true,
                FaultPriorityDescription = "TestValue1943402444",
                FaultPriorityRating = "R20"
            };
            var token = jwtToken;

            // Act
            var result = await _testClass.CreateFaultPriority(faultPriority, token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallCreateFaultPriorityWithNullFaultPriority()
        {
            var result = await _testClass.CreateFaultPriority(default(FaultPriority), "TestValue346025230");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallCreateFaultPriorityWithInvalidToken(string value)
        {
            var result = await _testClass.CreateFaultPriority(new FaultPriority
            {
                ID = 0,
                InputBy = "TestValue1126855641",
                InputOn = DateTime.UtcNow,
                Active = true,
                FaultPriorityDescription = "TestValue502034562",
                FaultPriorityRating = "TestValue674872759"
            }, value);

            Assert.Null(result);
        }

        [Fact]
        public async Task CanCallUpdateFaultPriority()
        {
            // Arrange
            var faultPriority = new FaultPriority
            {
                ID = 14,
                InputBy = "TestValue400951115",
                InputOn = DateTime.UtcNow,
                Active = false,
                FaultPriorityDescription = "TestValue1068385572",
                FaultPriorityRating = "R20"
            };
            var token = jwtToken;

            // Act
            var result = await _testClass.UpdateFaultPriority(faultPriority, token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallUpdateFaultPriorityWithNullFaultPriority()
        {
            var result = await _testClass.UpdateFaultPriority(default(FaultPriority), "TestValue461605646");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallUpdateFaultPriorityWithInvalidToken(string value)
        {
            var result = await _testClass.UpdateFaultPriority(new FaultPriority
            {
                ID = 16249003,
                InputBy = "TestValue2096606710",
                InputOn = DateTime.UtcNow,
                Active = false,
                FaultPriorityDescription = "TestValue1414119468",
                FaultPriorityRating = "TestValue776849720"
            }, value);

            Assert.Null(result);
        }

        [Fact]
        public void CanSetAndGetFaultPriorities()
        {
            // Arrange
            var testValue = new List<FaultPriority>();

            // Act
            _testClass.FaultPriorities = testValue;

            // Assert
            Assert.Same(testValue, _testClass.FaultPriorities);
        }
    }
}