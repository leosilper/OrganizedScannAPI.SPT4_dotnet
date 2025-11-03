using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OrganizedScannApi.Api.Controllers;
using OrganizedScannApi.Domain.Entities;
using OrganizedScannApi.Domain.Enums;
using OrganizedScannApi.Infrastructure.Data;
using System;
using System.Collections.Generic;
using Xunit;

namespace OrganizedScannAPI.Tests
{
    public class AuthTests
    {
        private readonly ApplicationDbContext _context;
        private readonly AuthController _controller;
        private readonly IConfiguration _configuration;

        public AuthTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("AuthTestDb_" + Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            // Mock IConfiguration for JWT settings
            var inMemorySettings = new Dictionary<string, string> {
                {"JwtSettings:SecretKey", "SuperSecretKeyForJWTTokenGeneration123456789"},
                {"JwtSettings:Issuer", "OrganizedScannAPI"},
                {"JwtSettings:Audience", "OrganizedScannAPIClients"},
                {"JwtSettings:ExpirationInMinutes", "60"}
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();

            _controller = new AuthController(_context, _configuration);
        }

        [Fact]
        public async Task Register_ValidUser_Should_Return_Created()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "test@example.com",
                Password = "Password123",
                Role = 0
            };

            // Act
            var result = await _controller.Register(request);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(201, createdAtActionResult.StatusCode);
            var authResponse = Assert.IsType<AuthResponse>(createdAtActionResult.Value);
            Assert.NotEmpty(authResponse.Token);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            Assert.NotNull(user);
        }

        [Fact]
        public async Task Register_DuplicateEmail_Should_Return_BadRequest()
        {
            // Arrange
            var user = new User { Email = "duplicate@example.com", Password = "Password", Role = UserRole.USER };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var request = new RegisterRequest
            {
                Email = "duplicate@example.com",
                Password = "Test123456",
                Role = 0
            };

            // Act
            var result = await _controller.Register(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task Login_ValidCredentials_Should_Return_Token()
        {
            // Arrange
            var user = new User { Email = "login@example.com", Password = "Test123456", Role = UserRole.USER };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var request = new LoginRequest
            {
                Email = "login@example.com",
                Password = "Test123456"
            };

            // Act
            var result = await _controller.Login(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, okResult.StatusCode);

            var authResponse = Assert.IsType<AuthResponse>(okResult.Value);
            Assert.NotEmpty(authResponse.Token);
        }

        [Fact]
        public async Task Login_InvalidCredentials_Should_Return_Unauthorized()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "nonexistent@example.com",
                Password = "WrongPassword"
            };

            // Act
            var result = await _controller.Login(request);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result.Result);
        }
    }
}

