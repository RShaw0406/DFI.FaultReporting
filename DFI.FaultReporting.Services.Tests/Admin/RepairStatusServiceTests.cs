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

    public class RepairStatusServiceTests
    {
        private RepairStatusService _testClass;
        private RepairStatusHttp _repairStatusHttp;
        private UserHttp _userHttp;

        public string jwtToken = "";

        public RepairStatusServiceTests()
        {
            var mockFactory = new Mock<IHttpClientFactory>();

            var client = new HttpClient();

            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IHttpClientFactory factory = mockFactory.Object;

            var mockSettings = new Mock<ISettingsService>();

            ISettingsService settings = mockSettings.Object;

            _repairStatusHttp = new RepairStatusHttp(factory, settings);
            _testClass = new RepairStatusService(_repairStatusHttp);
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
            var instance = new RepairStatusService(_repairStatusHttp);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public async Task CanCallGetRepairStatuses()
        {
            // Arrange
            var token = jwtToken;

            // Act
            var result = await _testClass.GetRepairStatuses(token);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetRepairStatusesWithInvalidToken(string value)
        {
            var result = await _testClass.GetRepairStatuses(value);

            Assert.Null(result);
        }

        [Fact]
        public async Task CanCallGetRepairStatus()
        {
            // Arrange
            var ID = 6;
            var token = jwtToken;

            // Act
            var result = await _testClass.GetRepairStatus(ID, token);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetRepairStatusWithInvalidToken(string value)
        {
            var result = await _testClass.GetRepairStatus(6, value);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetRepairStatusPerformsMapping()
        {
            // Arrange
            var ID = 6;
            var token = jwtToken;

            // Act
            var result = await _testClass.GetRepairStatus(ID, token);

            // Assert
            Assert.Equal(ID, result.ID);
        }

        [Fact]
        public async Task CanCallCreateRepairStatus()
        {
            // Arrange
            var repairStatus = new RepairStatus
            {
                ID = 0,
                RepairStatusDescription = "TestValue1030956805",
                InputBy = "TestValue411256021",
                InputOn = DateTime.UtcNow,
                Active = true
            };
            var token = jwtToken;

            // Act
            var result = await _testClass.CreateRepairStatus(repairStatus, token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallCreateRepairStatusWithNullRepairStatus()
        {
            var result = await _testClass.CreateRepairStatus(default(RepairStatus), "TestValue702461828");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallCreateRepairStatusWithInvalidToken(string value)
        {
            var result = await _testClass.CreateRepairStatus(new RepairStatus
            {
                ID = 0,
                RepairStatusDescription = "TestValue923098768",
                InputBy = "TestValue1897931544",
                InputOn = DateTime.UtcNow,
                Active = false
            }, value);

            Assert.Null(result);
        }

        [Fact]
        public async Task CanCallUpdateRepairStatus()
        {
            // Arrange
            var repairStatus = new RepairStatus
            {
                ID = 6,
                RepairStatusDescription = "TestValue141643202",
                InputBy = "TestValue1702873637",
                InputOn = DateTime.UtcNow,
                Active = false
            };
            var token = jwtToken;

            // Act
            var result = await _testClass.UpdateRepairStatus(repairStatus, token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallUpdateRepairStatusWithNullRepairStatus()
        {
            var result = await _testClass.UpdateRepairStatus(default(RepairStatus), "TestValue1353722167");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallUpdateRepairStatusWithInvalidToken(string value)
        {
            var result = await _testClass.UpdateRepairStatus(new RepairStatus
            {
                ID = 6,
                RepairStatusDescription = "TestValue1812351027",
                InputBy = "TestValue1791218889",
                InputOn = DateTime.UtcNow,
                Active = false
            }, value);

            Assert.Null(result);
        }

        [Fact]
        public void CanSetAndGetRepairStatuses()
        {
            // Arrange
            var testValue = new List<RepairStatus>();

            // Act
            _testClass.RepairStatuses = testValue;

            // Assert
            Assert.Same(testValue, _testClass.RepairStatuses);
        }
    }
}