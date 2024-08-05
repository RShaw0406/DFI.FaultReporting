namespace DFI.FaultReporting.Services.Tests.Roles
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using DFI.FaultReporting.Http.Roles;
    using DFI.FaultReporting.Http.Users;
    using DFI.FaultReporting.JWT.Requests;
    using DFI.FaultReporting.JWT.Response;
    using DFI.FaultReporting.Models.Roles;
    using DFI.FaultReporting.Services.Interfaces.Settings;
    using DFI.FaultReporting.Services.Roles;
    using Moq;
    using Xunit;

    public class StaffRoleServiceTests
    {
        private StaffRoleService _testClass;
        private StaffRoleHttp _staffRoleHttp;
        private UserHttp _userHttp;

        public string jwtToken = "";

        public StaffRoleServiceTests()
        {
            var mockFactory = new Mock<IHttpClientFactory>();

            var client = new HttpClient();

            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IHttpClientFactory factory = mockFactory.Object;

            var mockSettings = new Mock<ISettingsService>();

            ISettingsService settings = mockSettings.Object;

            _staffRoleHttp = new StaffRoleHttp(factory, settings);
            _testClass = new StaffRoleService(_staffRoleHttp);
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
            var instance = new StaffRoleService(_staffRoleHttp);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public async Task CanCallGetStaffRoles()
        {
            // Arrange
            var token = jwtToken;

            // Act
            var result = await _testClass.GetStaffRoles(token);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetStaffRolesWithInvalidToken(string value)
        {
            var result = await _testClass.GetStaffRoles(value);

            Assert.Null(result);
        }

        [Fact]
        public async Task CanCallGetStaffRole()
        {
            // Arrange
            var ID = 23;
            var token = jwtToken;

            // Act
            var result = await _testClass.GetStaffRole(ID, token);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetStaffRoleWithInvalidToken(string value)
        {
            var result = await _testClass.GetStaffRole(12, value);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetStaffRolePerformsMapping()
        {
            // Arrange
            var ID = 23;
            var token = jwtToken;

            // Act
            var result = await _testClass.GetStaffRole(ID, token);

            // Assert
            Assert.Equal(ID, result.ID);
        }

        [Fact]
        public async Task CanCallCreateStaffRole()
        {
            // Arrange
            var staffRole = new StaffRole
            {
                ID = 0,
                RoleID = 6,
                StaffID = 11,
                InputBy = "TestValue1383130818",
                InputOn = DateTime.UtcNow,
                Active = false
            };
            var token = jwtToken;

            // Act
            var result = await _testClass.CreateStaffRole(staffRole, token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallCreateStaffRoleWithNullStaffRole()
        {
            var result = await _testClass.CreateStaffRole(default(StaffRole), "TestValue1616617089");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallCreateStaffRoleWithInvalidToken(string value)
        {
            var result = await _testClass.CreateStaffRole(new StaffRole
            {
                ID = 0,
                RoleID = 6,
                StaffID = 11,
                InputBy = "TestValue1383130818",
                InputOn = DateTime.UtcNow,
                Active = false
            }, value);

            Assert.Null(result);
        }

        [Fact]
        public async Task CanCallUpdateStaffRole()
        {
            // Arrange
            var staffRole = new StaffRole
            {
                ID = 23,
                RoleID = 6,
                StaffID = 11,
                InputBy = "TestValue397142172",
                InputOn = DateTime.UtcNow,
                Active = false
            };
            var token = jwtToken;

            // Act
            var result = await _testClass.UpdateStaffRole(staffRole, token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallUpdateStaffRoleWithNullStaffRole()
        {
            var result = await _testClass.UpdateStaffRole(default(StaffRole), "TestValue1198101939");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallUpdateStaffRoleWithInvalidToken(string value)
        {
            var result = await _testClass.UpdateStaffRole(new StaffRole
            {
                ID = 23,
                RoleID = 6,
                StaffID = 11,
                InputBy = "TestValue108740203",
                InputOn = DateTime.UtcNow,
                Active = false
            }, value);

            Assert.Null(result);
        }

        [Fact]
        public async Task CanCallDeleteStaffRole()
        {
            // Arrange
            var ID = 30;
            var token = jwtToken;

            // Act
            var result = await _testClass.DeleteStaffRole(ID, token);

            // Assert
            Assert.Equal(ID, result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallDeleteStaffRoleWithInvalidToken(string value)
        {
            var result = await _testClass.DeleteStaffRole(30, value);

            Assert.Equal(30, result);
        }

        [Fact]
        public void CanSetAndGetStaffRoles()
        {
            // Arrange
            var testValue = new List<StaffRole>();

            // Act
            _testClass.StaffRoles = testValue;

            // Assert
            Assert.Same(testValue, _testClass.StaffRoles);
        }
    }
}