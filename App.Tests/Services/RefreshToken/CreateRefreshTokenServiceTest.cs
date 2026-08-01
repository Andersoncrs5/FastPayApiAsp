using App.Config.Options;
using App.Config.Security.Crypto; // Injetado para mockar a dependência do Gateway
using App.Config.uow;
using App.Modules.RefreshToken.Gateway;
using App.Modules.RefreshToken.Model;
using App.Modules.RefreshToken.Repositories;
using App.Modules.RefreshToken.Services.Provider;
using FluentAssertions;
using IdGen;
using Microsoft.Extensions.Options;
using Npgsql;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace App.Tests.Services.RefreshToken;

public sealed class CreateRefreshTokenServiceTest
{
    private readonly IOptions<JwtOptions> _jwtOptions;
    private readonly IUnitOfWork _uow;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IdGenerator _idGenerator;
    private readonly ICryptoService _cryptoService; 
    private readonly RefreshTokenGateway _gateway;  
    private readonly CreateRefreshTokenService _service;

    public CreateRefreshTokenServiceTest()
    {
        _jwtOptions = Options.Create(new JwtOptions
        {
            SecretKey = "super-secret-key-for-jwt-token",
            ValidIssuer = "FastPay",
            ValidAudience = "FastPayClients",
            TokenValidityInMinutes = 15,
            RefreshTokenValidityInMinutes = 60
        });

        _uow = Substitute.For<IUnitOfWork>();
        _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
        _cryptoService = Substitute.For<ICryptoService>();
        
        _idGenerator = new IdGenerator(0); 
        
        _gateway = new RefreshTokenGateway(_cryptoService);

        _uow.RefreshTokenRepository.Returns(_refreshTokenRepository);
        _uow.IdGenerator.Returns(_idGenerator);

        _service = new CreateRefreshTokenService(_jwtOptions, _uow, _gateway);
    }

    [Fact]
    public async Task Execute_WhenValidUserId_ShouldCreateAndReturnRefreshToken()
    {
        long userId = 102030L;
        string expectedHash = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";

        _cryptoService.ComputeSha256Hash(Arg.Any<string>()).Returns(expectedHash);
        
        _refreshTokenRepository.CreateAsync(Arg.Any<RefreshTokenEntity>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.Execute(userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Value.Should().NotBeNull();
        
        result.Value.Token.Should().NotBeNullOrEmpty();
        result.Value.ExpiresAt.Should().BeCloseTo(
            DateTimeOffset.UtcNow.AddMinutes(_jwtOptions.Value.RefreshTokenValidityInMinutes), 
            TimeSpan.FromSeconds(5)
        );

        _cryptoService.Received(1).ComputeSha256Hash(result.Value.Token);

        await _refreshTokenRepository.Received(1).CreateAsync(
            Arg.Is<RefreshTokenEntity>(entity =>
                entity.UserId == userId &&
                entity.TokenHash == expectedHash &&
                entity.ExpiresAt == result.Value.ExpiresAt &&
                entity.Id > 0));
    }

    [Fact]
    public async Task Execute_WhenDatabaseThrowsForeignKeyViolation_ShouldReturnConflictResult()
    {
        long userId = 999L;
        _cryptoService.ComputeSha256Hash(Arg.Any<string>()).Returns("mocked_hash");

        var pgException = CreatePostgresException("23503", "fk_refresh_tokens_users");

        _refreshTokenRepository.CreateAsync(Arg.Any<RefreshTokenEntity>())
            .Throws(pgException);
// Act
        var result = await _service.Execute(userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.Errors.First().Should().Be("fk_refresh_tokens_users");
    }

    [Fact]
    public async Task Execute_WhenUnexpectedExceptionOccurs_ShouldBubbleUp()
    {
        // Arrange
        long userId = 123L;
        _cryptoService.ComputeSha256Hash(Arg.Any<string>()).Returns("mocked_hash");

        _refreshTokenRepository.CreateAsync(Arg.Any<RefreshTokenEntity>())
            .Throws(new Exception("Lost connection to DB"));

        // Act
        var act = async () => await _service.Execute(userId);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Lost connection to DB");
    }

    private static PostgresException CreatePostgresException(string sqlState, string constraintName)
    {
        return new PostgresException(
            messageText: "database constraint violation triggered",
            severity: "ERROR",
            invariantSeverity: "ERROR",
            sqlState: sqlState,
            constraintName: constraintName);
    }
}