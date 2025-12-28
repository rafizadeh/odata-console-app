using FluentAssertions;
using ODataConsoleApp.Infrastructure.Search;
using ODataConsoleApp.Interfaces;
using ODataConsoleApp.Models;

namespace ODataConsoleApp.Tests.Search;

public class CompositeFilterStrategyTests
{
    [Fact]
    public void Should_BuildSingleFilter_When_OneFieldWithExactMatch()
    {
        // Arrange
        var strategy = new CompositeFilterStrategy();
        var criteria = new FilterCriteria();
        criteria.Filters.Add(new FieldFilter
        {
            FieldName = "FirstName",
            Value = "John",
            SearchType = SearchType.ExactMatch
        });

        // Act
        var result = strategy.BuildFilterQuery(criteria);

        // Assert
        result.Should().Be("FirstName eq 'John'");
    }

    [Fact]
    public void Should_BuildSingleFilter_When_OneFieldWithContains()
    {
        // Arrange
        var strategy = new CompositeFilterStrategy();
        var criteria = new FilterCriteria();
        criteria.Filters.Add(new FieldFilter
        {
            FieldName = "LastName",
            Value = "Do",
            SearchType = SearchType.Contains
        });

        // Act
        var result = strategy.BuildFilterQuery(criteria);

        // Assert
        result.Should().Be("contains(LastName, 'Do')");
    }

    [Fact]
    public void Should_BuildSingleFilter_When_OneFieldWithStartsWith()
    {
        // Arrange
        var strategy = new CompositeFilterStrategy();
        var criteria = new FilterCriteria();
        criteria.Filters.Add(new FieldFilter
        {
            FieldName = "UserName",
            Value = "john",
            SearchType = SearchType.StartsWith
        });

        // Act
        var result = strategy.BuildFilterQuery(criteria);

        // Assert
        result.Should().Be("startswith(UserName, 'john')");
    }

    [Fact]
    public void Should_CombineWithAnd_When_MultipleFiltersProvided()
    {
        // Arrange
        var strategy = new CompositeFilterStrategy();
        var criteria = new FilterCriteria();
        criteria.Filters.Add(new FieldFilter
        {
            FieldName = "FirstName",
            Value = "John",
            SearchType = SearchType.ExactMatch
        });
        criteria.Filters.Add(new FieldFilter
        {
            FieldName = "LastName",
            Value = "Doe",
            SearchType = SearchType.ExactMatch
        });

        // Act
        var result = strategy.BuildFilterQuery(criteria);

        // Assert
        result.Should().Be("FirstName eq 'John' and LastName eq 'Doe'");
    }

    [Fact]
    public void Should_SupportDifferentSearchTypes_When_MultipleFilters()
    {
        // Arrange
        var strategy = new CompositeFilterStrategy();
        var criteria = new FilterCriteria();
        criteria.Filters.Add(new FieldFilter
        {
            FieldName = "FirstName",
            Value = "John",
            SearchType = SearchType.ExactMatch
        });
        criteria.Filters.Add(new FieldFilter
        {
            FieldName = "LastName",
            Value = "Do",
            SearchType = SearchType.Contains
        });
        criteria.Filters.Add(new FieldFilter
        {
            FieldName = "UserName",
            Value = "john",
            SearchType = SearchType.StartsWith
        });

        // Act
        var result = strategy.BuildFilterQuery(criteria);

        // Assert
        result.Should().Be("FirstName eq 'John' and contains(LastName, 'Do') and startswith(UserName, 'john')");
    }

    [Fact]
    public void Should_BuildNestedFilter_When_CityFieldProvided()
    {
        // Arrange
        var strategy = new CompositeFilterStrategy();
        var criteria = new FilterCriteria();
        criteria.Filters.Add(new FieldFilter
        {
            FieldName = "City",
            Value = "Seattle",
            SearchType = SearchType.Contains
        });

        // Act
        var result = strategy.BuildFilterQuery(criteria);

        // Assert
        result.Should().Be("AddressInfo/any(a: contains(a/City/Name, 'Seattle'))");
    }

    [Fact]
    public void Should_CombineSimpleAndNestedFilters_When_MixedFiltersProvided()
    {
        // Arrange
        var strategy = new CompositeFilterStrategy();
        var criteria = new FilterCriteria();
        criteria.Filters.Add(new FieldFilter
        {
            FieldName = "FirstName",
            Value = "John",
            SearchType = SearchType.ExactMatch
        });
        criteria.Filters.Add(new FieldFilter
        {
            FieldName = "City",
            Value = "Seattle",
            SearchType = SearchType.Contains
        });

        // Act
        var result = strategy.BuildFilterQuery(criteria);

        // Assert
        result.Should().Be("FirstName eq 'John' and AddressInfo/any(a: contains(a/City/Name, 'Seattle'))");
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_CriteriaIsNull()
    {
        // Arrange
        var strategy = new CompositeFilterStrategy();

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
        var strategy = new CompositeFilterStrategy();
        var criteria = new FilterCriteria();

        // Act
        var result = strategy.BuildFilterQuery(criteria);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Should_EscapeSingleQuotes_When_ValueContainsSingleQuote()
    {
        // Arrange
        var strategy = new CompositeFilterStrategy();
        var criteria = new FilterCriteria();
        criteria.Filters.Add(new FieldFilter
        {
            FieldName = "FirstName",
            Value = "O'Brien",
            SearchType = SearchType.ExactMatch
        });

        // Act
        var result = strategy.BuildFilterQuery(criteria);

        // Assert
        result.Should().Be("FirstName eq 'O''Brien'");
    }
}
