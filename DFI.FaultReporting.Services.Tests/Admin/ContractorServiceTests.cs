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

    public class ContractorServiceTests
    {
        private ContractorService _testClass;
        private ContractorHttp _contractorHttp;
        private UserHttp _userHttp;

        public string jwtToken = "";

        public ContractorServiceTests()
        {
            var mockFactory = new Mock<IHttpClientFactory>();

            var client = new HttpClient();

            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IHttpClientFactory factory = mockFactory.Object;

            var mockSettings = new Mock<ISettingsService>();

            ISettingsService settings = mockSettings.Object;

            _contractorHttp = new ContractorHttp(factory, settings);
            _testClass = new ContractorService(_contractorHttp);
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
            var instance = new ContractorService(_contractorHttp);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public async Task CanCallGetContractors()
        {
            // Arrange
            var token = jwtToken;

            // Act
            var result = await _testClass.GetContractors(token);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetContractorsWithInvalidToken(string value)
        {
            var result = await _testClass.GetContractors(value);

            Assert.Null(result);
        }

        [Fact]
        public async Task CanCallGetContractor()
        {
            // Arrange
            var ID = 6;
            var token = jwtToken;

            // Act
            var result = await _testClass.GetContractor(ID, token);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetContractorWithInvalidToken(string value)
        {
            var result = await _testClass.GetContractor(160867506, value);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetContractorPerformsMapping()
        {
            // Arrange
            var ID = 6;
            var token = jwtToken;

            // Act
            var result = await _testClass.GetContractor(ID, token);

            // Assert
            Assert.Equal(ID, result.ID);
        }

        [Fact]
        public async Task CanCallCheckForContractor()
        {
            // Arrange
            var email = "contractor@text4.com";

            // Act
            var result = await _testClass.CheckForContractor(email);

            // Assert
            Assert.Equal(true, result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallCheckForContractorWithInvalidEmail(string value)
        {
            var result = await _testClass.CheckForContractor(value);

            Assert.Equal(false, result);
        }

        [Fact]
        public async Task CanCallCreateContractor()
        {
            // Arrange
            var contractor = new Contractor
            {
                ID = 0,
                Email = "test@test.com",
                ContractorName = "TestValue977622301",
                InputBy = "TestValue1820794486",
                InputOn = DateTime.UtcNow,
                Active = false
            };
            var token = jwtToken;

            // Act
            var result = await _testClass.CreateContractor(contractor, token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallCreateContractorWithNullContractor()
        {
            var result = await _testClass.CreateContractor(default(Contractor), "TestValue939997497");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallCreateContractorWithInvalidToken(string value)
        {
            var result = await _testClass.CreateContractor(new Contractor
            {
                ID = 9,
                Email = "test@test.com",
                ContractorName = "TestValue623579376",
                InputBy = "TestValue958901245",
                InputOn = DateTime.UtcNow,
                Active = false
            }, value);

            Assert.Null(result);
        }

        [Fact]
        public async Task CanCallUpdateContractor()
        {
            // Arrange
            var contractor = new Contractor
            {
                ID = 9,
                Email = "test@test.com",
                ContractorName = "TestValue1030132851",
                InputBy = "TestValue225618305",
                InputOn = DateTime.UtcNow,
                Active = false
            };
            var token = jwtToken;

            // Act
            var result = await _testClass.UpdateContractor(contractor, token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallUpdateContractorWithNullContractor()
        {
            var result = await _testClass.UpdateContractor(default(Contractor), "TestValue15404118");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallUpdateContractorWithInvalidToken(string value)
        {
            var result = await _testClass.UpdateContractor(new Contractor
            {
                ID = 0,
                Email = "test@test.com",
                ContractorName = "TestValue576365511",
                InputBy = "TestValue1545960780",
                InputOn = DateTime.UtcNow,
                Active = false
            }, value);

            Assert.Null(result);
        }

        [Fact]
        public void CanSetAndGetContractors()
        {
            // Arrange
            var testValue = new List<Contractor>();

            // Act
            _testClass.Contractors = testValue;

            // Assert
            Assert.Same(testValue, _testClass.Contractors);
        }
    }
}