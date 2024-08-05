namespace DFI.FaultReporting.Services.Tests.FaultReports
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using DFI.FaultReporting.Http.FaultReports;
    using DFI.FaultReporting.Http.Users;
    using DFI.FaultReporting.JWT.Requests;
    using DFI.FaultReporting.JWT.Response;
    using DFI.FaultReporting.Models.FaultReports;
    using DFI.FaultReporting.Services.FaultReports;
    using DFI.FaultReporting.Services.Interfaces.Settings;
    using Moq;
    using Xunit;

    public class RepairServiceTests
    {
        private RepairService _testClass;
        private RepairHttp _repairHttp;
        private UserHttp _userHttp;

        public string jwtToken = "";

        public RepairServiceTests()
        {
            var mockFactory = new Mock<IHttpClientFactory>();

            var client = new HttpClient();

            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IHttpClientFactory factory = mockFactory.Object;

            var mockSettings = new Mock<ISettingsService>();

            ISettingsService settings = mockSettings.Object;

            _repairHttp = new RepairHttp(factory, settings);
            _testClass = new RepairService(_repairHttp);
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
            var instance = new RepairService(_repairHttp);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public async Task CanCallGetRepairs()
        {
            // Arrange
            var token = jwtToken;

            // Act
            var result = await _testClass.GetRepairs(token);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetRepairsWithInvalidToken(string value)
        {
            var result = await _testClass.GetRepairs(value);

            Assert.Null(result);
        }

        [Fact]
        public async Task CanCallGetRepair()
        {
            // Arrange
            var ID = 17;
            var token = jwtToken;

            // Act
            var result = await _testClass.GetRepair(ID, token);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetRepairWithInvalidToken(string value)
        {
            var result = await _testClass.GetRepair(17, value);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetRepairPerformsMapping()
        {
            // Arrange
            var ID = 17;
            var token = jwtToken;

            // Act
            var result = await _testClass.GetRepair(ID, token);

            // Assert
            Assert.Equal(ID, result.ID);
        }

        [Fact]
        public async Task CanCallCreateRepair()
        {
            // Arrange
            var repair = new Repair
            {
                ID = 0,
                FaultID = 18,
                RepairTargetDate = DateTime.UtcNow,
                ActualRepairDate = DateTime.UtcNow,
                RepairNotes = "TestValue1099292362",
                RepairStatusID = 1537430370,
                ContractorID = 282171952,
                InputBy = "TestValue849591375",
                InputOn = DateTime.UtcNow,
                Active = true
            };
            var token = jwtToken;

            // Act
            var result = await _testClass.CreateRepair(repair, token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallCreateRepairWithNullRepair()
        {
            var result = await _testClass.CreateRepair(default(Repair), "TestValue564801688");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallCreateRepairWithInvalidToken(string value)
        {
            var result = await _testClass.CreateRepair(new Repair
            {
                ID = 0,
                FaultID = 18,
                RepairTargetDate = DateTime.UtcNow,
                ActualRepairDate = DateTime.UtcNow,
                RepairNotes = "TestValue1099292362",
                RepairStatusID = 1537430370,
                ContractorID = 282171952,
                InputBy = "TestValue849591375",
                InputOn = DateTime.UtcNow,
                Active = true
            }, value);

            Assert.Null(result);
        }

        [Fact]
        public async Task CanCallUpdateRepair()
        {
            // Arrange
            var repair = new Repair
            {
                ID = 17,
                FaultID = 18,
                RepairTargetDate = DateTime.UtcNow,
                ActualRepairDate = DateTime.UtcNow,
                RepairNotes = "TestValue916230376",
                RepairStatusID = 248335881,
                ContractorID = 1284709750,
                InputBy = "TestValue331260573",
                InputOn = DateTime.UtcNow,
                Active = true
            };
            var token = jwtToken;

            // Act
            var result = await _testClass.UpdateRepair(repair, token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallUpdateRepairWithNullRepair()
        {
            var result = await _testClass.UpdateRepair(default(Repair), "TestValue917711537");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallUpdateRepairWithInvalidToken(string value)
        {
            var result = await _testClass.UpdateRepair(new Repair
            {
                ID = 17,
                FaultID = 18,
                RepairTargetDate = DateTime.UtcNow,
                ActualRepairDate = DateTime.UtcNow,
                RepairNotes = "TestValue916230376",
                RepairStatusID = 248335881,
                ContractorID = 1284709750,
                InputBy = "TestValue331260573",
                InputOn = DateTime.UtcNow,
                Active = true
            }, value);

            Assert.Null(result);
        }

        [Fact]
        public void CanSetAndGetRepairs()
        {
            // Arrange
            var testValue = new List<Repair>();

            // Act
            _testClass.Repairs = testValue;

            // Assert
            Assert.Same(testValue, _testClass.Repairs);
        }
    }
}