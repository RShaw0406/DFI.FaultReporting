namespace DFI.FaultReporting.Services.Tests.Claims
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using DFI.FaultReporting.Http.Claims;
    using DFI.FaultReporting.Http.Users;
    using DFI.FaultReporting.JWT.Requests;
    using DFI.FaultReporting.JWT.Response;
    using DFI.FaultReporting.Models.Claims;
    using DFI.FaultReporting.Services.Claims;
    using DFI.FaultReporting.Services.Interfaces.Settings;
    using Moq;
    using Xunit;

    public class LegalRepServiceTests
    {
        private LegalRepService _testClass;
        private LegalRepHttp _legalRepHttp;
        private UserHttp _userHttp;

        public string jwtToken = "";

        public LegalRepServiceTests()
        {
            var mockFactory = new Mock<IHttpClientFactory>();

            var client = new HttpClient();

            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IHttpClientFactory factory = mockFactory.Object;

            var mockSettings = new Mock<ISettingsService>();

            ISettingsService settings = mockSettings.Object;

            _legalRepHttp = new LegalRepHttp(factory, settings);
            _testClass = new LegalRepService(_legalRepHttp);
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
            var instance = new LegalRepService(_legalRepHttp);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public async Task CanCallGetLegalReps()
        {
            // Arrange
            var token = jwtToken;

            // Act
            var result = await _testClass.GetLegalReps(token);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetLegalRepsWithInvalidToken(string value)
        {
            var result = await _testClass.GetLegalReps(value);

            Assert.Null(result);
        }

        [Fact]
        public async Task CanCallGetLegalRep()
        {
            // Arrange
            var ID = 1;
            var token = jwtToken;

            // Act
            var result = await _testClass.GetLegalRep(ID, token);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetLegalRepWithInvalidToken(string value)
        {
            var result = await _testClass.GetLegalRep(1, value);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetLegalRepPerformsMapping()
        {
            // Arrange
            var ID = 1;
            var token = jwtToken;

            // Act
            var result = await _testClass.GetLegalRep(ID, token);

            // Assert
            Assert.Equal(ID, result.ID);
        }

        [Fact]
        public async Task CanCallCreateLegalRep()
        {
            // Arrange
            var legalRep = new LegalRep
            {
                ID = 0,
                ClaimID = 9,
                Title = "Mr",
                FirstName = "Test",
                LastName = "Test",
                CompanyName = "Test",
                Postcode = "BT1 8PA",
                AddressLine1 = "Test",
                AddressLine2 = "Test",
                AddressLine3 = "Test",
                InputBy = "TestValue1793671927",
                InputOn = DateTime.UtcNow,
                Active = false
            };
            var token = jwtToken;

            // Act
            var result = await _testClass.CreateLegalRep(legalRep, token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallCreateLegalRepWithNullLegalRep()
        {
            var result = await _testClass.CreateLegalRep(default(LegalRep), "TestValue1170591006");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallCreateLegalRepWithInvalidToken(string value)
        {
            var result = await _testClass.CreateLegalRep(new LegalRep
            {
                ID = 0,
                ClaimID = 9,
                Title = "Mr",
                FirstName = "TestValue907422522",
                LastName = "TestValue1934135193",
                CompanyName = "TestValue2017774591",
                Postcode = "BT1 8PA",
                AddressLine1 = "TestValue1053341338",
                AddressLine2 = "TestValue1653195573",
                AddressLine3 = "TestValue1670494862",
                InputBy = "TestValue2058568119",
                InputOn = DateTime.UtcNow,
                Active = false
            }, value);

            Assert.Null(result);
        }

        [Fact]
        public async Task CanCallUpdateLegalRep()
        {
            // Arrange
            var legalRep = new LegalRep
            {
                ID = 1,
                ClaimID = 9,
                Title = "Mr",
                FirstName = "Test",
                LastName = "Test",
                CompanyName = "Test",
                Postcode = "BT1 8PA",
                AddressLine1 = "Test",
                AddressLine2 = "Test",
                AddressLine3 = "Test",
                InputBy = "TestValue1987246835",
                InputOn = DateTime.UtcNow,
                Active = false
            };
            var token = jwtToken;

            // Act
            var result = await _testClass.UpdateLegalRep(legalRep, token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallUpdateLegalRepWithNullLegalRep()
        {
            var result = await _testClass.UpdateLegalRep(default(LegalRep), "TestValue708634592");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallUpdateLegalRepWithInvalidToken(string value)
        {
            var result = await _testClass.UpdateLegalRep(new LegalRep
            {
                ID = 1,
                ClaimID = 9,
                Title = "Mr",
                FirstName = "Test",
                LastName = "Test",
                CompanyName = "Test",
                Postcode = "BT1 8PA",
                AddressLine1 = "Test",
                AddressLine2 = "Test",
                AddressLine3 = "Test",
                InputBy = "TestValue1987246835",
                InputOn = DateTime.UtcNow,
                Active = true
            }, value);
        }

        [Fact]
        public void CanSetAndGetLegalReps()
        {
            // Arrange
            var testValue = new List<LegalRep>();

            // Act
            _testClass.LegalReps = testValue;

            // Assert
            Assert.Same(testValue, _testClass.LegalReps);
        }
    }
}