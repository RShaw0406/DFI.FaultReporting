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

    public class StaffServiceTests
    {
        private StaffService _testClass;
        private StaffHttp _staffHttp;
        private UserHttp _userHttp;

        public string jwtToken = "";

        public StaffServiceTests()
        {
            var mockFactory = new Mock<IHttpClientFactory>();

            var client = new HttpClient();

            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IHttpClientFactory factory = mockFactory.Object;

            var mockSettings = new Mock<ISettingsService>();

            ISettingsService settings = mockSettings.Object;

            _staffHttp = new StaffHttp(factory, settings);
            _testClass = new StaffService(_staffHttp);
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
            var instance = new StaffService(_staffHttp);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public async Task CanCallLogin()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "rshaw0406@outlook.com",
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

            // Assert
            Assert.Null(result.UserName);
        }

        [Fact]
        public async Task CanCallLock()
        {
            // Arrange
            var emailAddress = "test9@test.com";

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
        public async Task CanCallGetAllStaff()
        {
            // Arrange
            var token = jwtToken;

            // Act
            var result = await _testClass.GetAllStaff(token);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetAllStaffWithInvalidToken(string value)
        {
            var result = await _testClass.GetAllStaff(value);

            Assert.Null(result);
        }

        [Fact]
        public async Task CanCallGetStaff()
        {
            // Arrange
            var ID = 16;
            var token = jwtToken;

            // Act
            var result = await _testClass.GetStaff(ID, token);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetStaffWithInvalidToken(string value)
        {
            var result = await _testClass.GetStaff(16, value);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetStaffPerformsMapping()
        {
            // Arrange
            var ID = 16;
            var token = jwtToken;

            // Act
            var result = await _testClass.GetStaff(ID, token);

            // Assert
            Assert.Equal(ID, result.ID);
        }

        [Fact]
        public async Task CanCallCheckEmail()
        {
            // Arrange
            var email = "test9@test.com";

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
            var email = "test9@test.com";
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
        public async Task CanCallCreateStaff()
        {
            // Arrange
            var staff = new Staff
            {
                ID = 0,
                Email = "test@test120.com",
                Password = "Now&oTYKDeB/pfl80e2yGztlPsdqcRFT1IlukiIkCpks/QY=",
                PasswordSalt = "3I51ofH+ALbKqqOCCXTqaw==",
                Prefix = "Mr",
                FirstName = "Test",
                LastName = "Test",
                AccountLocked = false,
                AccountLockedEnd = DateTime.UtcNow,
                InputBy = "TestValue847696626",
                InputOn = DateTime.UtcNow,
                Active = false
            };
            var token = jwtToken;

            // Act
            var result = await _testClass.CreateStaff(staff, token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallCreateStaffWithNullStaff()
        {
            var result = await _testClass.CreateStaff(default(Staff), "TestValue2094678561");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallCreateStaffWithInvalidToken(string value)
        {
            var result = await _testClass.CreateStaff(new Staff
            {
                ID = 0,
                Email = "test@test10.com",
                Password = "Now&oTYKDeB/pfl80e2yGztlPsdqcRFT1IlukiIkCpks/QY=",
                PasswordSalt = "3I51ofH+ALbKqqOCCXTqaw==",
                Prefix = "Mr",
                FirstName = "Test",
                LastName = "Test",
                AccountLocked = false,
                AccountLockedEnd = DateTime.UtcNow,
                InputBy = "TestValue847696626",
                InputOn = DateTime.UtcNow,
                Active = false
            }, value);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CanCallUpdateStaff()
        {
            // Arrange
            var staff = new Staff
            {
                ID = 16,
                Email = "test9@test.com",
                Password = "oTYKDeB/pfl80e2yGztlPsdqcRFT1IlukiIkCpks/QY=",
                PasswordSalt = "3I51ofH+ALbKqqOCCXTqaw==",
                Prefix = "Mr",
                FirstName = "Test",
                LastName = "Test",
                AccountLocked = false,
                AccountLockedEnd = DateTime.UtcNow,
                InputBy = "TestValue1847315344",
                InputOn = DateTime.UtcNow,
                Active = false
            };
            var token = jwtToken;

            // Act
            var result = await _testClass.UpdateStaff(staff, token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallUpdateStaffWithNullStaff()
        {
            var result = await _testClass.UpdateStaff(default(Staff), "TestValue904550147");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallUpdateStaffWithInvalidToken(string value)
        {
            var result = await _testClass.UpdateStaff(new Staff
            {
                ID = 16,
                Email = "test9@test.com",
                Password = "oTYKDeB/pfl80e2yGztlPsdqcRFT1IlukiIkCpks/QY=",
                PasswordSalt = "3I51ofH+ALbKqqOCCXTqaw==",
                Prefix = "Mr",
                FirstName = "Test",
                LastName = "Test",
                AccountLocked = false,
                AccountLockedEnd = DateTime.UtcNow,
                InputBy = "TestValue1847315344",
                InputOn = DateTime.UtcNow,
                Active = false
            }, value);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void CanSetAndGetStaff()
        {
            // Arrange
            var testValue = new List<Staff>();

            // Act
            _testClass.Staff = testValue;

            // Assert
            Assert.Same(testValue, _testClass.Staff);
        }
    }
}