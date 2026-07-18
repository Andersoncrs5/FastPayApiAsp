using App.Config.Database;
using App.Config.Exceptions;
using App.Config.Extensions;
using App.Config.Options;
using Asp.Versioning;
using IdGen;
using Microsoft.Extensions.Options;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using StackExchange.Redis;
using IDatabase = App.Config.Database.IDatabase;

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
// POSTGRES 
// ============================

builder.Services.AddSingleton<IIdGenerator<long>>(_ =>
{
    var structure = new IdStructure(45, 2, 16);

    var generator = new IdGenerator(
        generatorId: 1
        );

    return generator;
});

// ============================
// POSTGRES 
// ============================

builder.Services.AddSingleton<IDatabase, Database>();

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
// Middleware Pipeline
// ============================

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseRateLimiter();

// app.UseCors();

app.UseAuthentication();

app.UseResponseCompression();

app.UseAuthorization();

// ============================
// Endpoints
// ============================

// app.MapPaymentEndpoints();
// app.MapCustomerEndpoints();
// app.MapWebhookEndpoints();


app.Run();


public partial class Program { }