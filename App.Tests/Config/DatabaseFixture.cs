using App.Config.Database;
using App.Config.Database.Migrations;
using App.Config.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Testcontainers.Kafka;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using IDatabase = App.Config.Database.IDatabase;

namespace App.Tests.Config;

public class DatabaseFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    public PostgreSqlContainer PostgresContainer { get; } = new PostgreSqlBuilder()
        .WithImage("postgres:18-alpine") 
        .WithDatabase("fastpay_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();
    
    private readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:8.0.5")
        .Build();
    //
    // private readonly KafkaContainer _kafkaContainer = new KafkaBuilder()
    //     .WithImage("confluentinc/cp-kafka:7.6.0")
    //     .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            // ==========================================
            // OVERRIDE OF POSTGRES / IDATABASE
            // ==========================================
            var dbDescriptor = services.SingleOrDefault(d => 
                d.ServiceType == typeof(IDatabase));
            if (dbDescriptor != null) services.Remove(dbDescriptor);

            var postgresConn = PostgresContainer.GetConnectionString();
            services.AddSingleton<IOptions<DatabaseOptions>>(_ => 
                Options.Create(new DatabaseOptions { Postgres = postgresConn }));
            
            services.AddSingleton<IDatabase, Database>();

            var redisDescriptor = services.SingleOrDefault(d => 
                d.ServiceType == typeof(IConnectionMultiplexer));
            if (redisDescriptor != null) services.Remove(redisDescriptor);

            var redisConn = _redisContainer.GetConnectionString();
            var multiplexer = ConnectionMultiplexer.Connect(redisConn);
            services.AddSingleton<IConnectionMultiplexer>(multiplexer);

            services.AddSingleton<IOptions<RedisOptions>>(_ => 
                Options.Create(new RedisOptions { ConnectionString = redisConn }));

            services.AddSingleton<IOptions<JwtOptions>>(_ => 
                Options.Create(new JwtOptions 
                { 
                    SecretKey = "secret_key_com_tamanho_suficiente_para_o_algoritmo_de_teste_valido",
                    ValidIssuer = "FastPay.App",
                    ValidAudience = "FastPay.App.Users",
                    TokenValidityInMinutes = 60,
                    RefreshTokenValidityInMinutes = 120
                }));
        });
    }

    public async Task InitializeAsync()
    { 
        await Task.WhenAll(
            PostgresContainer.StartAsync(), 
            _redisContainer.StartAsync()
            //_kafkaContainer.StartAsync()
        );
        
        using var scope = Services.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<MigrationRunner>();
        await runner.RunAsync();
    }

    public new async Task DisposeAsync()
    {
        await Task.WhenAll(
            PostgresContainer.StopAsync(), 
            _redisContainer.StopAsync()
            //_kafkaContainer.StopAsync()
        );
    }
}
