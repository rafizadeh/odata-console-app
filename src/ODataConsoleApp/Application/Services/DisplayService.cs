namespace ODataConsoleApp.Application.Services;

using System.Globalization;
using ODataConsoleApp.Interfaces;
using ODataConsoleApp.Models;

public class DisplayService : IDisplayService
{
    private const int BoxWidth = 59;
    private const int PersonBoxWidth = 58;

    public void DisplayPeopleList(PaginatedResult<Person> people)
    {
        ArgumentNullException.ThrowIfNull(people);

        Console.WriteLine(DrawDoubleLine(BoxWidth));
        Console.WriteLine(CenterText($"People (Page {people.CurrentPage} of {people.TotalPages})", BoxWidth));
        Console.WriteLine(DrawDoubleLine(BoxWidth));
        Console.WriteLine();

        if (people.Items != null)
        {
            int index = 1;
            foreach (var person in people.Items)
            {
                var firstName = person.FirstName ?? string.Empty;
                var lastName = person.LastName ?? string.Empty;
                var userName = person.UserName ?? string.Empty;
                var fullName = $"{firstName} {lastName}".Trim();
                
                Console.WriteLine($"{index,2}. {fullName} - {userName}");
                index++;
            }
        }

        Console.WriteLine();

        Console.WriteLine(DrawSingleLine(BoxWidth));

        Console.WriteLine("Navigation:");
        Console.WriteLine("  [N] Next Page  [P] Previous Page  [S] Select Person  [M] Main Menu");
    }

