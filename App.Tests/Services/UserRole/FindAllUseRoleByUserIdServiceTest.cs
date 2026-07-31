using App.Config.uow;
using App.Modules.UserRole.Model;
using App.Modules.UserRole.Repositories;
using App.Modules.UserRole.Services.Base;
using App.Modules.UserRole.Services.Provider;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace App.Tests.Services.UserRole;

public sealed class FindAllUseRoleByUserIdServiceTest
{
    private readonly IUnitOfWork _uow;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IFindAllUseRoleByUserIdService _service;

    public FindAllUseRoleByUserIdServiceTest()
    {
        _uow = Substitute.For<IUnitOfWork>();
        _userRoleRepository = Substitute.For<IUserRoleRepository>();

        _uow.UserRoleRepository.Returns(_userRoleRepository);

        _service = new FindAllUseRoleByUserIdService(_uow);
    }

    [Fact]
    public async Task Execute_WhenUserHasRoles_ReturnsSuccessWithRoles()
    {
        long userId = 10;

        var roles = new List<UserRoleEntity>
        {
            new()
            {
                Id = 1,
                UserId = userId,
                RoleId = 100,
                Active = true,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = 2,
                UserId = userId,
                RoleId = 200,
                Active = true,
                CreatedAt = DateTimeOffset.UtcNow
            }
        };

        _userRoleRepository.GetAllByUserId(userId)
            .Returns(roles);

        var result = await _service.Execute(userId);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);

        result.Value.Should().HaveCount(2);
        result.Value.Should().BeEquivalentTo(roles);

        await _userRoleRepository.Received(1)
            .GetAllByUserId(userId);
    }

    [Fact]
    public async Task Execute_WhenUserHasNoRoles_ReturnsSuccessWithEmptyList()
    {
        long userId = 99;

        _userRoleRepository.GetAllByUserId(userId)
            .Returns(new List<UserRoleEntity>());

        var result = await _service.Execute(userId);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);

        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();

        await _userRoleRepository.Received(1)
            .GetAllByUserId(userId);
    }

    [Fact]
    public async Task Execute_WhenRepositoryThrowsException_BubblesUpException()
    {
        long userId = 10;

        _userRoleRepository.GetAllByUserId(userId)
            .Throws(new Exception("Database connection failed"));

        var exception = await Assert.ThrowsAsync<Exception>(
            () => _service.Execute(userId));

        exception.Message.Should().Be("Database connection failed");

        await _userRoleRepository.Received(1)
            .GetAllByUserId(userId);
    }
}