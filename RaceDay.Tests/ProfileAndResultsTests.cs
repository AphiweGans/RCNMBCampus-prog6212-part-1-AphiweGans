using System.Net;
using System.Net.Http.Json;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.Tests;

public class ProfileAndResultsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ProfileAndResultsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task LoggedInUser_CanViewOwnProfile_ReturnsOk()
    {
        var client = await TestHelpers.GetAuthenticatedClientAsync(_factory, UserRole.Participant);

        var response = await client.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UnauthenticatedUser_CannotViewProfile_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task LoggedInUser_CanUpdateOwnProfile_ReturnsOk()
    {
        var client = await TestHelpers.GetAuthenticatedClientAsync(_factory, UserRole.Organiser);

        var response = await client.PutAsJsonAsync("/api/users/me", new ProfileUpdateDto
        {
            FullName = "Updated Name",
            Phone = "0821112222"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_CanCaptureResult_ForOwnEventEnrolment_ReturnsCreated()
    {
        var organiserClient = await TestHelpers.GetAuthenticatedClientAsync(_factory, UserRole.Organiser);
        var participantClient = await TestHelpers.GetAuthenticatedClientAsync(_factory, UserRole.Participant);

        var eventResponse = await organiserClient.PostAsJsonAsync("/api/events", new EventCreateDto
        {
            EventName = "Results Test Event",
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

        var enrolResponse = await participantClient.PostAsync($"/api/categories/{createdCategory!.CategoryID}/enrol", null);
        var createdEnrolment = await enrolResponse.Content.ReadFromJsonAsync<Enrolment>();

        var resultResponse = await organiserClient.PostAsJsonAsync(
            $"/api/enrolments/{createdEnrolment!.EnrolmentID}/results",
            new ResultCreateDto { Position = 1, Status = "Finished" });

        Assert.Equal(HttpStatusCode.Created, resultResponse.StatusCode);

        var myResultsResponse = await participantClient.GetAsync("/api/participants/me/results");
        Assert.Equal(HttpStatusCode.OK, myResultsResponse.StatusCode);

        var results = await myResultsResponse.Content.ReadFromJsonAsync<List<Result>>();
        Assert.Single(results!);
    }

    [Fact]
    public async Task AnyUser_CanViewCategoryResultsLeaderboard_ReturnsOk()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/categories/1/results");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