    public void DisplayPersonDetails(Person person)
    {
        ArgumentNullException.ThrowIfNull(person);

        var boxWidth = PersonBoxWidth - 2;

        Console.WriteLine("┌" + new string('─', PersonBoxWidth - 2) + "┐");

        Console.WriteLine("│" + CenterText("Person Details", PersonBoxWidth - 2) + "│");

        Console.WriteLine("├" + new string('─', PersonBoxWidth - 2) + "┤");

        var firstName = person.FirstName ?? string.Empty;
        var lastName = person.LastName ?? string.Empty;
        var fullName = $"{firstName} {lastName}".Trim();
        Console.WriteLine("│ " + PadRight($"Name:        {fullName}", boxWidth - 1) + "│");

        var userName = person.UserName ?? string.Empty;
        Console.WriteLine("│ " + PadRight($"Username:    {userName}", boxWidth - 1) + "│");

        if (!string.IsNullOrWhiteSpace(person.Gender))
        {
            Console.WriteLine("│ " + PadRight($"Gender:      {person.Gender}", boxWidth - 1) + "│");
        }

        if (person.Age.HasValue)
        {
            Console.WriteLine("│ " + PadRight($"Age:         {person.Age}", boxWidth - 1) + "│");
        }

        if (person.Emails != null && person.Emails.Any())
        {
            var emailDisplay = string.Join(", ", person.Emails);
            Console.WriteLine("│ " + PadRight($"Email(s):    {emailDisplay}", boxWidth - 1) + "│");
        }

        if (!string.IsNullOrWhiteSpace(person.FavoriteFeature))
        {
            Console.WriteLine("│ " + PadRight($"Fav Feature: {person.FavoriteFeature}", boxWidth - 1) + "│");
        }

        if (person.Features != null && person.Features.Any())
        {
            var featuresDisplay = string.Join(", ", person.Features);
            Console.WriteLine("│ " + PadRight($"Features:    {featuresDisplay}", boxWidth - 1) + "│");
        }

        if (person.AddressInfo != null && person.AddressInfo.Any())
        {
            Console.WriteLine("├" + new string('─', PersonBoxWidth - 2) + "┤");
            Console.WriteLine("│ " + PadRight($"Addresses ({person.AddressInfo.Count}):", boxWidth - 1) + "│");

            foreach (var address in person.AddressInfo)
            {
                var addressLine = address.Address ?? string.Empty;
                var cityName = address.City?.Name ?? string.Empty;
                var region = address.City?.Region ?? string.Empty;
                var country = address.City?.CountryRegion ?? string.Empty;

                var cityDisplay = new[] { cityName, region, country }
                    .Where(s => !string.IsNullOrWhiteSpace(s));
                var fullAddress = string.IsNullOrWhiteSpace(addressLine)
                    ? string.Join(", ", cityDisplay)
                    : $"{addressLine}, {string.Join(", ", cityDisplay)}";

                Console.WriteLine("│ " + PadRight($"  {fullAddress}", boxWidth - 1) + "│");
            }
        }

        if (person.HomeAddress != null)
        {
            Console.WriteLine("├" + new string('─', PersonBoxWidth - 2) + "┤");
            Console.WriteLine("│ " + PadRight("Home Address:", boxWidth - 1) + "│");

            var addressLine = person.HomeAddress.Address ?? string.Empty;
            var cityName = person.HomeAddress.City?.Name ?? string.Empty;
            var region = person.HomeAddress.City?.Region ?? string.Empty;
            var country = person.HomeAddress.City?.CountryRegion ?? string.Empty;

            var cityDisplay = new[] { cityName, region, country }
                .Where(s => !string.IsNullOrWhiteSpace(s));
            var fullAddress = string.IsNullOrWhiteSpace(addressLine)
                ? string.Join(", ", cityDisplay)
                : $"{addressLine}, {string.Join(", ", cityDisplay)}";

            Console.WriteLine("│ " + PadRight($"  {fullAddress}", boxWidth - 1) + "│");
        }

        if (person.Friends != null && person.Friends.Any())
        {
            Console.WriteLine("├" + new string('─', PersonBoxWidth - 2) + "┤");
            Console.WriteLine("│ " + PadRight($"Friends ({person.Friends.Count}):", boxWidth - 1) + "│");

            foreach (var friend in person.Friends)
            {
                var friendFirstName = friend.FirstName ?? string.Empty;
                var friendLastName = friend.LastName ?? string.Empty;
                var friendFullName = $"{friendFirstName} {friendLastName}".Trim();
                var friendUserName = friend.UserName ?? string.Empty;

                var friendDisplay = string.IsNullOrEmpty(friendFullName)
                    ? $"  {friendUserName}"
                    : $"  {friendFullName} ({friendUserName})";

                Console.WriteLine("│ " + PadRight(friendDisplay, boxWidth - 1) + "│");

                var friendDetails = new List<string>();
                if (!string.IsNullOrWhiteSpace(friend.Gender))
                {
                    friendDetails.Add(friend.Gender);
                }
                if (friend.Age.HasValue)
                {
                    friendDetails.Add($"Age: {friend.Age}");
                }
                if (friendDetails.Any())
                {
                    Console.WriteLine("│ " + PadRight($"    {string.Join(", ", friendDetails)}", boxWidth - 1) + "│");
                }

                if (friend.Emails != null && friend.Emails.Any())
                {
                    var emailsDisplay = string.Join(", ", friend.Emails);
                    Console.WriteLine("│ " + PadRight($"    Email: {emailsDisplay}", boxWidth - 1) + "│");
                }
            }
        }

        if (person.Trips != null && person.Trips.Any())
        {
            Console.WriteLine("├" + new string('─', PersonBoxWidth - 2) + "┤");
            Console.WriteLine("│ " + PadRight($"Trips ({person.Trips.Count}):", boxWidth - 1) + "│");

            foreach (var trip in person.Trips)
            {
                var tripName = trip.Name ?? string.Empty;
                var startDate = trip.StartsAt.ToString("yyyy-MM-dd");
                var endDate = trip.EndsAt.ToString("yyyy-MM-dd");

                var tripDisplay = string.IsNullOrEmpty(tripName)
                    ? $"  Trip ({startDate} to {endDate})"
                    : $"  {tripName} ({startDate} to {endDate})";
                Console.WriteLine("│ " + PadRight(tripDisplay, boxWidth - 1) + "│");

                Console.WriteLine("│ " + PadRight($"    Budget: ${trip.Budget.ToString("N2", CultureInfo.InvariantCulture)}", boxWidth - 1) + "│");

                if (!string.IsNullOrWhiteSpace(trip.Description))
                {
                    Console.WriteLine("│ " + PadRight($"    {trip.Description}", boxWidth - 1) + "│");
                }

                if (trip.Tags != null && trip.Tags.Any())
                {
                    var tagsDisplay = string.Join(", ", trip.Tags);
                    Console.WriteLine("│ " + PadRight($"    Tags: {tagsDisplay}", boxWidth - 1) + "│");
                }
            }
        }

        Console.WriteLine("└" + new string('─', PersonBoxWidth - 2) + "┘");
    }

