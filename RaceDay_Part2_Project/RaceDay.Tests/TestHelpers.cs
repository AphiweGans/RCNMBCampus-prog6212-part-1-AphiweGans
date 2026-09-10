using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.Tests;

// Shared helper so every test class can spin up a logged-in HttpClient
// for a given role without repeating the register+login boilerplate.
public static class TestHelpers
{
    public static async Task<HttpClient> GetAuthenticatedClientAsync(CustomWebApplicationFactory factory, UserRole role)
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });

        var email = $"{role}_{Guid.NewGuid()}@test.com";
        var registerDto = new RegisterDto
        {
            FullName = $"Test {role}",
            Email = email,
            Password = "Password123!",
            Role = role
        };

        await client.PostAsJsonAsync("/api/auth/register", registerDto);
        await client.PostAsJsonAsync("/api/auth/login", new LoginDto { Email = email, Password = "Password123!" });

        return client;
    }
}
