using App.Modules.RefreshToken.Model;
using App.Tests.Config;
using FluentAssertions;
using Xunit.Abstractions;

namespace App.Tests.Repositories;

public sealed class RefreshTokenRepositoryTest : BaseIntegrationTest
{
    private readonly DatabaseFixture _fixture;
    private readonly ITestOutputHelper _output;

    public RefreshTokenRepositoryTest(
        DatabaseFixture fixture,
        ITestOutputHelper output) : base(fixture)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task CreateRefreshToken_ShouldCreateToken()
    {
        var user = await CreateUser();

        var token = await CreateRefreshToken(user.Id);

        token.Should().NotBeNull();
        token.Id.Should().BeGreaterThan(0);
        token.UserId.Should().Be(user.Id);
        token.TokenHash.Should().NotBeNullOrWhiteSpace();
        token.ExpiresAt.Should().BeAfter(DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task ExistsById_Should_Return_False()
    {
        long id = Faker.Random.Long(100_000, 999_999);

        bool exists = await RefreshTokenRepository.ExistsByIdAsync(id);

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsById_Should_Return_True()
    {
        var user = await CreateUser();
        var token = await CreateRefreshToken(user.Id);

        bool exists = await RefreshTokenRepository.ExistsByIdAsync(token.Id);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task GetById_Should_Return_Token_WhenItExists()
    {
        var user = await CreateUser();
        var token = await CreateRefreshToken(user.Id);

        var dbToken = await RefreshTokenRepository.GetByIdAsync(token.Id);

        dbToken.Should().NotBeNull();
        dbToken!.Id.Should().Be(token.Id);
        dbToken.UserId.Should().Be(user.Id);
        dbToken.TokenHash.Should().Be(token.TokenHash);
    }

    [Fact]
    public async Task GetById_Should_Return_Null_WhenItDoesNotExist()
    {
        long id = Faker.Random.Long(100_000, 999_999);

        var dbToken = await RefreshTokenRepository.GetByIdAsync(id);

        dbToken.Should().BeNull();
    }

    [Fact]
    public async Task GetByTokenHash_Should_Return_Token_WhenItExists()
    {
        var user = await CreateUser();
        var token = await CreateRefreshToken(user.Id);

        var dbToken = await RefreshTokenRepository.GetByTokenHashAsync(token.TokenHash);

        dbToken.Should().NotBeNull();
        dbToken!.Id.Should().Be(token.Id);
        dbToken.UserId.Should().Be(user.Id);
        dbToken.TokenHash.Should().Be(token.TokenHash);
    }

    [Fact]
    public async Task GetByTokenHash_Should_Return_Null_WhenItDoesNotExist()
    {
        var dbToken = await RefreshTokenRepository.GetByTokenHashAsync(
            Guid.NewGuid().ToString("N"));

        dbToken.Should().BeNull();
    }

    [Fact]
    public async Task GetAllByUserId_Should_Return_AllTokensForUser()
    {
        var user = await CreateUser();
        var otherUser = await CreateUser();

        var token1 = await CreateRefreshToken(user.Id);
        var token2 = await CreateRefreshToken(user.Id);
        var otherToken = await CreateRefreshToken(otherUser.Id);

        var tokens = await RefreshTokenRepository.GetAllByUserIdAsync(user.Id);

        tokens.Should().NotBeNull();
        tokens.Should().HaveCountGreaterThanOrEqualTo(2);
        tokens.Should().Contain(t => t.Id == token1.Id);
        tokens.Should().Contain(t => t.Id == token2.Id);
        tokens.Should().NotContain(t => t.Id == otherToken.Id);
    }

    [Fact]
    public async Task ExistsByTokenHash_Should_Return_True_WhenTokenExists()
    {
        var user = await CreateUser();
        var token = await CreateRefreshToken(user.Id);

        bool exists = await RefreshTokenRepository.ExistsByTokenHashAsync(token.TokenHash);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByTokenHash_Should_Return_False_WhenTokenDoesNotExist()
    {
        bool exists = await RefreshTokenRepository.ExistsByTokenHashAsync(
            Guid.NewGuid().ToString("N"));

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task Update_ShouldPersistChanges()
    {
        var user = await CreateUser();
        var token = await CreateRefreshToken(user.Id);

        token.RevokedAt = DateTimeOffset.UtcNow;
        token.UpdatedAt = DateTimeOffset.UtcNow;

        await RefreshTokenRepository.UpdateAsync(token);

        var updated = await RefreshTokenRepository.GetByIdAsync(token.Id);

        updated.Should().NotBeNull();
        updated!.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Delete_ShouldDeleteToken()
    {
        var user = await CreateUser();
        var token = await CreateRefreshToken(user.Id);

        await RefreshTokenRepository.DeleteAsync(token.Id);

        bool exists = await RefreshTokenRepository.ExistsByIdAsync(token.Id);

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAndCount_ShouldDeleteToken()
    {
        var user = await CreateUser();
        var token = await CreateRefreshToken(user.Id);

        int count = await RefreshTokenRepository.DeleteAndCountAsync(token.Id);

        bool exists = await RefreshTokenRepository.ExistsByIdAsync(token.Id);

        exists.Should().BeFalse();
        count.Should().Be(1);
    }

    [Fact]
    public async Task DeleteAll_ShouldRemoveAllTokens()
    {
        var user1 = await CreateUser();
        var user2 = await CreateUser();

        await CreateRefreshToken(user1.Id);
        await CreateRefreshToken(user2.Id);

        int deleted = await RefreshTokenRepository.DeleteAllAsync();

        deleted.Should().BeGreaterThanOrEqualTo(2);
        (await RefreshTokenRepository.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task DeleteAllById_ShouldRemoveOnlyProvidedTokens()
    {
        var user = await CreateUser();

        var token1 = await CreateRefreshToken(user.Id);
        var token2 = await CreateRefreshToken(user.Id);
        var token3 = await CreateRefreshToken(user.Id);

        int deleted = await RefreshTokenRepository.DeleteAllByIdAsync(
            [token1.Id, token3.Id]);

        deleted.Should().Be(2);

        (await RefreshTokenRepository.ExistsByIdAsync(token1.Id)).Should().BeFalse();
        (await RefreshTokenRepository.ExistsByIdAsync(token3.Id)).Should().BeFalse();
        (await RefreshTokenRepository.ExistsByIdAsync(token2.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAllById_Should_ReturnZero_WhenIdsListIsEmpty()
    {
        int deleted = await RefreshTokenRepository.DeleteAllByIdAsync([]);

        deleted.Should().Be(0);
    }

    [Fact]
    public async Task CreateAll_ShouldCreateMultipleTokens()
    {
        var user = await CreateUser();

        var tokens = new List<RefreshTokenEntity>
        {
            new()
            {
                Id = Faker.Random.Long(10_000_000, 9_999_999_999),
                UserId = user.Id,
                TokenHash = Guid.NewGuid().ToString("N"),
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(30),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Faker.Random.Long(10_000_000, 9_999_999_999),
                UserId = user.Id,
                TokenHash = Guid.NewGuid().ToString("N"),
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(30),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            }
        };

        int inserted = await RefreshTokenRepository.CreateAllAsync(tokens);

        inserted.Should().Be(2);
        (await RefreshTokenRepository.CountAsync()).Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task UpdateAll_ShouldUpdateMultipleTokens()
    {
        var user = await CreateUser();
        var token1 = await CreateRefreshToken(user.Id);
        var token2 = await CreateRefreshToken(user.Id);

        token1.RevokedAt = DateTimeOffset.UtcNow;
        token2.RevokedAt = DateTimeOffset.UtcNow;

        int updated = await RefreshTokenRepository.UpdateAllAsync([token1, token2]);

        updated.Should().Be(2);

        var dbToken1 = await RefreshTokenRepository.GetByIdAsync(token1.Id);
        var dbToken2 = await RefreshTokenRepository.GetByIdAsync(token2.Id);

        dbToken1!.RevokedAt.Should().NotBeNull();
        dbToken2!.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Repository_Should_ExecuteAllBaseCrudOperationsSuccessfully()
    {
        var user = await CreateUser();
        var token = await CreateRefreshToken(user.Id);

        var dbToken = await RefreshTokenRepository.GetByIdAsync(token.Id);
        var exists = await RefreshTokenRepository.ExistsByIdAsync(token.Id);

        dbToken.Should().NotBeNull();
        dbToken!.Id.Should().Be(token.Id);
        dbToken.UserId.Should().Be(user.Id);
        dbToken.TokenHash.Should().Be(token.TokenHash);
        exists.Should().BeTrue();

        dbToken.RevokedAt = DateTimeOffset.UtcNow;
        dbToken.UpdatedAt = DateTimeOffset.UtcNow;

        await RefreshTokenRepository.UpdateAsync(dbToken);

        var updated = await RefreshTokenRepository.GetByIdAsync(token.Id);
        updated.Should().NotBeNull();
        updated!.RevokedAt.Should().NotBeNull();

        int affectedRows = await RefreshTokenRepository.DeleteAndCountAsync(token.Id);
        bool existsAfterDelete = await RefreshTokenRepository.ExistsByIdAsync(token.Id);

        affectedRows.Should().Be(1);
        existsAfterDelete.Should().BeFalse();
    }
}