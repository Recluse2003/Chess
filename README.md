# Chess Website

A web-based chess application built with ASP.NET Core Razor Pages and Entity Framework Core. The project focuses on implementing chess functionality alongside a maintainable backend architecture with clearly separated responsibilities.

## Features

-

## Technology Stack

**Backend**
- C#
- ASP.NET Core Razor Pages
- Entity Framework Core
- MySQL
- MediatR for command and query handling
- SignalR for real-time updates between two players
- Dependency injection

**Architecture and design**
- Domain-driven modelling of core chess concepts
- Layered architecture with Domain, Application, Infrastructure and Web projects
- Repository interfaces and implementations
- Data Transfer Objects (DTOs)
- Application-level validation and result/error handling

**Testing**
- Xunit
- FluentAssertions

## Architecture

The solution is divided into four projects:

- **Domain** — Contains the core chess domain entities and business rules.
- **Application** — Coordinates application use cases through commands and queries, defines relevant abstractions, and handles application-level validation. A result pattern is used to return the responses which could include the value requested, or an error depending on if a domain rule is broken, that results in an exception. 
- **Infrastructure** — Implements persistence and other infrastructure concerns, including Entity Framework Core and repository implementations. Also contains the persistence entities, which are mapped from domain entities, to be persisted into the database. 
- **Web** — Provides the Razor Pages user interface and connects incoming requests to the application layer. 

### Architectural principles

The project uses dependency inversion to keep core application logic independent of infrastructure implementation details.

- Domain logic is kept separate from presentation and persistence concerns.
- Application code depends on abstractions rather than concrete infrastructure implementations.
- Infrastructure implements interfaces defined by the application layer.
- Dependency injection provides the implementations together at runtime.
- Commands and queries separate use cases that modify data from those that retrieve data.

