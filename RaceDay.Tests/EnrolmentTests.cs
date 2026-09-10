using System.Net;
using System.Net.Http.Json;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.Tests;

public class EnrolmentTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public EnrolmentTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Participant_CanEnrolInCategory_AndEnrolmentIsRecordedCorrectly()
    {
        var organiserClient = await TestHelpers.GetAuthenticatedClientAsync(_factory, UserRole.Organiser);
        var participantClient = await TestHelpers.GetAuthenticatedClientAsync(_factory, UserRole.Participant);

        var eventResponse = await organiserClient.PostAsJsonAsync("/api/events", new EventCreateDto
        {
            EventName = "Enrolment Test Event",
            EventDate = DateTime.UtcNow.AddMonths(1),
            Location = "Test City",
            Distance = 21.1m,
            EventType = EventType.Run
        });
        var createdEvent = await eventResponse.Content.ReadFromJsonAsync<Event>();

        var categoryResponse = await organiserClient.PostAsJsonAsync(
            $"/api/events/{createdEvent!.EventID}/categories",
            new CategoryCreateDto { CategoryName = "21km", DistanceKm = 21, MaxParticipants = 200, EntryFee = 150 });
        var createdCategory = await categoryResponse.Content.ReadFromJsonAsync<Category>();

        var enrolResponse = await participantClient.PostAsync($"/api/categories/{createdCategory!.CategoryID}/enrol", null);
        Assert.Equal(HttpStatusCode.Created, enrolResponse.StatusCode);

        var myEnrolmentsResponse = await participantClient.GetAsync("/api/enrolments/me");
        Assert.Equal(HttpStatusCode.OK, myEnrolmentsResponse.StatusCode);

        var enrolments = await myEnrolmentsResponse.Content.ReadFromJsonAsync<List<Enrolment>>();

        Assert.Single(enrolments!);
        Assert.Equal(createdCategory.CategoryID, enrolments![0].CategoryID);
    }

    [Fact]
    public async Task Participant_CannotEnrolTwiceInSameCategory_ReturnsConflict()
    {
        var organiserClient = await TestHelpers.GetAuthenticatedClientAsync(_factory, UserRole.Organiser);
        var participantClient = await TestHelpers.GetAuthenticatedClientAsync(_factory, UserRole.Participant);

        var eventResponse = await organiserClient.PostAsJsonAsync("/api/events", new EventCreateDto
        {
            EventName = "Double Enrol Event",
            EventDate = DateTime.UtcNow.AddMonths(1),
            Location = "Test City",
            Distance = 5m,
            EventType = EventType.Walk
        });
        var createdEvent = await eventResponse.Content.ReadFromJsonAsync<Event>();

        var categoryResponse = await organiserClient.PostAsJsonAsync(
            $"/api/events/{createdEvent!.EventID}/categories",
            new CategoryCreateDto { CategoryName = "5km", DistanceKm = 5, MaxParticipants = 100, EntryFee = 20 });
        var createdCategory = await categoryResponse.Content.ReadFromJsonAsync<Category>();

        await participantClient.PostAsync($"/api/categories/{createdCategory!.CategoryID}/enrol", null);
        var secondAttempt = await participantClient.PostAsync($"/api/categories/{createdCategory.CategoryID}/enrol", null);

        Assert.Equal(HttpStatusCode.Conflict, secondAttempt.StatusCode);
    }

    [Fact]
    public async Task UnauthenticatedUser_CannotEnrol_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/categories/1/enrol", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
