namespace ODataConsoleApp.Application.Services;

using Microsoft.Extensions.Logging;
using ODataConsoleApp.Infrastructure.Search;
using ODataConsoleApp.Interfaces;
using ODataConsoleApp.Models;

public class PersonService : IPersonService
{
    private const int MaxPageSize = 100;

    private readonly IODataRepository _repository;
    private readonly SearchStrategyFactory _strategyFactory;
    private readonly ILogger<PersonService> _logger;
    private readonly IFilterStrategy _filterStrategy;

    public PersonService(
        IODataRepository repository,
        SearchStrategyFactory strategyFactory,
        ILogger<PersonService> logger,
        IFilterStrategy filterStrategy)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _strategyFactory = strategyFactory ?? throw new ArgumentNullException(nameof(strategyFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _filterStrategy = filterStrategy ?? throw new ArgumentNullException(nameof(filterStrategy));
    }

    public async Task<PaginatedResult<Person>> GetPeopleAsync(int page, int pageSize)
    {
        if (page < 1)
        {
            throw new ArgumentException("Page number must be greater than 0.", nameof(page));
        }

        if (pageSize < 1)
        {
            throw new ArgumentException("Page size must be greater than 0.", nameof(pageSize));
        }

        if (pageSize > MaxPageSize)
        {
            throw new ArgumentException($"Page size cannot exceed {MaxPageSize}.", nameof(pageSize));
        }

        try
        {
            _logger.LogInformation("Fetching people: page={Page}, pageSize={PageSize}", page, pageSize);

            var skip = (page - 1) * pageSize;

            var result = await _repository.GetPeopleAsync(skip, pageSize);

            _logger.LogInformation("Successfully fetched {Count} people (page {Page} of {TotalPages})",
                result.Items?.Count() ?? 0, result.CurrentPage, result.TotalPages);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching people: page={Page}, pageSize={PageSize}", page, pageSize);
            throw;
        }
    }

    public async Task<IEnumerable<Person>> SearchPeopleAsync(SearchCriteria criteria)
    {
        if (criteria == null)
        {
            throw new ArgumentNullException(nameof(criteria));
        }

        if (criteria.IsEmpty())
        {
            throw new ArgumentException("Search criteria must contain at least one search field.", nameof(criteria));
        }

        try
        {
            _logger.LogInformation("Searching people with criteria: SearchType={SearchType}, FirstName={FirstName}, LastName={LastName}, UserName={UserName}",
                criteria.SearchType, criteria.FirstName, criteria.LastName, criteria.UserName);

            var strategy = _strategyFactory.CreateStrategy(criteria.SearchType);

            var filterQuery = strategy.BuildFilterQuery(criteria);

            _logger.LogInformation("Built filter query: {FilterQuery}", filterQuery);

            var results = await _repository.SearchPeopleAsync(filterQuery);

            _logger.LogInformation("Search completed: found {Count} matching people", results.Count());

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching people with criteria: SearchType={SearchType}", criteria.SearchType);
            throw;
        }
    }

    public async Task<Person?> GetPersonDetailsAsync(string username)
    {
        if (username == null)
        {
            throw new ArgumentNullException(nameof(username));
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Username cannot be empty or whitespace.", nameof(username));
        }

        try
        {
            _logger.LogInformation("Fetching person details for username: {Username}", username);

            var person = await _repository.GetPersonByUsernameAsync(username, includeRelated: true);

            if (person != null)
            {
                _logger.LogInformation("Successfully retrieved details for user: {Username} (Friends: {FriendsCount}, Trips: {TripsCount})",
                    username, person.Friends?.Count ?? 0, person.Trips?.Count ?? 0);
            }
            else
            {
                _logger.LogInformation("Person not found: {Username}", username);
            }

            return person;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching person details for username: {Username}", username);
            throw;
        }
    }

    public async Task<IEnumerable<Person>> FilterPeopleAsync(FilterCriteria criteria)
    {
        if (criteria == null)
        {
            throw new ArgumentNullException(nameof(criteria));
        }

        if (criteria.IsEmpty())
        {
            throw new ArgumentException("Filter criteria must contain at least one filter.", nameof(criteria));
        }

        try
        {
            _logger.LogInformation("Filtering people with {FilterCount} filters", criteria.Filters?.Count ?? 0);

            var filterQuery = _filterStrategy.BuildFilterQuery(criteria);

            _logger.LogInformation("Built filter query: {FilterQuery}", filterQuery);

            var results = await _repository.SearchPeopleAsync(filterQuery);

            _logger.LogInformation("Filter completed: found {Count} matching people", results.Count());

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering people with {FilterCount} filters", criteria.Filters?.Count ?? 0);
            throw;
        }
    }
}
