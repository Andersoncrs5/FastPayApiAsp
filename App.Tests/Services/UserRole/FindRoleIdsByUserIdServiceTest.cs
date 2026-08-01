using App.Config.uow;
using App.Modules.UserRole.Repositories;
using App.Modules.UserRole.Services.Provider;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace App.Tests.Services.UserRole;

public sealed class FindRoleIdsByUserIdServiceTest
{
    private readonly IUnitOfWork _uow;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly FindRoleIdsByUserIdService _service;

    public FindRoleIdsByUserIdServiceTest()
    {
        _uow = Substitute.For<IUnitOfWork>();
        _userRoleRepository = Substitute.For<IUserRoleRepository>();

        _uow.UserRoleRepository.Returns(_userRoleRepository);

        _service = new FindRoleIdsByUserIdService(_uow);
    }

    [Fact]
    public async Task Execute_WhenUserHasRoles_ShouldReturnListOfRoleIds()
    {
        // Arrange
        long userId = 42L;
        var mockRoleIds = new List<long> { 1L, 2L, 3L };

        _userRoleRepository.GetRoleIdsByUserIdAsync(userId)
            .Returns(mockRoleIds);

        // Act
        var result = await _service.Execute(userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(3);
        result.Value.Should().ContainInOrder(1L, 2L, 3L);

        await _userRoleRepository.Received(1).GetRoleIdsByUserIdAsync(userId);
    }

    [Fact]
    public async Task Execute_WhenUserHasNoRoles_ShouldReturnEmptyList()
    {
        // Arrange
        long userId = 99L;
        var mockEmptyList = new List<long>();

        _userRoleRepository.GetRoleIdsByUserIdAsync(userId)
            .Returns(mockEmptyList);

        // Act
        var result = await _service.Execute(userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();

        await _userRoleRepository.Received(1).GetRoleIdsByUserIdAsync(userId);
    }

    [Fact]
    public async Task Execute_WhenRepositoryThrowsException_ShouldBubbleUp()
    {
        // Arrange
        long userId = 1L;
        var expectedException = new Exception("Database connection failure");

        _userRoleRepository.GetRoleIdsByUserIdAsync(userId)
            .Returns(Task.FromException<List<long>>(expectedException));

        // Act
        var act = async () => await _service.Execute(userId);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Database connection failure");

        await _userRoleRepository.Received(1).GetRoleIdsByUserIdAsync(userId);
    }
}