    public void DisplayEmptyResult(string message)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Message cannot be empty or whitespace.", nameof(message));
        }

        Console.WriteLine(DrawDoubleLine(BoxWidth));
        Console.WriteLine(CenterText("Search Results", BoxWidth));
        Console.WriteLine(DrawDoubleLine(BoxWidth));
        Console.WriteLine();

        Console.WriteLine(message);
        Console.WriteLine();

        Console.WriteLine("Suggestions:");
        Console.WriteLine("  • Try broader search criteria (use \"Contains\" instead of \"Exact Match\")");
        Console.WriteLine("  • Check spelling of search terms");
        Console.WriteLine("  • Try searching with fewer fields");
        Console.WriteLine("  • Use \"List People\" to browse all available people");
    }

    public void DisplayError(string message)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Message cannot be empty or whitespace.", nameof(message));
        }

        Console.WriteLine(DrawDoubleLine(BoxWidth));
        Console.WriteLine(CenterText("ERROR", BoxWidth));
        Console.WriteLine(DrawDoubleLine(BoxWidth));
        Console.WriteLine();

        Console.WriteLine($"⚠ {message}");
    }

    #region Helper Methods

    private static string CenterText(string text, int width)
    {
        if (text.Length >= width)
            return text;

        int totalPadding = width - text.Length;
        int leftPadding = totalPadding / 2;
        int rightPadding = totalPadding - leftPadding;

        return new string(' ', leftPadding) + text + new string(' ', rightPadding);
    }

    private static string PadRight(string text, int width)
    {
        if (width < 4)
        {
            return text.Length > width ? text.Substring(0, width) : text + new string(' ', width - text.Length);
        }

        if (text.Length > width)
        {
            return text.Substring(0, width - 3) + "...";
        }

        return text + new string(' ', width - text.Length);
    }

    private static string DrawDoubleLine(int width)
    {
        return new string('═', width);
    }

    private static string DrawSingleLine(int width)
    {
        return new string('─', width);
    }

    public void DisplayFilterSummary(SearchCriteria? searchCriteria, FilterCriteria? filterCriteria, int resultCount)
    {
        if (filterCriteria != null && !filterCriteria.IsEmpty())
        {
            DisplayFilterCriteriaSummary(filterCriteria, resultCount);
        }
        else if (searchCriteria != null && !searchCriteria.IsEmpty())
        {
            DisplaySearchCriteriaSummary(searchCriteria, resultCount);
        }
        else
        {
            Console.WriteLine($"Current Filter: None ({resultCount} results)");
        }
    }

    private static void DisplaySearchCriteriaSummary(SearchCriteria searchCriteria, int resultCount)
    {
        var searchType = searchCriteria.SearchType.ToString().ToLowerInvariant();
        var searchValue = searchCriteria.FirstName ?? searchCriteria.LastName ?? searchCriteria.UserName ?? "";

        Console.WriteLine($"Current Filter: FirstName/LastName/UserName {searchType} '{searchValue}' ({resultCount} results)");
    }

    private static void DisplayFilterCriteriaSummary(FilterCriteria filterCriteria, int resultCount)
    {
        var filterParts = new List<string>();

        foreach (var filter in filterCriteria.Filters)
        {
            var searchType = filter.SearchType.ToString().ToLowerInvariant();
            filterParts.Add($"{filter.FieldName} {searchType} '{filter.Value}'");
        }

        var filterText = string.Join(" AND ", filterParts);
        Console.WriteLine($"Current Filter: {filterText} ({resultCount} results)");
    }

    #endregion
}

