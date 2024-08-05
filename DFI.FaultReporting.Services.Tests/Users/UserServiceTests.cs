namespace DFI.FaultReporting.Services.Tests.Users
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using DFI.FaultReporting.Http.Users;
    using DFI.FaultReporting.JWT.Requests;
    using DFI.FaultReporting.JWT.Response;
    using DFI.FaultReporting.Models.Users;
    using DFI.FaultReporting.Services.Interfaces.Settings;
    using DFI.FaultReporting.Services.Users;
    using Moq;
    using Xunit;

    public class UserServiceTests
    {
        private UserService _testClass;
        private UserHttp _userHttp;

        public string jwtToken = "";

        public UserServiceTests()
        {
            var mockFactory = new Mock<IHttpClientFactory>();

            var client = new HttpClient();

            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IHttpClientFactory factory = mockFactory.Object;

            var mockSettings = new Mock<ISettingsService>();

            ISettingsService settings = mockSettings.Object;

            _userHttp = new UserHttp(factory, settings);
            _testClass = new UserService(_userHttp);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new UserService(_userHttp);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public async Task CanCallRegister()
        {
            // Arrange
            var registrationRequest = new RegistrationRequest
            {
                Email = "test@test123.com",
                Password = "Now&then0406",
                ConfirmPassword = "Now&then0406",
                Prefix = "Mr",
                FirstName = "Test",
                LastName = "Test",
                DayDOB = 04,
                MonthDOB = 06,
                YearDOB = 1995,
                ContactNumber = "07789665443",
                Postcode = "BT1 8PA",
                AddressLine1 = "Test",
                AddressLine2 = "Test",
                AddressLine3 = "Test"
            };

            // Act
            var result = await _testClass.Register(registrationRequest);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallRegisterWithNullRegistrationRequest()
        {
            var result = await _testClass.Register(default(RegistrationRequest));

            Assert.Null(result);
        }

        [Fact]
        public async Task CanCallLogin()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "test@test.com",
                Password = "Now&then0406"
            };

            // Act
            var result = await _testClass.Login(loginRequest);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallLoginWithNullLoginRequest()
        {
            var result = await _testClass.Login(default(LoginRequest));

            Assert.Null(result.UserName);
        }

        [Fact]
        public async Task CanCallLock()
        {
            // Arrange
            var emailAddress = "test@test89.com";

            // Act
            var result = await _testClass.Lock(emailAddress);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallLockWithInvalidEmailAddress(string value)
        {
            var result = await _testClass.Lock(value);

            Assert.Null(result.UserName);
        }

        [Fact]
        public async Task CanCallGetUsers()
        {
            LoginRequest loginRequest = new LoginRequest
            {
                Email = "test@test.com",
                Password = "Now&then0406"
            };

            AuthResponse authResponse = _userHttp.Login(loginRequest).Result;

            jwtToken = authResponse.Token;

            // Arrange
            var token = jwtToken;

            // Act
            var result = await _testClass.GetUsers(token);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetUsersWithInvalidToken(string value)
        {
            var result = await _testClass.GetUsers(value);

            Assert.Null(result);
        }

        [Fact]
        public async Task CanCallGetUser()
        {
            LoginRequest loginRequest = new LoginRequest
            {
                Email = "test@test.com",
                Password = "Now&then0406"
            };

            AuthResponse authResponse = _userHttp.Login(loginRequest).Result;

            jwtToken = authResponse.Token;

            // Arrange
            var ID = 10;
            var token = jwtToken;

            // Act
            var result = await _testClass.GetUser(ID, token);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetUserWithInvalidToken(string value)
        {
            var result = await _testClass.GetUser(10, value);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserPerformsMapping()
        {
            LoginRequest loginRequest = new LoginRequest
            {
                Email = "test@test.com",
                Password = "Now&then0406"
            };

            AuthResponse authResponse = _userHttp.Login(loginRequest).Result;

            jwtToken = authResponse.Token;

            // Arrange
            var ID = 10;
            var token = jwtToken;

            // Act
            var result = await _testClass.GetUser(ID, token);

            // Assert
            Assert.Equal(ID, result.ID);
        }

        [Fact]
        public async Task CanCallCheckEmail()
        {
            // Arrange
            var email = "test@test66.com";

            // Act
            var result = await _testClass.CheckEmail(email);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallCheckEmailWithInvalidEmail(string value)
        {
            var result = await _testClass.CheckEmail(value);

            Assert.False(result);
        }

        [Fact]
        public async Task CanCallResetPassword()
        {
            // Arrange
            var email = "test@test66.com";
            var password = "Now&then0406";

            // Act
            var result = await _testClass.ResetPassword(email, password);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallResetPasswordWithInvalidEmail(string value)
        {
            var result = await _testClass.ResetPassword(value, "Now&then0406");

            Assert.False(result);
        }

        [Fact]
        public async Task CanCallUpdateUser()
        {
            LoginRequest loginRequest = new LoginRequest
            {
                Email = "test@test.com",
                Password = "Now&then0406"
            };

            AuthResponse authResponse = _userHttp.Login(loginRequest).Result;

            jwtToken = authResponse.Token;

            // Arrange
            var user = new User
            {
                ID = 10,
                Email = "test@test99.com",
                Password = "Now&then0406",
                PasswordSalt = "jzF7ebH5YEHnKrsPLAKHYg==",
                Prefix = "Mr",
                FirstName = "Test",
                LastName = "Test",
                DOB = DateTime.UtcNow,
                AddressLine1 = "Test",
                AddressLine2 = "Test",
                AddressLine3 = "Test",
                Postcode = "BT1 8PA",
                ContactNumber = "07789665443",
                AccountLocked = false,
                AccountLockedEnd = DateTime.UtcNow,
                InputBy = "TestValue562371269",
                InputOn = DateTime.UtcNow,
                Active = false
            };
            var token = jwtToken;

            // Act
            var result = await _testClass.UpdateUser(user, token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallUpdateUserWithNullUser()
        {
            var result = await _testClass.UpdateUser(default(User), "TestValue1737832743");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallUpdateUserWithInvalidToken(string value)
        {
            LoginRequest loginRequest = new LoginRequest
            {
                Email = "test@test.com",
                Password = "Now&then0406"
            };

            AuthResponse authResponse = _userHttp.Login(loginRequest).Result;

            jwtToken = authResponse.Token;

            var result = await _testClass.UpdateUser(new User
            {
                ID = 10,
                Email = "test@test66.com",
                Password = "Now&then0406",
                PasswordSalt = "jzF7ebH5YEHnKrsPLAKHYg==",
                Prefix = "Mr",
                FirstName = "Test",
                LastName = "Test",
                DOB = DateTime.UtcNow,
                AddressLine1 = "Test",
                AddressLine2 = "Test",
                AddressLine3 = "Test",
                Postcode = "BT1 8PA",
                ContactNumber = "07789665443",
                AccountLocked = false,
                AccountLockedEnd = DateTime.UtcNow,
                InputBy = "TestValue562371269",
                InputOn = DateTime.UtcNow,
                Active = false
            }, value);

            Assert.Null(result);
        }

        [Fact]
        public async Task CanCallDeleteUser()
        {
            // Arrange
            var ID = 10;
            var token = jwtToken;

            // Act
            var result = await _testClass.DeleteUser(ID, token);

            // Assert
            Assert.Equal(ID, result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallDeleteUserWithInvalidToken(string value)
        {
            var result = await _testClass.DeleteUser(10, value);

            Assert.Equal(10, result);
        }

        [Fact]
        public void CanSetAndGetUsers()
        {
            // Arrange
            var testValue = new List<User>();

            // Act
            _testClass.Users = testValue;

            // Assert
            Assert.Same(testValue, _testClass.Users);
        }
    }
}