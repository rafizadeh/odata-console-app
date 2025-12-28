namespace ODataConsoleApp.Tests.Services;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ODataConsoleApp.Application.Services;
using ODataConsoleApp.Infrastructure.Search;
using ODataConsoleApp.Interfaces;
using ODataConsoleApp.Models;
using Xunit;

public class PersonServiceTests
{
    #region GetPeopleAsync Tests

    public class GetPeopleAsyncTests
    {
        private readonly Mock<IODataRepository> _mockRepository;
        private readonly SearchStrategyFactory _strategyFactory;
        private readonly Mock<ILogger<PersonService>> _mockLogger;
        private readonly Mock<IFilterStrategy> _mockFilterStrategy;

        public GetPeopleAsyncTests()
        {
            _mockRepository = new Mock<IODataRepository>();
            _strategyFactory = new SearchStrategyFactory();
            _mockLogger = new Mock<ILogger<PersonService>>();
            _mockFilterStrategy = new Mock<IFilterStrategy>();
        }

        [Fact]
        public async Task Should_ReturnPaginatedResult_When_ValidPageAndPageSizeProvided()
        {
            // Arrange
            var expectedPeople = new List<Person>
            {
                new Person { UserName = "user1", FirstName = "John", LastName = "Doe" },
                new Person { UserName = "user2", FirstName = "Jane", LastName = "Smith" }
            };

            var expectedResult = new PaginatedResult<Person>
            {
                Items = expectedPeople,
                TotalCount = 2,
                CurrentPage = 1,
                PageSize = 10
            };

            _mockRepository
                .Setup(r => r.GetPeopleAsync(0, 10))
                .ReturnsAsync(expectedResult);

            var service = new PersonService(_mockRepository.Object, _strategyFactory, _mockLogger.Object, _mockFilterStrategy.Object);

            // Act
            var result = await service.GetPeopleAsync(page: 1, pageSize: 10);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.Items.Should().BeEquivalentTo(expectedPeople);
            result.CurrentPage.Should().Be(1);
            result.PageSize.Should().Be(10);
            result.TotalCount.Should().Be(2);
        }

