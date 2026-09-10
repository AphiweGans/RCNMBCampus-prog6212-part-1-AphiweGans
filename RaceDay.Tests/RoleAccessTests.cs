using System.Net;
using System.Net.Http.Json;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.Tests;

// Verifies that role-based access is enforced in both directions:
// Organisers cannot use Participant-only actions and vice versa.
public class RoleAccessTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public RoleAccessTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Organiser_CannotEnrolInCategory_ReturnsForbidden()
    {
        var organiserClient = await TestHelpers.GetAuthenticatedClientAsync(_factory, UserRole.Organiser);

        var eventResponse = await organiserClient.PostAsJsonAsync("/api/events", new EventCreateDto
        {
            EventName = "Role Test Event",
            EventDate = DateTime.UtcNow.AddMonths(1),
            Location = "Test City",
            Distance = 10m,
            EventType = EventType.Run
        });
        var createdEvent = await eventResponse.Content.ReadFromJsonAsync<Event>();

        var categoryResponse = await organiserClient.PostAsJsonAsync(
            $"/api/events/{createdEvent!.EventID}/categories",
            new CategoryCreateDto { CategoryName = "10km", DistanceKm = 10, MaxParticipants = 100, EntryFee = 50 });
        var createdCategory = await categoryResponse.Content.ReadFromJsonAsync<Category>();

        // An Organiser attempting the Participant-only enrol action must be rejected
        var enrolResponse = await organiserClient.PostAsync($"/api/categories/{createdCategory!.CategoryID}/enrol", null);

        Assert.Equal(HttpStatusCode.Forbidden, enrolResponse.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotCreateCategory_ReturnsForbidden()
    {
        var organiserClient = await TestHelpers.GetAuthenticatedClientAsync(_factory, UserRole.Organiser);
        var participantClient = await TestHelpers.GetAuthenticatedClientAsync(_factory, UserRole.Participant);

        var eventResponse = await organiserClient.PostAsJsonAsync("/api/events", new EventCreateDto
        {
            EventName = "Category Role Test Event",
            EventDate = DateTime.UtcNow.AddMonths(1),
            Location = "Test City",
            Distance = 5m,
            EventType = EventType.Walk
        });
        var createdEvent = await eventResponse.Content.ReadFromJsonAsync<Event>();

        // A Participant attempting the Organiser-only category creation action must be rejected
        var response = await participantClient.PostAsJsonAsync(
            $"/api/events/{createdEvent!.EventID}/categories",
            new CategoryCreateDto { CategoryName = "5km", DistanceKm = 5, MaxParticipants = 100, EntryFee = 20 });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotCaptureResults_ReturnsForbidden()
    {
        var participantClient = await TestHelpers.GetAuthenticatedClientAsync(_factory, UserRole.Participant);

        // Enrolment id doesn't need to exist for this check - the role check runs first
        var response = await participantClient.PostAsJsonAsync("/api/enrolments/1/results",
            new ResultCreateDto { Position = 1, Status = "Finished" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
