using FluentValidation;
using FluentValidation.AspNetCore;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using WorkflowManagement.API.Middleware;
using WorkflowManagement.Application.Interfaces;
using WorkflowManagement.Application.Services;
using WorkflowManagement.Infrastructure.Repositories;

// ============================================================
// Serilog — configure before builder so startup errors are logged
// ============================================================
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(
        "logs/workflowmanagement-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .Enrich.FromLogContext()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Replace default logging with Serilog
builder.Host.UseSerilog();

// ============================================================
// Controllers
// ============================================================
builder.Services.AddControllers();

// ============================================================
// FluentValidation — auto-validates request DTOs
// Scans Application assembly for all AbstractValidator<T>
// ============================================================
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<
    WorkflowManagement.Application.Common.PagedResponse<object>>();

// ============================================================
// Swagger / OpenAPI
// ============================================================
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Workflow Management API",
        Version = "v1"
    });
});

// ============================================================
// CORS — allow Angular dev server and production origin
// ============================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:4200",   // Angular dev server
                "https://localhost:4200")  // Angular dev server (HTTPS)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ============================================================
// Health Checks — SQL Server connectivity
// ============================================================
var connectionString =
    builder.Configuration.GetConnectionString(
        "DefaultConnection")!;

builder.Services.AddHealthChecks()
    .AddSqlServer(
        connectionString,
        name: "sqlserver",
        tags: new[] { "db", "sql" });

// ============================================================
// Dependency Injection — Repositories & Services
// ============================================================

// Organization
builder.Services.AddScoped<
    IOrganizationRepository,
    OrganizationRepository>();

builder.Services.AddScoped<
    IOrganizationService,
    OrganizationService>();

// Workflow
builder.Services.AddScoped<
    IWorkflowRepository,
    WorkflowRepository>();

builder.Services.AddScoped<
    IWorkflowService,
    WorkflowService>();

// Roles
builder.Services.AddScoped<
    IRoleRepository,
    RoleRepository>();

builder.Services.AddScoped<
    IRoleService,
    RoleService>();

// Users
builder.Services.AddScoped<
    IUserRepository,
    UserRepository>();

builder.Services.AddScoped<
    IUserService,
    UserService>();

// UserRoles
builder.Services.AddScoped<
    IUserRoleRepository,
    UserRoleRepository>();

builder.Services.AddScoped<
    IUserRoleService,
    UserRoleService>();

// WorkflowSteps
builder.Services.AddScoped<
    IWorkflowStepRepository,
    WorkflowStepRepository>();

builder.Services.AddScoped<
    IWorkflowStepService,
    WorkflowStepService>();

// ============================================================
// Build & Pipeline
// ============================================================
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "Workflow Management API v1");
    });
}

app.UseHttpsRedirection();

// CORS must be before UseAuthorization
app.UseCors("AllowAngular");

// Custom global exception handler
app.UseExceptionMiddleware();

app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

try
{
    Log.Information(
        "Workflow Management API starting up...");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(
        ex,
        "Application startup failed.");
}
finally
{
    Log.CloseAndFlush();
}