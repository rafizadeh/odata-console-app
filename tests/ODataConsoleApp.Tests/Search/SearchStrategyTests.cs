namespace ODataConsoleApp.Tests.Search;

using FluentAssertions;
using ODataConsoleApp.Infrastructure.Search;
using ODataConsoleApp.Interfaces;
using ODataConsoleApp.Models;
using Xunit;

public class SearchStrategyTests
{
    #region ExactMatchStrategy Tests

    public class ExactMatchStrategyTests
    {
        [Fact]
        public void Should_BuildFilterForFirstName_When_OnlyFirstNameProvided()
        {
            // Arrange
            var strategy = new ExactMatchStrategy();
            var criteria = new SearchCriteria { FirstName = "John" };

            // Act
            var result = strategy.BuildFilterQuery(criteria);

            // Assert
            result.Should().Be("FirstName eq 'John'");
        }

        [Fact]
        public void Should_PreserveInternalWhitespace_When_ValueHasSpacesInMiddle()
        {
            // Arrange
            var strategy = new ExactMatchStrategy();
            var criteria = new SearchCriteria { FirstName = "John Paul" };

            // Act
            var result = strategy.BuildFilterQuery(criteria);

            // Assert
            result.Should().Be("FirstName eq 'John Paul'");
        }

        [Fact]
        public void Should_EscapeSingleQuote_When_ValueContainsSingleQuote()
        {
            // Arrange
            var strategy = new ExactMatchStrategy();
            var criteria = new SearchCriteria { FirstName = "O'Brien" };

            // Act
            var result = strategy.BuildFilterQuery(criteria);

            // Assert
            result.Should().Be("FirstName eq 'O''Brien'");
        }

        [Fact]
        public void Should_EscapeMultipleSingleQuotes_When_ValueContainsMultipleSingleQuotes()
        {
            // Arrange
            var strategy = new ExactMatchStrategy();
            var criteria = new SearchCriteria { LastName = "O'Neil's" };

            // Act
            var result = strategy.BuildFilterQuery(criteria);

            // Assert
            result.Should().Be("LastName eq 'O''Neil''s'");
        }
    }

    #endregion

    #region ContainsStrategy Tests

    public class ContainsStrategyTests
    {
        [Fact]
        public void Should_BuildContainsFilterForFirstName_When_OnlyFirstNameProvided()
        {
            // Arrange
            var strategy = new ContainsStrategy();
            var criteria = new SearchCriteria { FirstName = "Jo" };

            // Act
            var result = strategy.BuildFilterQuery(criteria);

            // Assert
            result.Should().Be("contains(FirstName, 'Jo')");
        }

        [Fact]
        public void Should_PreserveCaseSensitivity_When_BuildingFilter()
        {
            // Arrange
            var strategy = new ContainsStrategy();
            var criteria = new SearchCriteria { FirstName = "JO", LastName = "do" };

            // Act
            var result = strategy.BuildFilterQuery(criteria);

            // Assert
            result.Should().Be("contains(FirstName, 'JO') or contains(LastName, 'do')");
        }

        [Fact]
        public void Should_EscapeSingleQuote_When_ValueContainsSingleQuote()
        {
            // Arrange
            var strategy = new ContainsStrategy();
            var criteria = new SearchCriteria { FirstName = "O'Br" };

            // Act
            var result = strategy.BuildFilterQuery(criteria);

            // Assert
            result.Should().Be("contains(FirstName, 'O''Br')");
        }

        [Fact]
        public void Should_HandleUnicodeCharacters_When_ValueContainsUnicode()
        {
            // Arrange
            var strategy = new ContainsStrategy();
            var criteria = new SearchCriteria { FirstName = "Jose", LastName = "Mull" };

            // Act
            var result = strategy.BuildFilterQuery(criteria);

            // Assert
            result.Should().Be("contains(FirstName, 'Jose') or contains(LastName, 'Mull')");
        }
    }

    #endregion

    #region StartsWithStrategy Tests

    public class StartsWithStrategyTests
    {
        [Fact]
        public void Should_BuildStartsWithFilterForFirstName_When_OnlyFirstNameProvided()
        {
            // Arrange
            var strategy = new StartsWithStrategy();
            var criteria = new SearchCriteria { FirstName = "Jo" };

            // Act
            var result = strategy.BuildFilterQuery(criteria);

            // Assert
            result.Should().Be("startswith(FirstName, 'Jo')");
        }

        [Fact]
        public void Should_CombineWithOr_When_MultipleFieldsProvided()
        {
            // Arrange
            var strategy = new StartsWithStrategy();
            var criteria = new SearchCriteria { FirstName = "Jo", LastName = "Do" };

            // Act
            var result = strategy.BuildFilterQuery(criteria);

            // Assert
            result.Should().Be("startswith(FirstName, 'Jo') or startswith(LastName, 'Do')");
        }

