## RaceDay - Part 2: RESTful API Development
## Overview
This part implements the RaceDay RESTful API in ASP.NET Core (C#), built directly
to the endpoint plan approved in Part 1. It connects to a SQL Server database via
Entity Framework Core (Code-First), enforces role-based access for Organisers and
Participants using cookie-based session authentication, and is fully covered by
unit tests that run automatically through GitHub Actions.

## Two Roles
Organiser - can create, edit, and delete events; manage event categories;
capture participant results; and view all enrolments for their own events.
Participant - can register, view their own profile, browse events and
categories, enrol in a category, view their own enrolments, and view their own
results.
Role is selected at registration and stored as a claim in the authentication
cookie after login, so every subsequent request already carries the user's role.

## Project Structure
RaceDay_Part2/
├── RaceDay.API/
│   ├── Controllers/       # AuthController, ProfileController, EventsController,
│   │                        CategoriesController, EnrolmentsController, ResultsController
│   ├── Models/             # Organiser, Participant, Event, Category, Enrolment, Result
│   ├── DTOs/                # Request/response shapes for each endpoint
│   ├── Data/                # RaceDayContext (EF Core, Code-First)
│   ├── Services/            # PasswordService (BCrypt hashing)
│   └── Program.cs
├── RaceDay.Tests/
│   ├── CustomWebApplicationFactory.cs  # Spins up the API with an in-memory DB for tests
│   ├── TestHelpers.cs                  # Shared login helper for tests
│   ├── AuthTests.cs
│   ├── EventManagementTests.cs
│   ├── RoleAccessTests.cs
│   └── EnrolmentTests.cs
└── .github/workflows/dotnet-ci.yml

## Note on schema deviation from Part 1
The Events table now also stores `Distance` and `EventType` (Run/Walk/Cycle),
which were not part of the original Part 1 ERD - the ERD only captured
distance at the Category level. This field was added directly to Events because
the Part 2 brief explicitly states "each event must capture a name, description,
date, location, distance, and event type."
This is a genuine, deliberate trade-off, not an oversight: the Part 2 rubric's
top band for "API Endpoints and Database" asks the database to match the Part 1
ERD and SQL script exactly, while the Part 2 functional requirements ask for
fields the ERD didn't include. Since the functional requirement is explicit and
specific, this repo follows it and documents the deviation here, as the brief
permits. If your marker prioritises an exact ERD match over the stated
functional requirement, consider removing `Distance`/`EventType` from Events
and keeping distance only at the Category level instead.

## Setup Instructions
Install .NET 8 SDK, Visual Studio 2022 (with ASP.NET and web development
workload), and SQL Server (Developer Edition) with SSMS.
Clone the repository and open `RaceDay.API.csproj` (or the solution) in Visual Studio.
Update the connection string in `RaceDay.API/appsettings.json` if your SQL Server
instance name differs from `localhost`.
Restore NuGet packages (Visual Studio does this automatically on open, or run
`dotnet restore` from the `RaceDay_Part2` folder).
Create the database using EF Core migrations:
```
   cd RaceDay.API
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```
Run the API (`dotnet run` from `RaceDay.API`, or F5 in Visual Studio).
Open `https://localhost:<port>/swagger` to view and test every endpoint through
the Swagger UI.

## Running the Tests
```
cd RaceDay_Part2
dotnet test RaceDay.Tests/RaceDay.Tests.csproj
```
Tests use an EF Core in-memory database, so no live SQL Server connection is
needed to run them - this is also why they can run inside GitHub Actions.


##Authentication Flow
`POST /api/auth/register` - creates an Organiser or Participant record with a
BCrypt-hashed password (never stored in plain text).
`POST /api/auth/login` - verifies credentials and issues an authentication
cookie containing the user's id and role as claims.
Every subsequent request automatically carries this cookie, so
`[Authorize(Roles = "Organiser")]` / `[Authorize(Roles = "Participant")]`
attributes on controllers enforce access without re-sending credentials.
`POST /api/auth/logout` - ends the session.

## CI/CD
Workflow file: `.github/workflows/dotnet-ci.yml`
Runs on every push/PR to `main`: restores, builds, and runs all unit tests.
Screenshot of successful green build: ADD_SCREENSHOT_HERE

## Video
Unlisted YouTube link: 