        [Theory]
        [InlineData(1, 10, 0)]   // Page 1 = skip 0
        [InlineData(2, 10, 10)]  // Page 2 = skip 10
        [InlineData(3, 10, 20)]  // Page 3 = skip 20
        [InlineData(5, 20, 80)]  // Page 5, size 20 = skip 80
        public async Task Should_CalculateCorrectSkipValue_When_PageAndPageSizeProvided(int page, int pageSize, int expectedSkip)
        {
            // Arrange
            var expectedResult = new PaginatedResult<Person>
            {
                Items = new List<Person>(),
                TotalCount = 100,
                CurrentPage = page,
                PageSize = pageSize
            };

            _mockRepository
                .Setup(r => r.GetPeopleAsync(expectedSkip, pageSize))
                .ReturnsAsync(expectedResult);

            var service = new PersonService(_mockRepository.Object, _strategyFactory, _mockLogger.Object, _mockFilterStrategy.Object);

            // Act
            var result = await service.GetPeopleAsync(page, pageSize);

            // Assert
            _mockRepository.Verify(r => r.GetPeopleAsync(expectedSkip, pageSize), Times.Once);
            result.CurrentPage.Should().Be(page);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Should_ThrowArgumentException_When_PageIsLessThanOne(int invalidPage)
        {
            // Arrange
            var service = new PersonService(_mockRepository.Object, _strategyFactory, _mockLogger.Object, _mockFilterStrategy.Object);

            // Act
            Func<Task> act = async () => await service.GetPeopleAsync(invalidPage, 10);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*page*")
                .WithMessage("*must be greater than 0*");
        }

        [Fact]
        public async Task Should_ReturnEmptyResult_When_NoDataExists()
        {
            // Arrange
            var expectedResult = new PaginatedResult<Person>
            {
                Items = new List<Person>(),
                TotalCount = 0,
                CurrentPage = 1,
                PageSize = 10
            };

            _mockRepository
                .Setup(r => r.GetPeopleAsync(0, 10))
                .ReturnsAsync(expectedResult);

            var service = new PersonService(_mockRepository.Object, _strategyFactory, _mockLogger.Object, _mockFilterStrategy.Object);

            // Act
            var result = await service.GetPeopleAsync(1, 10);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Should_PropagateException_When_RepositoryThrows()
        {
            // Arrange
            _mockRepository
                .Setup(r => r.GetPeopleAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new HttpRequestException("Service unavailable"));

            var service = new PersonService(_mockRepository.Object, _strategyFactory, _mockLogger.Object, _mockFilterStrategy.Object);

            // Act
            Func<Task> act = async () => await service.GetPeopleAsync(1, 10);

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>()
                .WithMessage("Service unavailable");
        }
    }

    #endregion

    #region SearchPeopleAsync Tests

    public class SearchPeopleAsyncTests
    {
        private readonly Mock<IODataRepository> _mockRepository;
        private readonly SearchStrategyFactory _strategyFactory;
        private readonly Mock<ILogger<PersonService>> _mockLogger;
        private readonly Mock<IFilterStrategy> _mockFilterStrategy;

        public SearchPeopleAsyncTests()
        {
            _mockRepository = new Mock<IODataRepository>();
            _strategyFactory = new SearchStrategyFactory();
            _mockLogger = new Mock<ILogger<PersonService>>();
            _mockFilterStrategy = new Mock<IFilterStrategy>();
        }

        [Fact]
        public async Task Should_ReturnSearchResults_When_ValidCriteriaProvided()
        {
            // Arrange
            var criteria = new SearchCriteria
            {
                FirstName = "John",
                SearchType = SearchType.ExactMatch
            };

            var expectedPeople = new List<Person>
            {
                new Person { UserName = "johndoe", FirstName = "John", LastName = "Doe" }
            };

            _mockRepository
                .Setup(r => r.SearchPeopleAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedPeople);

            var service = new PersonService(_mockRepository.Object, _strategyFactory, _mockLogger.Object, _mockFilterStrategy.Object);

            // Act
            var result = await service.SearchPeopleAsync(criteria);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.First().FirstName.Should().Be("John");
        }

        [Theory]
        [InlineData(SearchType.ExactMatch)]
        [InlineData(SearchType.Contains)]
        [InlineData(SearchType.StartsWith)]
        public async Task Should_UseCorrectStrategy_When_SearchTypeProvided(SearchType searchType)
        {
            // Arrange
            var criteria = new SearchCriteria
            {
                FirstName = "John",
                SearchType = searchType
            };

            _mockRepository
                .Setup(r => r.SearchPeopleAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<Person>());

            var service = new PersonService(_mockRepository.Object, _strategyFactory, _mockLogger.Object, _mockFilterStrategy.Object);

            // Act
            await service.SearchPeopleAsync(criteria);

            // Assert
            _mockRepository.Verify(r => r.SearchPeopleAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Should_ThrowArgumentNullException_When_CriteriaIsNull()
        {
            // Arrange
            var service = new PersonService(_mockRepository.Object, _strategyFactory, _mockLogger.Object, _mockFilterStrategy.Object);

            // Act
            Func<Task> act = async () => await service.SearchPeopleAsync(null!);

            // Assert
            var exception = await act.Should().ThrowAsync<ArgumentNullException>();
            exception.And.ParamName.Should().Be("criteria");
        }

        [Fact]
        public async Task Should_ThrowArgumentException_When_CriteriaIsEmpty()
        {
            // Arrange
            var emptyCriteria = new SearchCriteria();

            var service = new PersonService(_mockRepository.Object, _strategyFactory, _mockLogger.Object, _mockFilterStrategy.Object);

            // Act
            Func<Task> act = async () => await service.SearchPeopleAsync(emptyCriteria);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*criteria*")
                .WithMessage("*at least one search field*");
        }

        [Fact]
        public async Task Should_ReturnEmptyCollection_When_NoResultsFound()
        {
            // Arrange
            var criteria = new SearchCriteria
            {
                FirstName = "NonExistent",
                SearchType = SearchType.ExactMatch
            };

            _mockRepository
                .Setup(r => r.SearchPeopleAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<Person>());

            var service = new PersonService(_mockRepository.Object, _strategyFactory, _mockLogger.Object, _mockFilterStrategy.Object);

            // Act
            var result = await service.SearchPeopleAsync(criteria);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Should_PropagateException_When_RepositoryFails()
        {
            // Arrange
            var criteria = new SearchCriteria
            {
                FirstName = "John",
                SearchType = SearchType.ExactMatch
            };

            _mockRepository
                .Setup(r => r.SearchPeopleAsync(It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("Service unavailable"));

            var service = new PersonService(_mockRepository.Object, _strategyFactory, _mockLogger.Object, _mockFilterStrategy.Object);

            // Act
            Func<Task> act = async () => await service.SearchPeopleAsync(criteria);

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>()
                .WithMessage("Service unavailable");
        }
    }

    #endregion

    #region GetPersonDetailsAsync Tests

    public class GetPersonDetailsAsyncTests
    {
        private readonly Mock<IODataRepository> _mockRepository;
        private readonly SearchStrategyFactory _strategyFactory;
        private readonly Mock<ILogger<PersonService>> _mockLogger;
        private readonly Mock<IFilterStrategy> _mockFilterStrategy;

        public GetPersonDetailsAsyncTests()
        {
            _mockRepository = new Mock<IODataRepository>();
            _strategyFactory = new SearchStrategyFactory();
            _mockLogger = new Mock<ILogger<PersonService>>();
            _mockFilterStrategy = new Mock<IFilterStrategy>();
        }

        [Fact]
        public async Task Should_ReturnPersonWithRelatedData_When_UsernameExists()
        {
            // Arrange
            var username = "johndoe";
            var expectedPerson = new Person
            {
                UserName = username,
                FirstName = "John",
                LastName = "Doe",
                Emails = new List<string> { "john.doe@example.com" }
            };

            _mockRepository
                .Setup(r => r.GetPersonByUsernameAsync(username, true))
                .ReturnsAsync(expectedPerson);

            var service = new PersonService(_mockRepository.Object, _strategyFactory, _mockLogger.Object, _mockFilterStrategy.Object);

            // Act
            var result = await service.GetPersonDetailsAsync(username);

            // Assert
            result.Should().NotBeNull();
            result!.UserName.Should().Be(username);
            result.FirstName.Should().Be("John");
            result.LastName.Should().Be("Doe");
        }

        [Fact]
        public async Task Should_ReturnPersonWithFriendsAndTrips_When_RelatedDataExists()
        {
            // Arrange
            var username = "johndoe";
            var expectedPerson = new Person
            {
                UserName = username,
                FirstName = "John",
                Friends = new List<Friend>
                {
                    new Friend { UserName = "janedoe", FirstName = "Jane" }
                },
                Trips = new List<Trip>
                {
                    new Trip { TripId = 1, Name = "Trip to Paris" }
                }
            };

            _mockRepository
                .Setup(r => r.GetPersonByUsernameAsync(username, true))
                .ReturnsAsync(expectedPerson);

            var service = new PersonService(_mockRepository.Object, _strategyFactory, _mockLogger.Object, _mockFilterStrategy.Object);

            // Act
            var result = await service.GetPersonDetailsAsync(username);

            // Assert
            result.Should().NotBeNull();
            result!.Friends.Should().HaveCount(1);
            result.Trips.Should().HaveCount(1);
        }

        [Fact]
        public async Task Should_ReturnNull_When_UsernameNotFound()
        {
            // Arrange
            var username = "nonexistent";

            _mockRepository
                .Setup(r => r.GetPersonByUsernameAsync(username, true))
                .ReturnsAsync((Person?)null);

            var service = new PersonService(_mockRepository.Object, _strategyFactory, _mockLogger.Object, _mockFilterStrategy.Object);

            // Act
            var result = await service.GetPersonDetailsAsync(username);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task Should_ThrowArgumentException_When_UsernameIsNullOrEmpty()
        {
            // Arrange
            var service = new PersonService(_mockRepository.Object, _strategyFactory, _mockLogger.Object, _mockFilterStrategy.Object);

            // Act & Assert - null
            Func<Task> actNull = async () => await service.GetPersonDetailsAsync(null!);
            await actNull.Should().ThrowAsync<ArgumentNullException>();

            // Act & Assert - empty
            Func<Task> actEmpty = async () => await service.GetPersonDetailsAsync("");
            await actEmpty.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*username*")
                .WithMessage("*cannot be empty*");
        }

        [Fact]
        public async Task Should_PropagateException_When_RepositoryThrows()
        {
            // Arrange
            var username = "johndoe";

            _mockRepository
                .Setup(r => r.GetPersonByUsernameAsync(username, true))
                .ThrowsAsync(new HttpRequestException("Service unavailable"));

            var service = new PersonService(_mockRepository.Object, _strategyFactory, _mockLogger.Object, _mockFilterStrategy.Object);

            // Act
            Func<Task> act = async () => await service.GetPersonDetailsAsync(username);

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>()
                .WithMessage("Service unavailable");
        }
    }

    #endregion

    #region FilterPeopleAsync Tests

    public class FilterPeopleAsyncTests
    {
        private readonly Mock<IODataRepository> _mockRepository;
        private readonly SearchStrategyFactory _strategyFactory;
        private readonly Mock<ILogger<PersonService>> _mockLogger;
        private readonly Mock<IFilterStrategy> _mockFilterStrategy;

        public FilterPeopleAsyncTests()
        {
            _mockRepository = new Mock<IODataRepository>();
            _strategyFactory = new SearchStrategyFactory();
            _mockLogger = new Mock<ILogger<PersonService>>();
            _mockFilterStrategy = new Mock<IFilterStrategy>();
        }

        [Fact]
        public async Task Should_ReturnFilteredPeople_When_SingleFilterProvided()
        {
            // Arrange
            var criteria = new FilterCriteria
            {
                Filters = new List<FieldFilter>
                {
                    new FieldFilter { FieldName = "FirstName", Value = "John", SearchType = SearchType.ExactMatch }
                }
            };

            var expectedFilter = "FirstName eq 'John'";
            var expectedPeople = new List<Person>
            {
                new Person { UserName = "johndoe", FirstName = "John", LastName = "Doe" }
            };

            _mockFilterStrategy
                .Setup(s => s.BuildFilterQuery(criteria))
                .Returns(expectedFilter);

            _mockRepository
                .Setup(r => r.SearchPeopleAsync(expectedFilter))
                .ReturnsAsync(expectedPeople);

            var service = new PersonService(_mockRepository.Object, _strategyFactory, _mockLogger.Object, _mockFilterStrategy.Object);

            // Act
            var result = await service.FilterPeopleAsync(criteria);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.First().FirstName.Should().Be("John");
            _mockFilterStrategy.Verify(s => s.BuildFilterQuery(criteria), Times.Once);
        }

        [Fact]
        public async Task Should_ReturnFilteredPeople_When_MultipleFiltersProvided()
        {
            // Arrange
            var criteria = new FilterCriteria
            {
                Filters = new List<FieldFilter>
                {
                    new FieldFilter { FieldName = "FirstName", Value = "John", SearchType = SearchType.Contains },
                    new FieldFilter { FieldName = "LastName", Value = "Doe", SearchType = SearchType.StartsWith }
                }
            };

            var expectedFilter = "contains(FirstName, 'John') and startswith(LastName, 'Doe')";
            var expectedPeople = new List<Person>
            {
                new Person { UserName = "johndoe", FirstName = "John", LastName = "Doe" },
                new Person { UserName = "johndoe2", FirstName = "Johnny", LastName = "Doeson" }
            };

            _mockFilterStrategy
                .Setup(s => s.BuildFilterQuery(criteria))
                .Returns(expectedFilter);

            _mockRepository
                .Setup(r => r.SearchPeopleAsync(expectedFilter))
                .ReturnsAsync(expectedPeople);

            var service = new PersonService(_mockRepository.Object, _strategyFactory, _mockLogger.Object, _mockFilterStrategy.Object);

            // Act
            var result = await service.FilterPeopleAsync(criteria);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            _mockFilterStrategy.Verify(s => s.BuildFilterQuery(criteria), Times.Once);
        }

        [Fact]
        public async Task Should_ThrowArgumentNullException_When_CriteriaIsNull()
        {
            // Arrange
            var service = new PersonService(_mockRepository.Object, _strategyFactory, _mockLogger.Object, _mockFilterStrategy.Object);

            // Act
            Func<Task> act = async () => await service.FilterPeopleAsync(null!);

            // Assert
            var exception = await act.Should().ThrowAsync<ArgumentNullException>();
            exception.And.ParamName.Should().Be("criteria");
        }

        [Fact]
        public async Task Should_ReturnEmptyCollection_When_NoResultsFound()
        {
            // Arrange
            var criteria = new FilterCriteria
            {
                Filters = new List<FieldFilter>
                {
                    new FieldFilter { FieldName = "FirstName", Value = "NonExistent", SearchType = SearchType.ExactMatch }
                }
            };

            _mockFilterStrategy
                .Setup(s => s.BuildFilterQuery(criteria))
                .Returns("FirstName eq 'NonExistent'");

            _mockRepository
                .Setup(r => r.SearchPeopleAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<Person>());

            var service = new PersonService(_mockRepository.Object, _strategyFactory, _mockLogger.Object, _mockFilterStrategy.Object);

            // Act
            var result = await service.FilterPeopleAsync(criteria);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }
    }

    #endregion

    #region Constructor Tests

    public class ConstructorTests
    {
        private readonly Mock<IODataRepository> _mockRepository;
        private readonly SearchStrategyFactory _strategyFactory;
        private readonly Mock<ILogger<PersonService>> _mockLogger;
        private readonly Mock<IFilterStrategy> _mockFilterStrategy;

        public ConstructorTests()
        {
            _mockRepository = new Mock<IODataRepository>();
            _strategyFactory = new SearchStrategyFactory();
            _mockLogger = new Mock<ILogger<PersonService>>();
            _mockFilterStrategy = new Mock<IFilterStrategy>();
        }

        [Fact]
        public void Should_ThrowArgumentNullException_When_RepositoryIsNull()
        {
            // Act
            Action act = () => new PersonService(null!, _strategyFactory, _mockLogger.Object, _mockFilterStrategy.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("repository");
        }

        [Fact]
        public void Should_ThrowArgumentNullException_When_StrategyFactoryIsNull()
        {
            // Act
            Action act = () => new PersonService(_mockRepository.Object, null!, _mockLogger.Object, _mockFilterStrategy.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("strategyFactory");
        }
    }

    #endregion
}