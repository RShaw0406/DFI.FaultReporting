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

    public class FaultTypeServiceTests
    {
        private FaultTypeService _testClass;
        private FaultTypeHttp _faultTypeHttp;
        private UserHttp _userHttp;

        public string jwtToken = "";

        public FaultTypeServiceTests()
        {
            var mockFactory = new Mock<IHttpClientFactory>();

            var client = new HttpClient();

            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IHttpClientFactory factory = mockFactory.Object;

            var mockSettings = new Mock<ISettingsService>();

            ISettingsService settings = mockSettings.Object;

            _faultTypeHttp = new FaultTypeHttp(factory, settings);
            _testClass = new FaultTypeService(_faultTypeHttp);
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
            var instance = new FaultTypeService(_faultTypeHttp);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public async Task CanCallGetFaultTypes()
        {
            // Act
            var result = await _testClass.GetFaultTypes();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CanCallGetFaultType()
        {
            // Arrange
            var ID = 10;
            var token = jwtToken;

            // Act
            var result = await _testClass.GetFaultType(ID, token);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetFaultTypeWithInvalidToken(string value)
        {
            var result = await _testClass.GetFaultType(10, value);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetFaultTypePerformsMapping()
        {
            // Arrange
            var ID = 10;
            var token = jwtToken;

            // Act
            var result = await _testClass.GetFaultType(ID, token);

            // Assert
            Assert.Equal(ID, result.ID);
        }

        [Fact]
        public async Task CanCallCreateFaultType()
        {
            // Arrange
            var faultType = new FaultType
            {
                ID = 0,
                FaultTypeDescription = "TestValue1645108735",
                InputBy = "TestValue797701895",
                InputOn = DateTime.UtcNow,
                Active = false
            };
            var token = jwtToken;

            // Act
            var result = await _testClass.CreateFaultType(faultType, token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallCreateFaultTypeWithNullFaultType()
        {
            var result = await _testClass.CreateFaultType(default(FaultType), "TestValue592851439");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallCreateFaultTypeWithInvalidToken(string value)
        {
            var result = await _testClass.CreateFaultType(new FaultType
            {
                ID = 0,
                FaultTypeDescription = "TestValue1645108735",
                InputBy = "TestValue797701895",
                InputOn = DateTime.UtcNow,
                Active = false
            }, value);

            Assert.Null(result);
        }

        [Fact]
        public async Task CanCallUpdateFaultType()
        {
            // Arrange
            var faultType = new FaultType
            {
                ID = 10,
                FaultTypeDescription = "TestValue122043000",
                InputBy = "TestValue2001612708",
                InputOn = DateTime.UtcNow,
                Active = false
            };
            var token = jwtToken;

            // Act
            var result = await _testClass.UpdateFaultType(faultType, token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallUpdateFaultTypeWithNullFaultType()
        {
            var result = await _testClass.UpdateFaultType(default(FaultType), "TestValue41244442");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallUpdateFaultTypeWithInvalidToken(string value)
        {
            var result = await _testClass.UpdateFaultType(new FaultType
            {
                ID = 10,
                FaultTypeDescription = "TestValue122043000",
                InputBy = "TestValue2001612708",
                InputOn = DateTime.UtcNow,
                Active = false
            }, value);

            Assert.Null(result);
        }

        [Fact]
        public void CanSetAndGetFaultTypes()
        {
            // Arrange
            var testValue = new List<FaultType>();

            // Act
            _testClass.FaultTypes = testValue;

            // Assert
            Assert.Same(testValue, _testClass.FaultTypes);
        }
    }
}