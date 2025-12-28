namespace ODataConsoleApp.Infrastructure.Repositories;

using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ODataConsoleApp.Core.Configuration;
using ODataConsoleApp.Interfaces;
using ODataConsoleApp.Models;

public class ODataRepository : IODataRepository
{
    private const int FirstPageNumber = 1;
    private const int MaxFilterQueryLength = 500;

    private readonly HttpClient _httpClient;
    private readonly ODataServiceSettings _settings;
    private readonly ILogger<ODataRepository> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ODataRepository(
        HttpClient httpClient,
        IOptions<ODataServiceSettings> settings,
        ILogger<ODataRepository> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<PaginatedResult<Person>> GetPeopleAsync(int skip, int top)
    {
        if (skip < 0)
        {
            throw new ArgumentException("Skip parameter cannot be negative.", nameof(skip));
        }

        if (top <= 0)
        {
            throw new ArgumentException("Top parameter must be greater than zero.", nameof(top));
        }

        var url = $"People?$skip={skip}&$top={top}&$count=true";

        _logger.LogInformation("GetPeopleAsync: Fetching people with skip={Skip}, top={Top}", skip, top);

        using var response = await ExecuteRequestAsync(url, "GetPeopleAsync");
        var odataResponse = await DeserializeResponseAsync<ODataCollectionResponse<Person>>(response, "GetPeopleAsync");

        var items = (odataResponse.Value ?? Enumerable.Empty<Person>()).ToList();
        var currentPage = top > 0 ? (skip / top) + FirstPageNumber : FirstPageNumber;

        var result = new PaginatedResult<Person>
        {
            Items = items,
            TotalCount = odataResponse.Count,
            CurrentPage = currentPage,
            PageSize = top
        };

        _logger.LogInformation("GetPeopleAsync: Successfully fetched {Count} people. Page {Page} of {TotalPages}",
            items.Count, result.CurrentPage, result.TotalPages);

        return result;
    }

    public async Task<IEnumerable<Person>> SearchPeopleAsync(string filterQuery)
    {
        ArgumentNullException.ThrowIfNull(filterQuery);

        if (filterQuery.Length > MaxFilterQueryLength)
        {
            throw new ArgumentException(
                $"Filter query exceeds maximum length of {MaxFilterQueryLength} characters.",
                nameof(filterQuery));
        }

        var url = string.IsNullOrEmpty(filterQuery)
            ? "People"
            : $"People?$filter={Uri.EscapeDataString(filterQuery)}";

        _logger.LogInformation("SearchPeopleAsync: Searching people with filter: {Filter}", filterQuery);

        using var response = await ExecuteRequestAsync(url, "SearchPeopleAsync");
        var odataResponse = await DeserializeResponseAsync<ODataCollectionResponse<Person>>(response, "SearchPeopleAsync");

        var result = (odataResponse.Value ?? Enumerable.Empty<Person>()).ToList();

        _logger.LogInformation("SearchPeopleAsync: Search returned {Count} people", result.Count);

        return result;
    }

    public async Task<Person?> GetPersonByUsernameAsync(string username, bool includeRelated = false)
    {
        ArgumentNullException.ThrowIfNull(username);

        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Username cannot be empty or whitespace.", nameof(username));
        }

        var encodedUsername = Uri.EscapeDataString(username);
        var url = $"People('{encodedUsername}')";

        if (includeRelated)
        {
            url += "?$expand=Friends,Trips";
        }

        _logger.LogInformation("GetPersonByUsernameAsync: Fetching person with username: {Username}, includeRelated: {IncludeRelated}",
            username, includeRelated);

        using var response = await ExecuteRequestAsync(url, "GetPersonByUsernameAsync", allowNotFound: true);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogInformation("GetPersonByUsernameAsync: Person with username {Username} not found", username);
            return null;
        }

        var person = await DeserializeResponseAsync<Person>(response, "GetPersonByUsernameAsync");

        _logger.LogInformation("GetPersonByUsernameAsync: Successfully fetched person: {Username}", username);

        return person;
    }

    #region Private Helper Methods

    private async Task<HttpResponseMessage> ExecuteRequestAsync(
        string url,
        string operationName,
        bool allowNotFound = false)
    {
        try
        {
            var response = await _httpClient.GetAsync(url);

            if (allowNotFound && response.StatusCode == HttpStatusCode.NotFound)
            {
                return response;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("{Operation}: Request failed with status code: {StatusCode}",
                    operationName, response.StatusCode);

                var statusCodeInt = (int)response.StatusCode;
                var errorMessage = statusCodeInt >= 500
                    ? $"OData request failed - Server error: HTTP {statusCodeInt} ({response.StatusCode})"
                    : $"OData request failed - Client error: HTTP {statusCodeInt} ({response.StatusCode})";

                response.Dispose();
                throw new HttpRequestException(errorMessage);
            }

            return response;
        }
        catch (HttpRequestException ex) when (!ex.Message.StartsWith("OData request failed"))
        {
            _logger.LogError(ex, "{Operation}: HTTP request failed", operationName);
            throw;
        }
    }

    private async Task<T> DeserializeResponseAsync<T>(
        HttpResponseMessage response,
        string operationName) where T : class
    {
        try
        {
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<T>(content, _jsonOptions);

            if (result == null)
            {
                throw new JsonException($"Failed to deserialize {typeof(T).Name} - result was null");
            }

            return result;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "{Operation}: Failed to parse JSON response", operationName);
            throw;
        }
    }

    #endregion

    #region Private DTOs

    private class ODataCollectionResponse<T>
    {
        public IEnumerable<T>? Value { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("@odata.count")]
        public long Count { get; set; }
    }

    #endregion
}