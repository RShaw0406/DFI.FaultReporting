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

    public class WitnessServiceTests
    {
        private WitnessService _testClass;
        private WitnessHttp _witnessHttp;
        private UserHttp _userHttp;

        public string jwtToken = "";

        public WitnessServiceTests()
        {
            var mockFactory = new Mock<IHttpClientFactory>();

            var client = new HttpClient();

            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IHttpClientFactory factory = mockFactory.Object;

            var mockSettings = new Mock<ISettingsService>();

            ISettingsService settings = mockSettings.Object;

            _witnessHttp = new WitnessHttp(factory, settings);
            _testClass = new WitnessService(_witnessHttp);
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
            var instance = new WitnessService(_witnessHttp);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public async Task CanCallGetWitnesses()
        {
            // Arrange
            var token = jwtToken;

            // Act
            var result = await _testClass.GetWitnesses(token);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetWitnessesWithInvalidToken(string value)
        {
            var result = await _testClass.GetWitnesses(value);

            Assert.Null(result);
        }

        [Fact]
        public async Task CanCallGetWitness()
        {
            // Arrange
            var ID = 3;
            var token = jwtToken;

            // Act
            var result = await _testClass.GetWitness(ID, token);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetWitnessWithInvalidToken(string value)
        {
            var result = await _testClass.GetWitness(3, value);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetWitnessPerformsMapping()
        {
            // Arrange
            var ID = 3;
            var token = jwtToken;

            // Act
            var result = await _testClass.GetWitness(ID, token);

            // Assert
            Assert.Equal(ID, result.ID);
        }

        [Fact]
        public async Task CanCallCreateWitness()
        {
            // Arrange
            var witness = new Witness
            {
                ID = 0,
                ClaimID = 9,
                Title = "Mr",
                FirstName = "Test",
                LastName = "Test",
                Email = "test@test.com",
                ContactNumber = "07798776554",
                Postcode = "BT1 8PA",
                AddressLine1 = "Test",
                AddressLine2 = "Test",
                AddressLine3 = "Test",
                InputBy = "TestValue2040621005",
                InputOn = DateTime.UtcNow,
                Active = false
            };
            var token = jwtToken;

            // Act
            var result = await _testClass.CreateWitness(witness, token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallCreateWitnessWithNullWitness()
        {
            var result = await _testClass.CreateWitness(default(Witness), "TestValue1732884392");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallCreateWitnessWithInvalidToken(string value)
        {
            var result = await _testClass.CreateWitness(new Witness
            {
                ID = 0,
                ClaimID = 9,
                Title = "Mr",
                FirstName = "Test",
                LastName = "Test",
                Email = "test@test.com",
                ContactNumber = "07798776554",
                Postcode = "BT1 8PA",
                AddressLine1 = "Test",
                AddressLine2 = "Test",
                AddressLine3 = "Test",
                InputBy = "TestValue2040621005",
                InputOn = DateTime.UtcNow,
                Active = false
            }, value);

            Assert.Null(result);
        }

        [Fact]
        public async Task CanCallUpdateWitness()
        {
            // Arrange
            var witness = new Witness
            {
                ID = 3,
                ClaimID = 9,
                Title = "Mr",
                FirstName = "Test",
                LastName = "Test",
                Email = "test@test.com",
                ContactNumber = "07798776554",
                Postcode = "BT1 8PA",
                AddressLine1 = "Test",
                AddressLine2 = "Test",
                AddressLine3 = "Test",
                InputBy = "TestValue2040621005",
                InputOn = DateTime.UtcNow,
                Active = false
            };
            var token = jwtToken;

            // Act
            var result = await _testClass.UpdateWitness(witness, token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallUpdateWitnessWithNullWitness()
        {
            var result = await _testClass.UpdateWitness(default(Witness), "TestValue720901640");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallUpdateWitnessWithInvalidToken(string value)
        {
            var result = await _testClass.UpdateWitness(new Witness
            {
                ID = 3,
                Title = "Mr",
                FirstName = "Test",
                LastName = "Test",
                Email = "test@test.com",
                ContactNumber = "07798776554",
                Postcode = "BT1 8PA",
                AddressLine1 = "Test",
                AddressLine2 = "Test",
                AddressLine3 = "Test",
                InputBy = "TestValue2040621005",
                InputOn = DateTime.UtcNow,
                Active = false
            }, value);

            Assert.Null(result);
        }

        [Fact]
        public void CanSetAndGetWitnesses()
        {
            // Arrange
            var testValue = new List<Witness>();

            // Act
            _testClass.Witnesses = testValue;

            // Assert
            Assert.Same(testValue, _testClass.Witnesses);
        }
    }
}