# RentzyWeb - Project Context

RentzyWeb is a comprehensive property rental management system built using ASP.NET Core 8.0. It facilitates property listings, booking management, payment processing, and administrative oversight through a multi-tenant platform.

## Architecture & Technologies

The project follows a classic **N-tier architecture** to ensure separation of concerns and maintainability:

-   **RentzyWeb (Presentation Layer):** An ASP.NET Core MVC application. It contains Controllers, Views, and Web-specific logic. It uses session-based authentication.
-   **Rentzy.BLL (Business Logic Layer):** Contains services that implement core business rules, DTOs (Data Transfer Objects) for data exchange, and custom exceptions.
-   **Rentzy.DAL (Data Access Layer):** Manages data persistence using Entity Framework Core with SQL Server. Includes Entity Models, DBContext, Repositories, and Migrations.

### Core Technologies
-   **Framework:** .NET 8.0
-   **ORM:** Entity Framework Core
-   **Database:** SQL Server
-   **Authentication:** Custom Session-based (with BCrypt hashing)
-   **UI:** Razor Views, Vanilla CSS/JS
-   **Dependency Injection:** Built-in .NET DI container

## Project Structure

```text
/
├── RentzyWeb/             # MVC Web Application
│   ├── Controllers/       # Logic for handling requests
│   ├── Views/             # UI Templates
│   └── Program.cs         # App entry point & DI configuration
├── Rentzy.BLL/            # Business Logic Layer
│   ├── Services/          # Domain services (e.g., AuthService, PropertyService)
│   ├── DTOs/              # Data Transfer Objects
│   └── Factory/           # Object creation logic (e.g., UserFactory)
└── Rentzy.DAL/            # Data Access Layer
    ├── Context/           # EF Core DBContext
    ├── Models/            # Entity models (TPH inheritance for Users)
    ├── Repository/        # Data access implementations
    └── Migrations/        # Database schema history
```

## Getting Started

### Prerequisites
-   .NET 8 SDK
-   SQL Server (LocalDB or Instance)

### Building and Running
-   **Restore dependencies:** `dotnet restore`
-   **Build solution:** `dotnet build`
-   **Run Web app:** `dotnet run --project RentzyWeb/Rentzy.Web.csproj`

### Database Management
-   **Update Database:** `dotnet ef database update --project Rentzy.DAL --startup-project RentzyWeb`
-   **Add Migration:** `dotnet ef migrations add <MigrationName> --project Rentzy.DAL --startup-project RentzyWeb`

## Development Conventions

1.  **Strict Layering:** Never reference `RentzyWeb` from `BLL` or `DAL`. The Web layer depends on `BLL`, and `BLL` depends on `DAL`.
2.  **DTO Usage:** Always use DTOs to pass data between the Web and BLL layers. Do not leak Entity Models to the Views.
3.  **Repository Pattern:** Use repositories for all database operations to facilitate testing and decouple business logic from the ORM.
4.  **Async/Await:** Use asynchronous programming (`Task`, `async`, `await`) for all I/O bound operations (database, email).
5.  **Inheritance:** The `User` model uses Table-per-Hierarchy (TPH). Specialized roles like `Landlord` and `Tenant` inherit from `User`.
6.  **Error Handling:** Throw specific business exceptions in the BLL and handle them in the Controllers (or via middleware) to return appropriate user feedback.
7.  **Null Safety:** Utilize the Null Object Pattern (e.g., `NoUser`) for cases where a result might not be found, rather than returning null.
8.  **Security:** 
    -   Passswords must be hashed using `BCrypt`.
    -   Sensitive data should be stored in `appsettings.json` and managed via Environment Variables or Secrets in production.
