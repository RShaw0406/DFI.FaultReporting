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

    public class UserRoleServiceTests
    {
        private UserRoleService _testClass;
        private UserRoleHttp _userRoleHttp;
        private UserHttp _userHttp;

        public string jwtToken = "";

        public UserRoleServiceTests()
        {
            var mockFactory = new Mock<IHttpClientFactory>();

            var client = new HttpClient();

            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IHttpClientFactory factory = mockFactory.Object;

            var mockSettings = new Mock<ISettingsService>();

            ISettingsService settings = mockSettings.Object;

            _userRoleHttp = new UserRoleHttp(factory, settings);
            _testClass = new UserRoleService(_userRoleHttp);
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
            var instance = new UserRoleService(_userRoleHttp);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public async Task CanCallGetUserRoles()
        {
            // Act
            var result = await _testClass.GetUserRoles();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CanCallGetUserRole()
        {
            // Arrange
            var ID = 17;

            // Act
            var result = await _testClass.GetUserRole(ID);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetUserRolePerformsMapping()
        {
            // Arrange
            var ID = 17;

            // Act
            var result = await _testClass.GetUserRole(ID);

            // Assert
            Assert.Equal(ID, result.ID);
        }

        [Fact]
        public async Task CanCallCreateUserRole()
        {
            // Arrange
            var userRole = new UserRole
            {
                ID = 0,
                RoleID = 9,
                UserID = 8,
                InputBy = "TestValue1349312715",
                InputOn = DateTime.UtcNow,
                Active = false
            };

            // Act
            var result = await _testClass.CreateUserRole(userRole);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallCreateUserRoleWithNullUserRole()
        {
            var result = await _testClass.CreateUserRole(null);

            Assert.Null(result);
        }

        [Fact]
        public async Task CanCallUpdateUserRole()
        {
            // Arrange
            var userRole = new UserRole
            {
                ID = 17,
                RoleID = 1,
                UserID = 2,
                InputBy = "TestValue1021260519",
                InputOn = DateTime.UtcNow,
                Active = false
            };

            // Act
            var result = await _testClass.UpdateUserRole(userRole);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallUpdateUserRoleWithNullUserRole()
        {
            var result = await _testClass.UpdateUserRole(null);

            Assert.Null(result);
        }

        [Fact]
        public async Task CanCallDeleteUserRole()
        {
            // Arrange
            var ID = 17;

            // Act
            var result = await _testClass.DeleteUserRole(ID);

            // Assert
            Assert.Equal(ID, result);
        }

        [Fact]
        public void CanSetAndGetUserRoles()
        {
            // Arrange
            var testValue = new List<UserRole>();

            // Act
            _testClass.UserRoles = testValue;

            // Assert
            Assert.Same(testValue, _testClass.UserRoles);
        }
    }
}