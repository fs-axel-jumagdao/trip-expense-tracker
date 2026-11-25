using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TripExpenseTracker.Api.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Structured logging through Serilog only (no print or console statements in feature code).
builder.Host.UseSerilog((context, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console());

// Feature handlers (MediatR) and their validators (FluentValidation), found by assembly.
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// EF Core against SQL Server. The connection string comes from the environment, never from code.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Centralized error handling: no stack traces reach the client.
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.Run();

// Exposed for the test project.
public partial class Program { }
