using System.Text.Json;
using App.Config.Database;
using App.Config.Exceptions;
using App.Config.Extensions;
using App.Config.Options;
using App.Config.Snowflake;
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
using IDatabase = App.Config.Database.IDatabase;
using App.Config.Database.Migrations;
using App.Modules.User.Repositories;

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
// JSON Source Generator (AOT)
// ============================

// builder.Services.ConfigureHttpJsonOptions(options =>
// {
//     options.SerializerOptions.TypeInfoResolverChain.Insert(
//         0,
//         AppJsonSerializerContext.Default
//      );
// });



// ============================
// Heathy
// ============================

builder.Services
    .AddHealthChecks()
    .AddCheck(
        "self",
        () => HealthCheckResult.Healthy(),
        tags: ["live"])
    .AddNpgSql(
        builder.Configuration.GetConnectionString("Postgres")!,
        name: "postgres",
        tags: ["ready"])
    .AddRedis(
        builder.Configuration["Redis:ConnectionString"]!,
        name: "redis",
        tags: ["ready"]);

// ============================
// Snowflake ID 
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

builder.Services.AddSingleton<IDatabase, Database>();

// ============================
// DATABASE MIGRATIONS
// ============================

builder.Services.AddSingleton<IMigration, V001CreateSchemaMigrationsTable>();
builder.Services.AddSingleton<IMigration, V002CreateUsersTable>();

builder.Services.AddSingleton<MigrationRunner>();

// ============================
// Application Services
// ============================

builder.Services.AddScoped<IUserRepository, UserRepository>();

// builder.Services.AddFastPayModules();

// ============================
// OpenTelemetry + SigNoz
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
                options.Endpoint =
                    new Uri("http://localhost:4317");
            });
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter(options =>
            {
                options.Endpoint =
                    new Uri("http://localhost:4317");
            });
    });


// ============================
// OpenAPI (AOT Compatible)
// ============================

builder.Services.AddOpenApi();


// ============================
// API Versioning
// ============================

builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion =
            new ApiVersion(1);

        options.AssumeDefaultVersionWhenUnspecified = true;

        options.ReportApiVersions = true;
    });

// ============================
// Exceptions
// ============================

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddProblemDetails();

builder.Host.UseSerilog();

// ============================
// Security / Performance
// ============================

builder.Services.AddRateLimiter();


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


// ============================
// Application Services
// ============================

// builder.Services.AddFastPayModules();


// ============================
// Build
// ============================

var app = builder.Build();

// ============================
// OpenAPI
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
    var runner = scope.ServiceProvider
        .GetRequiredService<MigrationRunner>();

    await runner.RunAsync();
}

// ============================
// Middleware Pipeline
// ============================

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseRateLimiter();

// app.UseCors();

app.UseAuthentication();

app.UseResponseCompression();

app.UseAuthorization();


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
        Predicate = check => check.Tags.Contains("ready")
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

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response));
        }
    });

// ============================
// Endpoints
// ============================

// app.MapPaymentEndpoints();
// app.MapCustomerEndpoints();
// app.MapWebhookEndpoints();


app.Run();


public partial class Program { }