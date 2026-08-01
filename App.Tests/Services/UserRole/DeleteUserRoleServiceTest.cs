using App.Config.uow;
using App.Modules.UserRole.Repositories;
using App.Modules.UserRole.Services.Provider;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace App.Tests.Services.UserRole;

public sealed class DeleteUserRoleServiceTest
{
    private readonly IUnitOfWork _uow;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly DeleteUserRoleService _service;

    public DeleteUserRoleServiceTest()
    {
        _uow = Substitute.For<IUnitOfWork>();
        _userRoleRepository = Substitute.For<IUserRoleRepository>();

        _uow.UserRoleRepository.Returns(_userRoleRepository);

        _service = new DeleteUserRoleService(_uow);
    }


    [Fact]
    public async Task DeleteUserRole_WhenRelationExists_ReturnsSuccessResult()
    {
        long userRoleId = 123L;

        _userRoleRepository.DeleteAndCountAsync(userRoleId)
            .Returns(1);

        var result = await _service.DeleteAsync(userRoleId);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        await _userRoleRepository.Received(1)
            .DeleteAndCountAsync(userRoleId);
    }


    [Fact]
    public async Task DeleteUserRole_WhenRelationDoesNotExist_ReturnsNotFoundError()
    {
        long userRoleId = 999L;

        _userRoleRepository.DeleteAndCountAsync(userRoleId)
            .Returns(0);

        var result = await _service.DeleteAsync(userRoleId);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);

        result.Errors.First()
            .Should()
            .Be("User Role not found");

        await _userRoleRepository.Received(1)
            .DeleteAndCountAsync(userRoleId);
    }


    [Fact]
    public async Task DeleteUserRole_WhenRepositoryThrowsException_BubblesUpException()
    {
        long userRoleId = 123L;

        _userRoleRepository.DeleteAndCountAsync(userRoleId)
            .Throws(new Exception("Database connection failed"));

        var exception = await Assert.ThrowsAsync<Exception>(
            () => _service.DeleteAsync(userRoleId));

        exception.Message
            .Should()
            .Be("Database connection failed");
    }
}