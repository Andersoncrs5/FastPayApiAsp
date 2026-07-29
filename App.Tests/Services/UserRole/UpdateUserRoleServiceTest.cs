using App.Config.Database;
using App.Config.uow;
using App.Modules.UserRole.Dto.Requests;
using App.Modules.UserRole.Gateway;
using App.Modules.UserRole.Mapper;
using App.Modules.UserRole.Model;
using App.Modules.UserRole.Repositories;
using App.Modules.UserRole.Services.Base;
using App.Modules.UserRole.Services.Provider;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Npgsql;
using Xunit;

namespace App.Tests.Services.UserRole;

public sealed class UpdateUserRoleServiceTest
{
    private readonly IUnitOfWork _uow;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly UserRoleGateway _gateway;
    private readonly UserRoleMapper _mapper;
    private readonly IUpdateUserRoleService _service;

    public UpdateUserRoleServiceTest()
    {
        _uow = Substitute.For<IUnitOfWork>();
        _userRoleRepository = Substitute.For<IUserRoleRepository>();
        _gateway = Substitute.For<UserRoleGateway>();
        _mapper = new UserRoleMapper();

        _uow.UserRoleRepository.Returns(_userRoleRepository);

        _service = new UpdateUserRoleService(
            _uow,
            _gateway,
            _mapper);
    }

    [Fact]
    public async Task Execute_WhenUserRoleExists_ShouldUpdateSuccessfully()
    {
        long id = 1;

        var entity = new UserRoleEntity
        {
            Id = id,
            UserId = 10,
            RoleId = 20,
            Active = true,
            AssignedByUserId = 5,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var dto = new UpdateUserRoleDto(
            Active: false,
            AssignedByUserId: 99,
            RevokedAt: DateTimeOffset.UtcNow);

        _userRoleRepository.GetByIdAsync(id)
            .Returns(entity);

        _userRoleRepository.UpdateAsync(Arg.Any<UserRoleEntity>())
            .Returns(Task.CompletedTask);

        var result = await _service.Execute(id, dto);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);

        result.Value.Active.Should().BeFalse();
        result.Value.AssignedByUserId.Should().Be(99);
        result.Value.RevokedAt.Should().Be(dto.RevokedAt);

        await _userRoleRepository.Received(1)
            .UpdateAsync(Arg.Is<UserRoleEntity>(e =>
                e.Id == id &&
                e.Active == false &&
                e.AssignedByUserId == 99 &&
                e.RevokedAt == dto.RevokedAt));
    }

    [Fact]
    public async Task Execute_WhenUserRoleDoesNotExist_ShouldReturnNotFound()
    {
        long id = 100;

        var dto = new UpdateUserRoleDto(
            Active: false,
            AssignedByUserId: 1,
            RevokedAt: null);

        _userRoleRepository.GetByIdAsync(id)
            .Returns((UserRoleEntity?)null);

        var result = await _service.Execute(id, dto);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Errors.First().Should().Be("User not found");

        await _userRoleRepository.DidNotReceive()
            .UpdateAsync(Arg.Any<UserRoleEntity>());
    }

    [Fact]
    public async Task Execute_WhenUniqueConstraintOccurs_ShouldReturnConflict()
    {
        long id = 1;

        var entity = new UserRoleEntity
        {
            Id = id,
            UserId = 10,
            RoleId = 20,
            Active = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var dto = new UpdateUserRoleDto(
            Active: false,
            AssignedByUserId: null,
            RevokedAt: null);

        _userRoleRepository.GetByIdAsync(id)
            .Returns(entity);

        _userRoleRepository.UpdateAsync(Arg.Any<UserRoleEntity>())
            .Throws(CreateUniqueViolation("ux_user_roles_user_role"));

        var result = await _service.Execute(id, dto);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.Errors.First().Should().Be("ux_user_roles_user_role");
    }

    [Fact]
    public async Task Execute_WhenUnexpectedException_ShouldBubbleUp()
    {
        long id = 1;

        var entity = new UserRoleEntity
        {
            Id = id,
            UserId = 10,
            RoleId = 20,
            Active = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var dto = new UpdateUserRoleDto(
            Active: false,
            AssignedByUserId: null,
            RevokedAt: null);

        _userRoleRepository.GetByIdAsync(id)
            .Returns(entity);

        _userRoleRepository.UpdateAsync(Arg.Any<UserRoleEntity>())
            .Throws(new Exception("Database connection failed"));

        var ex = await Assert.ThrowsAsync<Exception>(
            () => _service.Execute(id, dto));

        ex.Message.Should().Be("Database connection failed");
    }

    private static PostgresException CreateUniqueViolation(string constraintName)
    {
        return new PostgresException(
            messageText: "duplicate key value violates unique constraint",
            severity: "ERROR",
            invariantSeverity: "ERROR",
            sqlState: "23505",
            constraintName: constraintName);
    }
}