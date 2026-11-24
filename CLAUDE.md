# CLAUDE.md

> AI Development Guide for React 19 + .NET 10 + MSSQL

---

# Project Overview

This application is a full-stack web platform built using:

- React 19
- TypeScript
- Vite
- ASP.NET Core .NET 10 Web API
- Entity Framework Core
- Microsoft SQL Server
- Vertical Slice Architecture
- MediatR
- FluentValidation

The goal of this document is to establish standards for both human and AI-assisted development.

AI-generated code should always adhere to the conventions, architecture, and quality requirements defined in this document.

---

# Core Principles

## Priorities

1. Maintainability
2. Simplicity
3. Readability
4. Testability
5. Performance
6. Security

## General Rules

- Follow SOLID principles pragmatically.
- Avoid over-engineering.
- Favor feature-based organization.
- Write self-documenting code.
- Eliminate dead code.
- Use strong typing wherever possible.
- Minimize shared abstractions.
- Keep features independently maintainable.

---

# Technology Stack

## Frontend

- React 19
- TypeScript
- Vite
- React Router
- TanStack Query
- Axios
- React Hook Form
- Zod
- Vitest
- React Testing Library

## Backend

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- MediatR
- FluentValidation
- Serilog
- xUnit
- Moq

## Database

- Microsoft SQL Server

---

# Architecture

## Frontend Architecture

```text
client/
├── src/
│
├── api/
│
├── components/
│
├── features/
│
├── pages/
│
├── routes/
│
├── hooks/
│
├── layouts/
│
├── types/
│
├── utils/
│
├── constants/
│
└── tests/
```

---

## Backend Architecture

Follow Vertical Slice Architecture.

Organize code by feature, not by technical layer.

Avoid folders such as:

```text
Controllers/
Services/
Repositories/
```

Prefer:

```text
Features/
```

Example:

```text
server/

├── Features/
│
├── Expenses/
│   ├── CreateExpense/
│   ├── UpdateExpense/
│   ├── DeleteExpense/
│   └── GetExpenses/
│
├── Trips/
│   ├── CreateTrip/
│   ├── UpdateTrip/
│   └── GetTrip/
│
├── Infrastructure/
├── Persistence/
├── Common/
└── Program.cs
```

---

# Vertical Slice Standards

Each feature should own its:

- Request
- Command
- Query
- Handler
- Validator
- Endpoint
- Response
- Tests

Example:

```text
Features/
└── Expenses/
    └── CreateExpense/
        ├── CreateExpenseCommand.cs
        ├── CreateExpenseHandler.cs
        ├── CreateExpenseValidator.cs
        ├── CreateExpenseEndpoint.cs
        ├── CreateExpenseResponse.cs
        └── CreateExpenseTests.cs
```

---

# Backend Standards

## API Design

Use RESTful APIs.

### Naming

Good:

```text
GET    /api/trips
GET    /api/trips/{id}

POST   /api/trips

PUT    /api/trips/{id}

DELETE /api/trips/{id}
```

Avoid:

```text
/api/getTrips
/api/createTrip
/api/deleteTrip
```

---

## Commands vs Queries

### Commands

Commands modify data.

Examples:

```text
CreateExpense
UpdateExpense
DeleteExpense
```

### Queries

Queries return data.

Examples:

```text
GetExpense
GetExpenses
GetTripDetails
```

---

## MediatR

Use MediatR for all feature requests.

Command:

```csharp
public sealed record CreateExpenseCommand(
    string Description,
    decimal Amount
) : IRequest<CreateExpenseResponse>;
```

Handler:

```csharp
public sealed class CreateExpenseHandler
    : IRequestHandler<CreateExpenseCommand, CreateExpenseResponse>
{
}
```

---

## Validation

Use FluentValidation.

Every request requiring validation must have a validator.

Example:

```csharp
public sealed class CreateExpenseValidator
    : AbstractValidator<CreateExpenseCommand>
{
    public CreateExpenseValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Amount)
            .GreaterThan(0);
    }
}
```

---

## Entity Framework

Use EF Core directly.

Do not create generic repositories.

Good:

```csharp
await dbContext.Expenses
    .FirstOrDefaultAsync(x => x.Id == request.Id);
```

Avoid:

```csharp
_repository.GetById(id);
```

unless there is a strong business reason.

---

## Dependency Injection

Use constructor injection.

Example:

```csharp
public sealed class CreateExpenseHandler
{
    private readonly AppDbContext _dbContext;

    public CreateExpenseHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
}
```

---

## Exception Handling

Use centralized exception handling.

Never expose stack traces.

Example:

```csharp
app.UseExceptionHandler();
```

---

## Logging

Use structured logging.

Example:

```csharp
_logger.LogInformation(
    "Expense {ExpenseId} created",
    expense.Id);
```

Never:

```csharp
_logger.LogInformation(
    $"Expense {expense.Id} created");
```

---

# Database Standards

## Naming

### Tables

Use plural names.

```text
Trips
Expenses
Users
Categories
```

### Columns

Use PascalCase.

```text
Id
TripId
CreatedAt
UpdatedAt
```

### Foreign Keys

```text
TripId
UserId
CategoryId
```

---

## Audit Fields

All major entities should include:

