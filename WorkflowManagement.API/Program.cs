using FluentValidation;
using FluentValidation.AspNetCore;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Security.Claims;
using Serilog;
using WorkflowManagement.API.Middleware;
using WorkflowManagement.API.Services;
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
Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;

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

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document),
            new List<string>()
        }
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
// Security Services & Coral-Style Claims Transformation
// ============================================================
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<IClaimsTransformation, CustomClaimsTransformation>();

// ============================================================
// Authentication & JWT Bearer (Azure AD SSO)
// ============================================================
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = builder.Configuration["SSOConfig:AuthorityUrl"];
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["SSOConfig:ValidIssuer"] ?? string.Empty,
        ValidateAudience = true,
        ValidAudience = builder.Configuration["SSOConfig:ValidAudiencesUrl"] ?? string.Empty,
        ValidateLifetime = true
    };
    options.MetadataAddress = builder.Configuration["SSOConfig:MetadataAddress"] ?? string.Empty;
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var authorization = context.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authorization.Substring("Bearer ".Length).Trim();
                if (token.Contains("&"))
                {
                    context.Token = token.Split('&')[0];
                }
            }
            return Task.CompletedTask;
        }
    };
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

// Workflow Engine (Instances & Tasks)
builder.Services.AddScoped<
    IWorkflowInstanceRepository,
    WorkflowInstanceRepository>();

builder.Services.AddScoped<
    IWorkflowTaskRepository,
    WorkflowTaskRepository>();

builder.Services.AddScoped<
    IWorkflowEngineService,
    WorkflowEngineService>();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Redirect root URL to /swagger
app.MapGet("/", async context =>
{
    context.Response.Redirect("/swagger");
    await Task.CompletedTask;
});

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