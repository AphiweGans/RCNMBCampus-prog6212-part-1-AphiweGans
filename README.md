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


