# OData Console App

## About the Project

Interactive OData console application built to explore, search, and display detailed information about People using the TripPin OData service. Built with .NET 9, featuring clean layered architecture, dependency injection, structured logging, and automated tests.

## Table of Contents

- [Architecture](#architecture)
- [Key Features](#key-features)
- [Technologies](#technologies)
- [Prerequisites](#prerequisites)
- [Setup](#setup)
- [Usage](#usage)
- [Running Tests](#running-tests)
- [Configuration](#configuration)
- [Project Structure](#project-structure)
- [Design Patterns Used](#design-patterns-used)
- [Logging](#logging)

## Architecture

The project follows **Clean Architecture** principles with 4 main layers:

```
┌─────────────────────────────────────┐
│         UI Layer (Console)          │
│  - PeopleExplorerHandler            │
│  - GlobalSearchHandler              │
│  - AdvancedFilterHandler            │
│  - PersonDetailsHandler             │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│      Application Layer              │
│  - PersonService                    │
│  - DisplayService                   │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│     Infrastructure Layer            │
│  - ODataRepository                  │
│  - SearchStrategies                 │
│  - FilterStrategies                 │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│         Core Layer                  │
│  - Models (Person, Trip, etc)       │
│  - Interfaces                       │
│  - Configuration                    │
└─────────────────────────────────────┘
```

### Layer Responsibilities

- **Core**: Domain models, interfaces, and configuration classes
- **Infrastructure**: External service communication (OData API), search and filter strategies
- **Application**: Business logic and service classes
- **UI**: User interface management (console menus and input handling)

## Key Features

- **People Explorer**: Browse all people with pagination support
- **Global Search**: Search by first name, last name, or email
- **Advanced Filtering**:
  - Age Range
  - City
  - Gender
  - Email Domain
  - And other criteria
- **Person Details**: View detailed information about selected person (friends, trips, addresses)
- **Pagination**: Page-by-page navigation for large datasets
- **Logging**: Detailed file-based logging with Serilog

## Technologies

- **.NET 9.0**
- **Microsoft.Extensions.DependencyInjection** - Dependency Injection
- **Microsoft.Extensions.Configuration** - Configuration management
- **Serilog** - Structured logging
- **HttpClient** - OData API communication
- **xUnit** - Unit testing
- **Moq** - Mocking framework (for tests)

## Prerequisites

To run this project, you need:

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Terminal or Command Prompt
- Internet connection (for OData service communication)

## Setup

### 1. Clone the repository

```bash
git clone <repository-url>
cd odata-console-app
```

### 2. Restore dependencies

```bash
dotnet restore
```

### 3. Build the project

```bash
dotnet build
```

## Usage

### Running the application

From the project root directory:

```bash
dotnet run --project src/ODataConsoleApp/ODataConsoleApp.csproj
```

Or run the built executable directly:

```bash
cd src/ODataConsoleApp/bin/Debug/net9.0
./ODataConsoleApp
```

### Main Menu

After the application starts, the following options are presented:

```
=== OData People Explorer ===
1. Browse All People
2. Global Search
3. Advanced Filter
4. Exit

Choose an option:
```

#### 1. Browse All People
Displays a list of all people with pagination. A specific number of results are shown per page, with the ability to navigate to next/previous pages.

#### 2. Global Search
Users can search by entering a keyword to search across first name, last name, or email.

**Example:**
```
Enter search term: Russell
```

#### 3. Advanced Filter
Filter data by one or more criteria:
- **Age Range**: Age range (e.g., 25-35)
- **City**: City name (e.g., Seattle)
- **Gender**: Gender (Male/Female)
- **Email Domain**: Email domain (e.g., example.com)

#### 4. Exit
Exit the application.

### Viewing Person Details

You can view detailed information about any person by entering their UserName from the list:
- Basic information (Name, Email, Gender, Birth Date)
- Address information
- Friends list
- Trips

## Running Tests

The project includes unit tests. To run the tests:

```bash
dotnet test
```

Run tests with detailed output:

```bash
dotnet test --verbosity normal
```

View test coverage:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Test Structure

Tests cover the following areas:
- **Infrastructure Tests**: `ODataRepository` tests
- **Service Tests**: `PersonService` tests
- **Search Tests**: Search strategy tests
- **Filter Tests**: Filter strategy tests

## Configuration

The application configuration is stored in `appsettings.json`:

```json
{
  "ODataService": {
    "BaseUrl": "http://services.odata.org/TripPinRESTierService/(S(...))/",
    "DefaultPageSize": 10,
    "RequestTimeout": 30
  },
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "logs/odata-console-.log",
          "rollingInterval": "Day"
        }
      }
    ]
  }
}
```

### Parameters

- **BaseUrl**: OData service base URL
- **DefaultPageSize**: Number of results to display per page
- **RequestTimeout**: HTTP request timeout duration (in seconds)
- **Serilog**: Logging configuration

## Project Structure

```
ODataConsoleApp/
│
├── src/
│   └── ODataConsoleApp/
│       ├── Program.cs                    # Entry point
│       ├── ApplicationHost.cs             # DI container and configuration
│       ├── appsettings.json              # Configuration file
│       │
│       ├── Core/                         # Domain layer
│       │   ├── Configuration/            # Configuration classes
│       │   ├── Interfaces/               # Interfaces
│       │   └── Models/                   # Domain models
│       │
│       ├── Application/                  # Business logic layer
│       │   └── Services/                 # Service classes
│       │
│       ├── Infrastructure/               # External services layer
│       │   ├── Repositories/             # OData Repository
│       │   └── Search/                   # Search and filtering
│       │
│       └── UI/                           # Presentation layer
│           ├── ConsoleWrapper.cs         # Console I/O
│           ├── PeopleExplorerHandler.cs  # Main menu
│           ├── GlobalSearchHandler.cs    # Search UI
│           ├── AdvancedFilterHandler.cs  # Filter UI
│           └── PersonDetailsHandler.cs   # Details UI
│
└── tests/
    └── ODataConsoleApp.Tests/
        ├── Infrastructure/               # Infrastructure layer tests
        ├── Services/                     # Application layer tests
        └── Search/                       # Search strategy tests
```

## Design Patterns Used

### 1. **Dependency Injection (DI)**
All services and repositories are managed through the DI container. This ensures loose coupling and testability.

**Example:**
```csharp
services.AddTransient<IPersonService, PersonService>();
services.AddSingleton<IODataRepository, ODataRepository>();
```

### 2. **Repository Pattern**
Data source communication is abstracted through the `IODataRepository` interface.

**Benefits:**
- Separates data access logic from business logic
- Easily mockable in tests
- Easy to change data sources

### 3. **Strategy Pattern**
Strategy Pattern is used for search and filter functionality.

**Implementation:**
- `ISearchStrategy` - Interface for different search strategies
- `IFilterStrategy` - Interface for different filter strategies
- `SearchStrategyFactory` - Dynamically selects appropriate strategy
- `CompositeFilterStrategy` - Combines multiple filters

**Benefits:**
- Easy to add new search or filter types
- Each strategy has its own responsibility (Single Responsibility)

### 4. **Factory Pattern**
`SearchStrategyFactory` creates the appropriate search strategy at runtime.

### 5. **Options Pattern**
Configuration is managed with strongly-typed classes (`IOptions<T>`).

**Example:**
```csharp
services.AddSingleton<IOptions<ODataServiceSettings>>(
    new OptionsWrapper<ODataServiceSettings>(appSettings.ODataService));
```

## Logging

The application uses **Serilog** structured logging library.

### Log Files

Log files are stored in the `logs/` directory:
```
logs/odata-console-YYYYMMDD.log
```

A separate file is created for each day (rolling interval: Day).

### Log Levels

- **Information**: General information messages
- **Warning**: Warning messages
- **Error**: Error messages
- **Fatal**: Critical errors

### Sample Log Output

```
[2025-12-30 12:34:56 INF] Application started successfully
[2025-12-30 12:35:02 INF] Fetching people with filter: $top=10&$skip=0
[2025-12-30 12:35:03 INF] Successfully retrieved 10 people
[2025-12-30 12:40:15 INF] Application exiting normally
```

## Additional Notes

### OData Service
The application uses Microsoft's public TripPin OData demo service. This service provides sample data and requires an internet connection.

### Error Handling
The application includes comprehensive error handling:
- Configuration validation
- HTTP request error handling
- User input validation
- Detailed logging for debugging

### Extension Possibilities
The project architecture is ready for the following extensions:
- Adding new search strategies
- Adding new filter criteria
- Connecting to other OData services
- Export functionality (JSON, CSV)
- Caching mechanism
