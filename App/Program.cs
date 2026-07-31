using System.Text.Json;
using App.Config.Database.Migrations;
using App.Config.Exceptions;
using App.Config.Extensions;
using App.Config.Options;
using App.Config.Security;
using App.Config.Tx;
using App.Config.Snowflake;
using App.Modules.Role.Repositories;
using App.Modules.Role.Services.Base;
using App.Modules.Role.Services.Provider;
using Asp.Versioning;
using IdGen;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using StackExchange.Redis;
using App.Modules.User.Repositories;
using App.Modules.User.Services.Base;
using App.Modules.User.Services.Provider;

var builder = WebApplication.CreateSlimBuilder(args);

const string serviceName = "FastPay.App";

// ============================
// OPTIONS 
// ============================
builder.Services.AddApplicationOptions(builder.Configuration);
builder.Services.AddResponseCompression();

// ============================
// REDIS
// ============================
builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
{
    RedisOptions options = provider.GetRequiredService<IOptions<RedisOptions>>().Value;
    return ConnectionMultiplexer.Connect(options.ConnectionString);
});

// ============================
// HEALTH CHECKS
// ============================
builder.Services
    .AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
    .AddNpgSql(
        builder.Configuration.GetConnectionString("Postgres")!,
        name: "postgres",
        tags: ["ready"])
    .AddRedis(
        builder.Configuration["Redis:ConnectionString"]!,
        name: "redis",
        tags: ["ready"]);

// ============================
// SNOWFLAKE
// ============================
builder.Services.AddSingleton<ISnowflakeGenerator, SnowflakeGenerator>();

builder.Services.AddSingleton<IIdGenerator<long>>(_ =>
{
    var structure = new IdStructure(
        timestampBits: 45,
        generatorIdBits: 2,
        sequenceBits: 16);

    return new IdGenerator(
        generatorId: 1,
        new IdGeneratorOptions(structure));
});

// ============================
// POSTGRES
// ============================
builder.Services.AddTransient<App.Config.Database.IDatabase, App.Config.Database.Database>();

// ============================
// TX
// ============================
builder.Services.AddScoped<IRequestDbContext, RequestDbContext>();
builder.Services.AddScoped<TransactionalMiddleware>();

// ============================
// DATABASE MIGRATIONS
// ============================
builder.Services.AddSingleton<App.Config.Database.IMigration, V001CreateUsersTable>();
builder.Services.AddSingleton<App.Config.Database.IMigration, V002CreateRolesTable>();
builder.Services.AddSingleton<App.Config.Database.IMigration, V003CreateUserRoleTable>();
builder.Services.AddSingleton<App.Config.Database.MigrationRunner>();

// ============================
// APPLICATION
// ============================
builder.Services.AddScoped<IDeleteUserService, DeleteUserService>();
builder.Services.AddScoped<ICreateUserService, CreateUserService>();
builder.Services.AddScoped<IUpdateUserService, UpdateUserService>();
builder.Services.AddScoped<IFindUserByIdService, FindUserByIdService>();

builder.Services.AddScoped<IDeleteRoleService, DeleteRoleService>();
builder.Services.AddScoped<ICreateRoleService, CreateRoleService>();
builder.Services.AddScoped<IUpdateRoleService, UpdateRoleService>();
builder.Services.AddScoped<IFindRoleByIdService, FindRoleByIdService>();

builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();

// ============================
// REPOSITORIES
// ============================
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();

// ============================
// OPENTELEMETRY
// ============================
builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddNpgsql()
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri("http://localhost:4317");
            });
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri("http://localhost:4317");
            });
    });

// ============================
// OPENAPI
// ============================
builder.Services.AddOpenApi();

// ============================
// VERSIONING
// ============================
builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    });

// ============================
// EXCEPTIONS
// ============================
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Host.UseSerilog();

// ============================
// SECURITY / PERF
// ============================
builder.Services.AddAuthorization();
builder.Services.AddAuthentication();
builder.Services.AddRateLimiter();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ============================
// BUILD
// ============================
var app = builder.Build();

// ============================
// OPENAPI
// ============================
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// ============================
// RUN DATABASE MIGRATIONS
// ============================
using (var scope = app.Services.CreateScope())
{
    var runner = scope.ServiceProvider.GetRequiredService<App.Config.Database.MigrationRunner>();
    await runner.RunAsync();
}

// ============================
// MIDDLEWARE
// ============================
app.UseExceptionHandler();
app.UseMiddleware<TransactionalMiddleware>();

app.UseHttpsRedirection();
app.UseRateLimiter();
// app.UseCors();

app.UseAuthentication();
app.UseResponseCompression();
app.UseAuthorization();

// ============================
// HEALTH
// ============================
app.MapHealthChecks(
    "/health/live",
    new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("live")
    });

app.MapHealthChecks(
    "/health/ready",
    new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";

            var response = new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(entry => new
                {
                    name = entry.Key,
                    status = entry.Value.Status.ToString(),
                    duration = entry.Value.Duration
                }),
                duration = report.TotalDuration
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    });

app.Run();

public partial class Program { }