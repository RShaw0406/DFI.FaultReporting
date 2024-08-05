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

    public class FaultServiceTests
    {
        private FaultService _testClass;
        private FaultHttp _faultHttp;
        private UserHttp _userHttp;

        public string jwtToken = "";

        public FaultServiceTests()
        {
            var mockFactory = new Mock<IHttpClientFactory>();

            var client = new HttpClient();

            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IHttpClientFactory factory = mockFactory.Object;

            var mockSettings = new Mock<ISettingsService>();

            ISettingsService settings = mockSettings.Object;

            _faultHttp = new FaultHttp(factory, settings);
            _testClass = new FaultService(_faultHttp);
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
            var instance = new FaultService(_faultHttp);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public async Task CanCallGetFaults()
        {
            // Act
            var result = await _testClass.GetFaults();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CanCallGetFault()
        {
            // Arrange
            var ID = 24;
            var token = jwtToken;

            // Act
            var result = await _testClass.GetFault(ID, token);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetFaultWithInvalidToken(string value)
        {
            var result = await _testClass.GetFault(24, value);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetFaultPerformsMapping()
        {
            // Arrange
            var ID = 24;
            var token = jwtToken;

            // Act
            var result = await _testClass.GetFault(ID, token);

            // Assert
            Assert.Equal(ID, result.ID);
        }

        [Fact]
        public async Task CanCallCreateFault()
        {
            // Arrange
            var fault = new Fault
            {
                ID = 0,
                Latitude = "54.56128824546601",
                Longitude = "-5.909585100365",
                RoadName = "Test",
                FaultPriorityID = 4,
                FaultTypeID = 1,
                FaultStatusID = 1,
                StaffID = 16,
                InputBy = "TestValue899202281",
                InputOn = DateTime.UtcNow,
                Active = false,
                RoadNumber = "A123",
                RoadTown = "Test",
                RoadCounty = "Test"
            };
            var token = jwtToken;

            // Act
            var result = await _testClass.CreateFault(fault, token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallCreateFaultWithNullFault()
        {
            var result = await _testClass.CreateFault(default(Fault), "TestValue487225068");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallCreateFaultWithInvalidToken(string value)
        {
            var result = await _testClass.CreateFault(new Fault
            {
                ID = 0,
                Latitude = "TestValue1783426221",
                Longitude = "TestValue135122356",
                RoadName = "TestValue660457582",
                FaultPriorityID = 1511543967,
                FaultTypeID = 370580524,
                FaultStatusID = 1496818405,
                StaffID = 480229583,
                InputBy = "TestValue807504560",
                InputOn = DateTime.UtcNow,
                Active = false,
                RoadNumber = "TestValue1958500061",
                RoadTown = "TestValue1727286134",
                RoadCounty = "TestValue1762556050"
            }, value);

            Assert.Null(result);
        }

        [Fact]
        public async Task CanCallUpdateFault()
        {
            // Arrange
            var fault = new Fault
            {
                ID = 24,
                Latitude = "54.56128824546601",
                Longitude = "-5.909585100365",
                RoadName = "Test",
                FaultPriorityID = 4,
                FaultTypeID = 1,
                FaultStatusID = 1,
                StaffID = 16,
                InputBy = "TestValue899202281",
                InputOn = DateTime.UtcNow,
                Active = false,
                RoadNumber = "A123",
                RoadTown = "Test",
                RoadCounty = "Test"
            };
            var token = jwtToken;

            // Act
            var result = await _testClass.UpdateFault(fault, token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallUpdateFaultWithNullFault()
        {
            var result = await _testClass.UpdateFault(default(Fault), "TestValue1721792456");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallUpdateFaultWithInvalidToken(string value)
        {
            var result = await _testClass.UpdateFault(new Fault
            {
                ID = 1065814860,
                Latitude = "TestValue1056038456",
                Longitude = "TestValue1842815974",
                RoadName = "TestValue244200397",
                FaultPriorityID = 1400015277,
                FaultTypeID = 321504824,
                FaultStatusID = 1948499187,
                StaffID = 245995851,
                InputBy = "TestValue2000434051",
                InputOn = DateTime.UtcNow,
                Active = false,
                RoadNumber = "TestValue1328407102",
                RoadTown = "TestValue1783724362",
                RoadCounty = "TestValue932016717"
            }, value);

            Assert.Null(result);
        }

        [Fact]
        public void CanSetAndGetFaults()
        {
            // Arrange
            var testValue = new List<Fault>();

            // Act
            _testClass.Faults = testValue;

            // Assert
            Assert.Same(testValue, _testClass.Faults);
        }
    }
}