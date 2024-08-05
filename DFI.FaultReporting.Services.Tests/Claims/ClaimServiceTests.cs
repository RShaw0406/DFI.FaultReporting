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

    public class ClaimServiceTests
    {
        private ClaimService _testClass;
        private ClaimHttp _claimHttp;
        private UserHttp _userHttp;

        public string jwtToken = "";

        public ClaimServiceTests()
        {
            var mockFactory = new Mock<IHttpClientFactory>();

            var client = new HttpClient();

            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IHttpClientFactory factory = mockFactory.Object;

            var mockSettings = new Mock<ISettingsService>();

            ISettingsService settings = mockSettings.Object;

            _claimHttp = new ClaimHttp(factory, settings);
            _testClass = new ClaimService(_claimHttp);
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
            var instance = new ClaimService(_claimHttp);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public async Task CanCallGetClaims()
        {
            // Arrange
            var token = jwtToken;

            // Act
            var result = await _testClass.GetClaims(token);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetClaimsWithInvalidToken(string value)
        {
            var result = await _testClass.GetClaims(value);

            Assert.Null(result);
        }

        [Fact]
        public async Task CanCallGetClaim()
        {
            // Arrange
            var ID = 8;
            var token = jwtToken;

            // Act
            var result = await _testClass.GetClaim(ID, token);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetClaimWithInvalidToken(string value)
        {
            var result = await _testClass.GetClaim(8, value);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetClaimPerformsMapping()
        {
            // Arrange
            var ID = 8;
            var token = jwtToken;

            // Act
            var result = await _testClass.GetClaim(ID, token);

            // Assert
            Assert.Equal(ID, result.ID);
        }

        [Fact]
        public async Task CanCallCreateClaim()
        {
            // Arrange
            var claim = new Claim
            {
                ID = 0,
                ClaimTypeID = 16,
                ClaimStatusID = 24,
                UserID = 8,
                StaffID = 16,
                FaultID = 24,
                IncidentDate = DateTime.UtcNow,
                IncidentDescription = "TestValue627350521",
                IncidentLocationDescription = "TestValue445645332",
                IncidentLocationLatitude = "TestValue1042710916",
                IncidentLocationLongitude = "TestValue1305186930",
                InjuryDescription = "TestValue326710003",
                DamageDescription = "TestValue1153696079",
                DamageClaimDescription = "TestValue1339714064",
                InputBy = "TestValue2073558489",
                InputOn = DateTime.UtcNow,
                Active = false
            };
            var token = jwtToken;

            // Act
            var result = await _testClass.CreateClaim(claim, token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallCreateClaimWithNullClaim()
        {
            var result = await _testClass.CreateClaim(default(Claim), "TestValue543848077");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallCreateClaimWithInvalidToken(string value)
        {
            var result = await _testClass.CreateClaim(new Claim
            {
                ID = 0,
                ClaimTypeID = 16,
                ClaimStatusID = 24,
                UserID = 8,
                StaffID = 16,
                FaultID = 24,
                IncidentDate = DateTime.UtcNow,
                IncidentDescription = "TestValue627350521",
                IncidentLocationDescription = "TestValue445645332",
                IncidentLocationLatitude = "TestValue1042710916",
                IncidentLocationLongitude = "TestValue1305186930",
                InjuryDescription = "TestValue326710003",
                DamageDescription = "TestValue1153696079",
                DamageClaimDescription = "TestValue1339714064",
                InputBy = "TestValue2073558489",
                InputOn = DateTime.UtcNow,
                Active = false
            }, value);

            Assert.Null(result);
        }

        [Fact]
        public async Task CanCallUpdateClaim()
        {
            // Arrange
            var claim = new Claim
            {
                ID = 8,
                ClaimTypeID = 16,
                ClaimStatusID = 24,
                UserID = 8,
                StaffID = 16,
                FaultID = 24,
                IncidentDate = DateTime.UtcNow,
                IncidentDescription = "TestValue692116275",
                IncidentLocationDescription = "TestValue222958778",
                IncidentLocationLatitude = "TestValue898826098",
                IncidentLocationLongitude = "TestValue1483037048",
                InjuryDescription = "TestValue1639968177",
                DamageDescription = "TestValue1662700713",
                DamageClaimDescription = "TestValue534968261",
                InputBy = "TestValue48888295",
                InputOn = DateTime.UtcNow,
                Active = true
            };
            var token = jwtToken;

            // Act
            var result = await _testClass.UpdateClaim(claim, token);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CannotCallUpdateClaimWithNullClaim()
        {
            var result = await _testClass.UpdateClaim(default(Claim), "TestValue247234275");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallUpdateClaimWithInvalidToken(string value)
        {
            var result = await _testClass.UpdateClaim(new Claim
            {
                ID = 8,
                ClaimTypeID = 16,
                ClaimStatusID = 24,
                UserID = 8,
                StaffID = 16,
                FaultID = 24,
                IncidentDate = DateTime.UtcNow,
                IncidentDescription = "TestValue692116275",
                IncidentLocationDescription = "TestValue222958778",
                IncidentLocationLatitude = "TestValue898826098",
                IncidentLocationLongitude = "TestValue1483037048",
                InjuryDescription = "TestValue1639968177",
                DamageDescription = "TestValue1662700713",
                DamageClaimDescription = "TestValue534968261",
                InputBy = "TestValue48888295",
                InputOn = DateTime.UtcNow,
                Active = true
            }, value);

            Assert.Null(result);
        }

        [Fact]
        public void CanSetAndGetClaims()
        {
            // Arrange
            var testValue = new List<Claim>();

            // Act
            _testClass.Claims = testValue;

            // Assert
            Assert.Same(testValue, _testClass.Claims);
        }
    }
}