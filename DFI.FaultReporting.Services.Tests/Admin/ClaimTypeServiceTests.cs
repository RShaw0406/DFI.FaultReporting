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

    public class ClaimTypeServiceTests
    {
        private ClaimTypeService _testClass;
        private ClaimTypeHttp _claimTypeHttp;
        private UserHttp _userHttp;

        public string jwtToken = "";

        public ClaimTypeServiceTests()
        {
            var mockFactory = new Mock<IHttpClientFactory>();

            var client = new HttpClient();

            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IHttpClientFactory factory = mockFactory.Object;

            var mockSettings = new Mock<ISettingsService>();

            ISettingsService settings = mockSettings.Object;

            _claimTypeHttp = new ClaimTypeHttp(factory, settings);
            _testClass = new ClaimTypeService(_claimTypeHttp);
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
            var instance = new ClaimTypeService(_claimTypeHttp);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public async Task CanCallGetClaimTypes()
        {
            // Arrange
            var token = jwtToken;

            // Act
            var result = await _testClass.GetClaimTypes(token);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetClaimTypesWithInvalidToken(string value)
        {
            var result = await _testClass.GetClaimTypes(value);

            Assert.Null(result);
        }

        [Fact]
        public async Task CanCallGetClaimType()
        {
            // Arrange
            var ID = 11;
            var token = jwtToken;


            // Act
            var result = await _testClass.GetClaimType(ID, token);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetClaimTypeWithInvalidToken(string value)
        {
            var result = await _testClass.GetClaimType(11, value);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetClaimTypePerformsMapping()
        {
            // Arrange
            var ID = 11;
            var token = jwtToken;

            // Act
            var result = await _testClass.GetClaimType(ID, token);

            // Assert
            Assert.Equal(ID, result.ID);
        }

        [Fact]
        public async Task CanCallCreateClaimType()
        {
            // Arrange
            var claimType = new ClaimType
            {
                ID = 0,
                ClaimTypeDescription = "TestValue1088108131",
                InputBy = "TestValue1572855264",
                InputOn = DateTime.UtcNow,
                Active = false
            };
            var token = jwtToken;

            // Act
            var result = await _testClass.CreateClaimType(claimType, token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallCreateClaimTypeWithNullClaimType()
        {
            var result = await _testClass.CreateClaimType(default(ClaimType), "TestValue2087941299");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallCreateClaimTypeWithInvalidToken(string value)
        {
            var result = await _testClass.CreateClaimType(new ClaimType
            {
                ID = 0,
                ClaimTypeDescription = "TestValue941574019",
                InputBy = "TestValue576365511",
                InputOn = DateTime.UtcNow,
                Active = true
            }, value);

            Assert.Null(result);
        }

        [Fact]
        public async Task CanCallUpdateClaimType()
        {
            // Arrange
            var claimType = new ClaimType
            {
                ID = 11,
                ClaimTypeDescription = "TestValue524163847",
                InputBy = "TestValue562650063",
                InputOn = DateTime.UtcNow,
                Active = true
            };
            var token = jwtToken;

            // Act
            var result = await _testClass.UpdateClaimType(claimType, token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallUpdateClaimTypeWithNullClaimType()
        {
            var result = await _testClass.UpdateClaimType(default(ClaimType), "TestValue1545960780");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallUpdateClaimTypeWithInvalidToken(string value)
        {
            var result = await _testClass.UpdateClaimType(new ClaimType
            {
                ID = 0,
                ClaimTypeDescription = "TestValue941574019",
                InputBy = "TestValue576365511",
                InputOn = DateTime.UtcNow,
                Active = true
            }, value);

            Assert.Null(result);
        }

        [Fact]
        public void CanSetAndGetClaimTypes()
        {
            // Arrange
            var testValue = new List<ClaimType>();

            // Act
            _testClass.ClaimTypes = testValue;

            // Assert
            Assert.Same(testValue, _testClass.ClaimTypes);
        }
    }
}