```csharp
CreatedAt
CreatedBy
UpdatedAt
UpdatedBy
```

---

## Entity Configurations

Use IEntityTypeConfiguration.

Example:

```csharp
public class ExpenseConfiguration
    : IEntityTypeConfiguration<Expense>
{
    public void Configure(
        EntityTypeBuilder<Expense> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Description)
            .HasMaxLength(100)
            .IsRequired();
    }
}
```

---

## Migrations

Every schema change requires a migration.

Never modify migrations already deployed.

---

# Frontend Standards

## Components

Prefer small focused components.

Good:

```text
ExpenseCard.tsx
ExpenseForm.tsx
ExpenseList.tsx
```

Avoid:

```text
ExpenseManagementPage.tsx
```

with thousands of lines.

---

## Component Structure

Example:

```text
features/
└── expenses/
    ├── components/
    ├── hooks/
    ├── api/
    ├── schemas/
    └── types/
```

---

## TypeScript

Enable strict mode.

Avoid:

```typescript
any
```

Prefer:

```typescript
interface Expense {
  id: number;
  description: string;
  amount: number;
}
```

---

## State Management

Prefer:

- TanStack Query for server state
- React state for local state

Avoid unnecessary global state libraries.

Use Context only when justified.

---

## API Communication

Use centralized API clients.

Example:

```typescript
const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
});
```

---

## Forms

Use:

- React Hook Form
- Zod

Example:

```typescript
const form = useForm<CreateExpenseRequest>({
  resolver: zodResolver(schema),
});
```

---

# Testing Standards

## Coverage Target

Minimum:

```text
80%
```

Required for:

- Business logic
- Handlers
- Validators
- Critical workflows

---

# Backend Testing

Frameworks:

- xUnit
- Moq

Test:

- Handlers
- Validators
- Services
- Business rules

Example:

```csharp
[Fact]
public async Task CreateExpense_Should_Create_Record()
{
}
```

---

# Frontend Testing

Frameworks:

- Vitest
- React Testing Library

Test:

- Components
- Hooks
- Forms
- User interactions

Avoid testing implementation details.

Focus on behavior.

Example:

```typescript
it("submits an expense", async () => {
});
```

---

# Security Standards

## Required

- Validate all inputs.
- Use HTTPS.
- Use parameterized SQL.
- Sanitize user input.
- Use environment variables.
- Follow least privilege.

---

## Never

- Hardcode secrets.
- Commit API keys.
- Commit connection strings.
- Log sensitive data.
- Return internal errors.

---

# Environment Variables

Note (decided 2026-11-24, see docs/plans/F1-registration-and-login.md): login sessions are server-side, so the `Jwt__` settings below are not used for login.

Backend:

```env
ConnectionStrings__DefaultConnection=

Jwt__Key=

Jwt__Issuer=

Jwt__Audience=
```

Frontend:

```env
VITE_API_URL=
```

---

# Development Commands

## Frontend

Install:

```bash
npm install
```

Run:

```bash
npm run dev
```

Build:

```bash
npm run build
```

Preview:

```bash
npm run preview
```

Lint:

```bash
npm run lint
```

Run tests:

```bash
npm run test
```

Coverage:

```bash
npm run test:coverage
```

Run tests once:

```bash
npm run test -- --run
```

---

## Backend

Restore:

```bash
dotnet restore
```

Build:

```bash
dotnet build
```

Run:

```bash
dotnet run
```

Watch:

```bash
dotnet watch run
```

Clean:

```bash
dotnet clean
```

---

# Unit Test Commands

Run all tests:

```bash
dotnet test
```

Run with coverage:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

Run specific project:

```bash
dotnet test tests/Application.Tests
```

Run by test name:

```bash
dotnet test --filter "CreateExpense"
```

Run by category:

```bash
dotnet test --filter Category=Unit
```

Generate TRX results:

```bash
dotnet test --logger trx
```

---

# Entity Framework Commands

Install EF CLI:

```bash
dotnet tool install --global dotnet-ef
```

Create migration:

```bash
dotnet ef migrations add InitialCreate
```

Create migration:

```bash
dotnet ef migrations add AddExpensesTable
```

Update database:

```bash
dotnet ef database update
```

Remove migration:

```bash
dotnet ef migrations remove
```

Generate SQL script:

```bash
dotnet ef migrations script
```

---

# Pull Request Requirements

Every PR must:

- Compile successfully.
- Pass all tests.
- Include tests for new functionality.
- Follow naming guidelines.
- Avoid unrelated changes.
- Maintain coverage expectations.
- Receive code review approval.

---

# AI Assistant Rules

When generating code:

1. Follow Vertical Slice Architecture.
2. Use feature-based organization.
3. Use MediatR for requests.
4. Use FluentValidation for validation.
5. Use EF Core directly.
6. Avoid generic repositories.
7. Use async/await.
8. Generate unit tests.
9. Generate strongly typed DTOs.
10. Prefer records for requests and responses.
11. Follow REST conventions.
12. Minimize abstractions.
13. Keep solutions simple.
14. Produce production-ready code.
15. Ensure code builds successfully.
16. Ensure tests pass.
17. Maintain minimum 80% test coverage.
18. Follow existing project conventions before introducing new patterns.

If an existing convention conflicts with this document, follow the existing project convention and document the deviation.