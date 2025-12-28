namespace ODataConsoleApp.Tests.Infrastructure;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using ODataConsoleApp.Core.Configuration;
using ODataConsoleApp.Infrastructure.Repositories;
using ODataConsoleApp.Models;
using System.Net;

public class ODataRepositoryTests
{
    private readonly Mock<ILogger<ODataRepository>> _mockLogger;
    private readonly ODataServiceSettings _settings;

    public ODataRepositoryTests()
    {
        _mockLogger = new Mock<ILogger<ODataRepository>>();
        _settings = new ODataServiceSettings
        {
            BaseUrl = "http://services.odata.org/TripPinRESTierService/(S(3mslpb2bc0k5ufk24olpghzx))/",
            DefaultPageSize = 10,
            RequestTimeout = 30
        };
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_Should_ThrowArgumentNullException_When_HttpClientIsNull()
    {
        // Arrange
        var options = Options.Create(_settings);

        // Act
        Action act = () => new ODataRepository(null!, options, _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .Where(ex => ex.ParamName == "httpClient");
    }

    [Fact]
    public void Constructor_Should_ThrowArgumentNullException_When_OptionsIsNull()
    {
        // Arrange
        var httpClient = new HttpClient();

        // Act
        Action act = () => new ODataRepository(httpClient, null!, _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .Where(ex => ex.ParamName == "settings");
    }

    #endregion

    #region GetPeopleAsync Tests

    [Fact]
    public async Task GetPeopleAsync_Should_ReturnPaginatedResults_When_ValidSkipTopProvided()
    {
        // Arrange
        var skip = 0;
        var top = 2;
        var odataResponse = CreateODataPeopleResponse(new[]
        {
            CreatePersonJson("user1", "John", "Doe"),
            CreatePersonJson("user2", "Jane", "Smith")
        }, 20);

        var repository = CreateRepositoryWithMockedHttp(odataResponse, HttpStatusCode.OK);

        // Act
        var result = await repository.GetPeopleAsync(skip, top);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.PageSize.Should().Be(top);
        result.CurrentPage.Should().Be(1);
    }

    [Fact]
    public async Task GetPeopleAsync_Should_ReturnCorrectPage_When_SkipIndicatesSecondPage()
    {
        // Arrange
        var skip = 10;
        var top = 10;
        var odataResponse = CreateODataPeopleResponse(new[]
        {
            CreatePersonJson("user11", "Test", "User")
        }, 25);

        var repository = CreateRepositoryWithMockedHttp(odataResponse, HttpStatusCode.OK);

        // Act
        var result = await repository.GetPeopleAsync(skip, top);

        // Assert
        result.Should().NotBeNull();
        result.CurrentPage.Should().Be(2);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task GetPeopleAsync_Should_ReturnEmptyResult_When_NoPeopleExist()
    {
        // Arrange
        var odataResponse = CreateODataPeopleResponse(Array.Empty<string>(), 0);
        var repository = CreateRepositoryWithMockedHttp(odataResponse, HttpStatusCode.OK);

        // Act
        var result = await repository.GetPeopleAsync(0, 10);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetPeopleAsync_Should_ThrowHttpRequestException_When_ServiceUnavailable()
    {
        // Arrange
        var repository = CreateRepositoryWithMockedHttp("Service Unavailable", HttpStatusCode.ServiceUnavailable);

        // Act
        Func<Task> act = async () => await repository.GetPeopleAsync(0, 10);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Theory]
    [InlineData(-1, 10)]
    [InlineData(0, 0)]
    public async Task GetPeopleAsync_Should_ThrowArgumentException_When_InvalidParameters(int skip, int top)
    {
        // Arrange
        var repository = CreateRepositoryWithMockedHttp("{}", HttpStatusCode.OK);

        // Act
        Func<Task> act = async () => await repository.GetPeopleAsync(skip, top);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    #endregion

    #region SearchPeopleAsync Tests

    [Fact]
    public async Task SearchPeopleAsync_Should_ReturnMatchingPeople_When_ValidFilterProvided()
    {
        // Arrange
        var filterQuery = "FirstName eq 'John'";
        var odataResponse = CreateODataSearchResponse(new[]
        {
            CreatePersonJson("john1", "John", "Doe"),
            CreatePersonJson("john2", "John", "Smith")
        });

        var repository = CreateRepositoryWithMockedHttp(odataResponse, HttpStatusCode.OK);

        // Act
        var result = await repository.SearchPeopleAsync(filterQuery);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(p => p.FirstName.Should().Be("John"));
    }

    [Fact]
    public async Task SearchPeopleAsync_Should_ReturnAllPeople_When_FilterQueryIsEmpty()
    {
        // Arrange
        var odataResponse = CreateODataSearchResponse(new[]
        {
            CreatePersonJson("user1", "John", "Doe"),
            CreatePersonJson("user2", "Jane", "Smith")
        });

        var repository = CreateRepositoryWithMockedHttp(odataResponse, HttpStatusCode.OK);

        // Act
        var result = await repository.SearchPeopleAsync(string.Empty);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchPeopleAsync_Should_ReturnEmptyCollection_When_NoMatchesFound()
    {
        // Arrange
        var odataResponse = CreateODataSearchResponse(Array.Empty<string>());
        var repository = CreateRepositoryWithMockedHttp(odataResponse, HttpStatusCode.OK);

        // Act
        var result = await repository.SearchPeopleAsync("FirstName eq 'NonExistent'");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchPeopleAsync_Should_ThrowArgumentNullException_When_FilterQueryIsNull()
    {
        // Arrange
        var repository = CreateRepositoryWithMockedHttp("{}", HttpStatusCode.OK);

        // Act
        Func<Task> act = async () => await repository.SearchPeopleAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .Where(ex => ex.ParamName == "filterQuery");
    }

    [Fact]
    public async Task SearchPeopleAsync_Should_ThrowHttpRequestException_When_NetworkFails()
    {
        // Arrange
        var repository = CreateRepositoryWithMockedHttp("Internal Server Error", HttpStatusCode.InternalServerError);

        // Act
        Func<Task> act = async () => await repository.SearchPeopleAsync("FirstName eq 'John'");

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    #endregion

    #region GetPersonByUsernameAsync Tests

    [Fact]
    public async Task GetPersonByUsernameAsync_Should_ReturnPerson_When_UsernameExists()
    {
        // Arrange
        var username = "russellwhyte";
        var personJson = CreateSinglePersonResponse("russellwhyte", "Russell", "Whyte", "russell@example.com");

        var repository = CreateRepositoryWithMockedHttp(personJson, HttpStatusCode.OK);

        // Act
        var result = await repository.GetPersonByUsernameAsync(username);

        // Assert
        result.Should().NotBeNull();
        result!.UserName.Should().Be(username);
        result.FirstName.Should().Be("Russell");
        result.LastName.Should().Be("Whyte");
    }

    [Fact]
    public async Task GetPersonByUsernameAsync_Should_ReturnNull_When_UsernameNotFound()
    {
        // Arrange
        var repository = CreateRepositoryWithMockedHttp("Not Found", HttpStatusCode.NotFound);

        // Act
        var result = await repository.GetPersonByUsernameAsync("nonexistentuser");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPersonByUsernameAsync_Should_IncludeRelatedData_When_IncludeRelatedIsTrue()
    {
        // Arrange
        var personJson = CreateSinglePersonWithRelatedResponse(
            "russellwhyte", "Russell", "Whyte",
            new[] { ("scott", "Scott", "Ketchum") },
            new[] { (1, "Trip to Paris", "A great trip") });

        var repository = CreateRepositoryWithMockedHttp(personJson, HttpStatusCode.OK);

        // Act
        var result = await repository.GetPersonByUsernameAsync("russellwhyte", includeRelated: true);

        // Assert
        result.Should().NotBeNull();
        result!.Friends.Should().NotBeEmpty();
        result.Trips.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetPersonByUsernameAsync_Should_ThrowHttpRequestException_When_ServerError()
    {
        // Arrange
        var repository = CreateRepositoryWithMockedHttp("Internal Server Error", HttpStatusCode.InternalServerError);

        // Act
        Func<Task> act = async () => await repository.GetPersonByUsernameAsync("russellwhyte");

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetPersonByUsernameAsync_Should_ThrowException_When_UsernameInvalid(string? username)
    {
        // Arrange
        var repository = CreateRepositoryWithMockedHttp("{}", HttpStatusCode.OK);

        // Act
        Func<Task> act = async () => await repository.GetPersonByUsernameAsync(username!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    #endregion

    #region Helper Methods

    private ODataRepository CreateRepositoryWithMockedHttp(string responseContent, HttpStatusCode statusCode)
    {
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(responseContent)
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri(_settings.BaseUrl)
        };

        var options = Options.Create(_settings);
        return new ODataRepository(httpClient, options, _mockLogger.Object);
    }

    private static string CreatePersonJson(string userName, string firstName, string lastName, string? email = null)
    {
        return $@"{{
            ""UserName"": ""{userName}"",
            ""FirstName"": ""{firstName}"",
            ""LastName"": ""{lastName}"",
            ""Emails"": [{(email != null ? $"\"{email}\"" : "")}],
            ""Gender"": ""Male""
        }}";
    }

    private static string CreateODataPeopleResponse(string[] people, int totalCount)
    {
        var peopleJson = string.Join(",", people);
        return $@"{{
            ""@odata.context"": ""http://services.odata.org/TripPinRESTierService/(S(3mslpb2bc0k5ufk24olpghzx))/$metadata#People"",
            ""@odata.count"": {totalCount},
            ""value"": [{peopleJson}]
        }}";
    }

    private static string CreateODataSearchResponse(string[] people)
    {
        var peopleJson = string.Join(",", people);
        return $@"{{
            ""@odata.context"": ""http://services.odata.org/TripPinRESTierService/(S(3mslpb2bc0k5ufk24olpghzx))/$metadata#People"",
            ""value"": [{peopleJson}]
        }}";
    }

    private static string CreateSinglePersonResponse(string userName, string firstName, string lastName, string? email = null)
    {
        return $@"{{
            ""@odata.context"": ""http://services.odata.org/TripPinRESTierService/(S(3mslpb2bc0k5ufk24olpghzx))/$metadata#People/$entity"",
            ""UserName"": ""{userName}"",
            ""FirstName"": ""{firstName}"",
            ""LastName"": ""{lastName}"",
            ""Emails"": [{(email != null ? $"\"{email}\"" : "")}],
            ""Gender"": ""Male""
        }}";
    }

    private static string CreateSinglePersonWithRelatedResponse(
        string userName, string firstName, string lastName,
        (string userName, string firstName, string lastName)[] friends,
        (int tripId, string name, string description)[] trips)
    {
        var friendsJson = string.Join(",", friends.Select(f => $@"{{
            ""UserName"": ""{f.userName}"",
            ""FirstName"": ""{f.firstName}"",
            ""LastName"": ""{f.lastName}""
        }}"));

        var tripsJson = string.Join(",", trips.Select(t => $@"{{
            ""TripId"": {t.tripId},
            ""Name"": ""{t.name}"",
            ""Description"": ""{t.description}"",
            ""StartsAt"": ""2024-01-01T00:00:00Z"",
            ""EndsAt"": ""2024-01-05T00:00:00Z""
        }}"));

        return $@"{{
            ""@odata.context"": ""http://services.odata.org/TripPinRESTierService/(S(3mslpb2bc0k5ufk24olpghzx))/$metadata#People/$entity"",
            ""UserName"": ""{userName}"",
            ""FirstName"": ""{firstName}"",
            ""LastName"": ""{lastName}"",
            ""Emails"": [],
            ""Gender"": ""Male"",
            ""Friends"": [{friendsJson}],
            ""Trips"": [{tripsJson}]
        }}";
    }

    #endregion
}