        [Fact]
        public void Should_EscapeSingleQuote_When_ValueContainsSingleQuote()
        {
            // Arrange
            var strategy = new StartsWithStrategy();
            var criteria = new SearchCriteria { FirstName = "O'Br" };

            // Act
            var result = strategy.BuildFilterQuery(criteria);

            // Assert
            result.Should().Be("startswith(FirstName, 'O''Br')");
        }
    }

    #endregion

    #region SearchStrategyFactory Tests

    public class SearchStrategyFactoryTests
    {
        [Fact]
        public void Should_ReturnExactMatchStrategy_When_SearchTypeIsExactMatch()
        {
            // Arrange
            var factory = new SearchStrategyFactory();

            // Act
            var strategy = factory.CreateStrategy(SearchType.ExactMatch);

            // Assert
            strategy.Should().NotBeNull();
            strategy.Should().BeOfType<ExactMatchStrategy>();
        }

        [Fact]
        public void Should_ReturnContainsStrategy_When_SearchTypeIsContains()
        {
            // Arrange
            var factory = new SearchStrategyFactory();

            // Act
            var strategy = factory.CreateStrategy(SearchType.Contains);

            // Assert
            strategy.Should().NotBeNull();
            strategy.Should().BeOfType<ContainsStrategy>();
        }

        [Fact]
        public void Should_ReturnStartsWithStrategy_When_SearchTypeIsStartsWith()
        {
            // Arrange
            var factory = new SearchStrategyFactory();

            // Act
            var strategy = factory.CreateStrategy(SearchType.StartsWith);

            // Assert
            strategy.Should().NotBeNull();
            strategy.Should().BeOfType<StartsWithStrategy>();
        }

        [Fact]
        public void Should_ThrowArgumentException_When_SearchTypeIsInvalid()
        {
            // Arrange
            var factory = new SearchStrategyFactory();
            var invalidSearchType = (SearchType)999;

            // Act
            Action act = () => factory.CreateStrategy(invalidSearchType);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*search type*")
                .And.ParamName.Should().Be("searchType");
        }
    }

    #endregion

    #region Integration and Edge Case Tests

    public class IntegrationAndEdgeCaseTests
    {
        [Fact]
        public void Should_ThrowArgumentNullException_When_CriteriaIsNull()
        {
            // Arrange
            var strategy = new ExactMatchStrategy();

            // Act
            Action act = () => strategy.BuildFilterQuery(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("criteria");
        }

        [Fact]
        public void Should_ReturnEmptyString_When_CriteriaIsEmpty()
        {
            // Arrange
            var strategy = new ExactMatchStrategy();
            var criteria = new SearchCriteria();

            // Act
            var result = strategy.BuildFilterQuery(criteria);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void Should_ReturnEmptyString_When_AllFieldsAreNull()
        {
            // Arrange
            var strategy = new StartsWithStrategy();
            var criteria = new SearchCriteria
            {
                FirstName = null,
                LastName = null,
                UserName = null
            };

            // Act
            var result = strategy.BuildFilterQuery(criteria);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void Should_HandleNullFieldsCorrectly_When_SomeFieldsAreNull()
        {
            // Arrange
            var strategy = new ContainsStrategy();
            var criteria = new SearchCriteria
            {
                FirstName = "John",
                LastName = null,
                UserName = null
            };

            // Act
            var result = strategy.BuildFilterQuery(criteria);

            // Assert
            result.Should().Be("contains(FirstName, 'John')");
        }

        [Fact]
        public void Should_ThrowArgumentException_When_ValueContainsControlCharacters()
        {
            // Arrange
            var strategy = new ExactMatchStrategy();
            var valueWithNull = "John" + '\x00' + "Doe";
            var criteria = new SearchCriteria { FirstName = valueWithNull };

            // Act
            Action act = () => strategy.BuildFilterQuery(criteria);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*control characters*");
        }

        [Theory]
        [InlineData(SearchType.ExactMatch, "John", "FirstName eq 'John'")]
        [InlineData(SearchType.Contains, "John", "contains(FirstName, 'John')")]
        [InlineData(SearchType.StartsWith, "John", "startswith(FirstName, 'John')")]
        public void Should_ProduceCorrectQuery_When_UsingFactoryWithDifferentSearchTypes(
            SearchType searchType,
            string firstName,
            string expectedQuery)
        {
            // Arrange
            var factory = new SearchStrategyFactory();
            var strategy = factory.CreateStrategy(searchType);
            var criteria = new SearchCriteria
            {
                FirstName = firstName,
                SearchType = searchType
            };

            // Act
            var result = strategy.BuildFilterQuery(criteria);

            // Assert
            result.Should().Be(expectedQuery);
        }
    }

    #endregion
}
