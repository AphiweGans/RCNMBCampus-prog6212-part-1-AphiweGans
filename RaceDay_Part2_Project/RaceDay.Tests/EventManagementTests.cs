using System.Net;
using System.Net.Http.Json;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.Tests;

public class EventManagementTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public EventManagementTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Organiser_CanCreateEvent_ReturnsCreated()
    {
        var client = await TestHelpers.GetAuthenticatedClientAsync(_factory, UserRole.Organiser);

        var eventDto = new EventCreateDto
        {
            EventName = "Test Marathon",
            EventDate = DateTime.UtcNow.AddMonths(1),
            Location = "Test City",
            Distance = 42.2m,
            EventType = EventType.Run
        };

        var response = await client.PostAsJsonAsync("/api/events", eventDto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotCreateEvent_ReturnsForbidden()
    {
        var client = await TestHelpers.GetAuthenticatedClientAsync(_factory, UserRole.Participant);

        var eventDto = new EventCreateDto
        {
            EventName = "Unauthorized Event",
            EventDate = DateTime.UtcNow.AddMonths(1),
            Location = "Test City",
            Distance = 10m,
            EventType = EventType.Run
        };

        var response = await client.PostAsJsonAsync("/api/events", eventDto);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UnauthenticatedUser_CannotCreateEvent_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var eventDto = new EventCreateDto
        {
            EventName = "No Auth Event",
            EventDate = DateTime.UtcNow.AddMonths(1),
            Location = "Test City",
            Distance = 10m,
            EventType = EventType.Run
        };

        var response = await client.PostAsJsonAsync("/api/events", eventDto);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AnyUser_CanViewEvents_ReturnsOk()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/events");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_CannotUpdateAnotherOrganisersEvent_ReturnsForbidden()
    {
        var ownerClient = await TestHelpers.GetAuthenticatedClientAsync(_factory, UserRole.Organiser);
        var otherClient = await TestHelpers.GetAuthenticatedClientAsync(_factory, UserRole.Organiser);

        var createResponse = await ownerClient.PostAsJsonAsync("/api/events", new EventCreateDto
        {
            EventName = "Owned Event",
            EventDate = DateTime.UtcNow.AddMonths(1),
            Location = "Test City",
            Distance = 10m,
            EventType = EventType.Run
        });

        var createdEvent = await createResponse.Content.ReadFromJsonAsync<Event>();

        var updateResponse = await otherClient.PutAsJsonAsync($"/api/events/{createdEvent!.EventID}", new EventCreateDto
        {
            EventName = "Hijacked Event",
            EventDate = DateTime.UtcNow.AddMonths(2),
            Location = "Different City",
            Distance = 21.1m,
            EventType = EventType.Walk
        });

        Assert.Equal(HttpStatusCode.Forbidden, updateResponse.StatusCode);
    }
}
