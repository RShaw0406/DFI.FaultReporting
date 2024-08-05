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

    public class RoleServiceTests
    {
        private RoleService _testClass;
        private RoleHttp _roleHttp;
        private UserHttp _userHttp;

        public string jwtToken = "";

        public RoleServiceTests()
        {
            var mockFactory = new Mock<IHttpClientFactory>();

            var client = new HttpClient();

            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IHttpClientFactory factory = mockFactory.Object;

            var mockSettings = new Mock<ISettingsService>();

            ISettingsService settings = mockSettings.Object;

            _roleHttp = new RoleHttp(factory, settings);
            _testClass = new RoleService(_roleHttp);
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
            var instance = new RoleService(_roleHttp);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public async Task CanCallGetRoles()
        {
            // Arrange
            var token = jwtToken;

            // Act
            var result = await _testClass.GetRoles(token);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetRolesWithInvalidToken(string value)
        {
            var result = await _testClass.GetRoles(value);

            Assert.Null(result);
        }

        [Fact]
        public async Task CanCallGetRole()
        {
            // Arrange
            var ID = 8;
            var token = jwtToken;

            // Act
            var result = await _testClass.GetRole(ID, token);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetRoleWithInvalidToken(string value)
        {
            var result = await _testClass.GetRole(8, value);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetRolePerformsMapping()
        {
            // Arrange
            var ID = 8;
            var token = jwtToken;

            // Act
            var result = await _testClass.GetRole(ID, token);

            // Assert
            Assert.Equal(ID, result.ID);
        }

        [Fact]
        public async Task CanCallCreateRole()
        {
            // Arrange
            var role = new Role
            {
                ID = 0,
                RoleDescription = "TestValue1597737059",
                InputBy = "TestValue1067803365",
                InputOn = DateTime.UtcNow,
                Active = false
            };
            var token = jwtToken;

            // Act
            var result = await _testClass.CreateRole(role, token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallCreateRoleWithNullRole()
        {
            var result = await _testClass.CreateRole(default(Role), "TestValue1490194403");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallCreateRoleWithInvalidToken(string value)
        {
            var result = await _testClass.CreateRole(new Role
            {
                ID = 0,
                RoleDescription = "TestValue1840330809",
                InputBy = "TestValue674761717",
                InputOn = DateTime.UtcNow,
                Active = false
            }, value);

            Assert.Null(result);
        }

        [Fact]
        public async Task CanCallUpdateRole()
        {
            // Arrange
            var role = new Role
            {
                ID = 8,
                RoleDescription = "TestValue1011959552",
                InputBy = "TestValue1920695244",
                InputOn = DateTime.UtcNow,
                Active = true
            };
            var token = jwtToken;

            // Act
            var result = await _testClass.UpdateRole(role, token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallUpdateRoleWithNullRole()
        {
            var result = await _testClass.UpdateRole(default(Role), "TestValue64869766");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallUpdateRoleWithInvalidToken(string value)
        {
            var result = await _testClass.UpdateRole(new Role
            {
                ID = 8,
                RoleDescription = "TestValue648579155",
                InputBy = "TestValue326971825",
                InputOn = DateTime.UtcNow,
                Active = false
            }, value);

            Assert.Null(result);
        }

        [Fact]
        public void CanSetAndGetRoles()
        {
            // Arrange
            var testValue = new List<Role>();

            // Act
            _testClass.Roles = testValue;

            // Assert
            Assert.Same(testValue, _testClass.Roles);
        }
    }
}