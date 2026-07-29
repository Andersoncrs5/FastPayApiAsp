using App.Config.uow;
using App.Modules.Role.Repositories;
using App.Modules.Role.Services.Base;
using App.Modules.Role.Services.Provider;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace App.Tests.Services.Role;

public sealed class DeleteRoleServiceTest
{
    private readonly IUnitOfWork _uow;
    private readonly IRoleRepository _roleRepository;
    private readonly IDeleteRoleService _service;

    public DeleteRoleServiceTest()
    {
        _uow = Substitute.For<IUnitOfWork>();
        _roleRepository = Substitute.For<IRoleRepository>();

        _uow.RoleRepository.Returns(_roleRepository);

        _service = new DeleteRoleService(_uow);
    }

    [Fact]
    public async Task DeleteRole_WhenRoleExists_ReturnsSuccessResult()
    {
        // Arrange
        const long roleId = 1;
        const int rowsAffected = 1;

        _roleRepository
            .DeleteAndCountAsync(roleId)
            .Returns(rowsAffected);

        // Act
        var result = await _service.DeleteRole(roleId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);

        await _roleRepository
            .Received(1)
            .DeleteAndCountAsync(roleId);
    }

    [Fact]
    public async Task DeleteRole_WhenRoleDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        const long roleId = 999;
        const int rowsAffected = 0;

        _roleRepository
            .DeleteAndCountAsync(roleId)
            .Returns(rowsAffected);

        // Act
        var result = await _service.DeleteRole(roleId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Errors.Should().ContainSingle();
        result.Errors.First().Should().Be("Role not found");

        await _roleRepository
            .Received(1)
            .DeleteAndCountAsync(roleId);
    }

    [Fact]
    public async Task DeleteRole_WhenRepositoryThrowsException_ShouldBubbleUpException()
    {
        // Arrange
        const long roleId = 10;

        var exception = new Exception("Database connection failed");

        _roleRepository
            .DeleteAndCountAsync(roleId)
            .Throws(exception);

        // Act
        Func<Task> act = () => _service.DeleteRole(roleId);

        // Assert
        await act.Should()
            .ThrowAsync<Exception>()
            .WithMessage("Database connection failed");

        await _roleRepository
            .Received(1)
            .DeleteAndCountAsync(roleId);
    }

    [Fact]
    public async Task DeleteRole_ShouldCallDeleteOnlyOnce()
    {
        // Arrange
        const long roleId = 50;

        _roleRepository
            .DeleteAndCountAsync(roleId)
            .Returns(1);

        // Act
        await _service.DeleteRole(roleId);

        // Assert
        await _roleRepository
            .Received(1)
            .DeleteAndCountAsync(roleId);

        await _roleRepository
            .DidNotReceive()
            .CreateAsync(Arg.Any<App.Modules.Role.Model.RoleEntity>());

        await _roleRepository
            .DidNotReceive()
            .UpdateAsync(Arg.Any<App.Modules.Role.Model.RoleEntity>());

        await _roleRepository
            .DidNotReceive()
            .GetByIdAsync(Arg.Any<long>());
    }
}