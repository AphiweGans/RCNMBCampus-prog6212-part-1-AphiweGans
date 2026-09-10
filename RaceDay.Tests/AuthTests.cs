using System.Net;
using System.Net.Http.Json;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.Tests;

public class AuthTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_NewParticipant_ReturnsCreated()
    {
        var client = _factory.CreateClient();

        var dto = new RegisterDto
        {
            FullName = "Test Participant",
            Email = $"participant_{Guid.NewGuid()}@test.com",
            Password = "Password123!",
            Role = UserRole.Participant
        };

        var response = await client.PostAsJsonAsync("/api/auth/register", dto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var email = $"dup_{Guid.NewGuid()}@test.com";
        var dto = new RegisterDto { FullName = "First User", Email = email, Password = "Password123!", Role = UserRole.Participant };

        await client.PostAsJsonAsync("/api/auth/register", dto);
        var secondAttempt = await client.PostAsJsonAsync("/api/auth/register", dto);

        Assert.Equal(HttpStatusCode.BadRequest, secondAttempt.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOk()
    {
        var client = _factory.CreateClient();
        var email = $"login_{Guid.NewGuid()}@test.com";

        await client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            FullName = "Login User",
            Email = email,
            Password = "Password123!",
            Role = UserRole.Organiser
        });

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login",
            new LoginDto { Email = email, Password = "Password123!" });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var email = $"wrongpw_{Guid.NewGuid()}@test.com";

        await client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            FullName = "Wrong Password User",
            Email = email,
            Password = "Password123!",
            Role = UserRole.Organiser
        });

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login",
            new LoginDto { Email = email, Password = "IncorrectPassword!" });

        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);
    }
}
