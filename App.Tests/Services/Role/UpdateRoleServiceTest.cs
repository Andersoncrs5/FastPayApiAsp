using App.Config.Database;
using App.Config.Exceptions.Classes;
using App.Config.uow;
using App.Modules.Role.Dto.Requests;
using App.Modules.Role.Mapper;
using App.Modules.Role.Model;
using App.Modules.Role.Repositories;
using App.Modules.Role.Services.Base;
using App.Modules.Role.Services.Provider;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Npgsql;
using Xunit;

namespace App.Tests.Services.Role;

public sealed class UpdateRoleServiceTest
{
    private readonly IUnitOfWork _uow;
    private readonly IRoleRepository _roleRepository;
    private readonly RoleMapper _mapper;
    private readonly IUpdateRoleService _service;

    public UpdateRoleServiceTest()
    {
        _uow = Substitute.For<IUnitOfWork>();
        _roleRepository = Substitute.For<IRoleRepository>();
        _mapper = new RoleMapper();

        _uow.RoleRepository.Returns(_roleRepository);

        _service = new UpdateRoleService(
            _uow,
            _mapper);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRoleExists_ShouldUpdateSuccessfully()
    {
        // Arrange
        const long roleId = 1;

        var existingRole = new RoleEntity
        {
            Id = roleId,
            Name = "User",
            NormalizedName = "USER",
            Description = "Old description",
            Active = true,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-10),
            UpdatedAt = DateTimeOffset.UtcNow.AddDays(-10)
        };

        var dto = new UpdateRoleDto(
            Name: "Admin",
            Description: "System administrator",
            "ADMIN",
            Active: false);

        _roleRepository.GetByIdAsync(roleId)
            .Returns(existingRole);

        _roleRepository.UpdateAsync(Arg.Any<RoleEntity>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ExecuteAsync(roleId, dto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);

        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(roleId);
        result.Value.Name.Should().Be("Admin");
        result.Value.NormalizedName.Should().Be("ADMIN");
        result.Value.Description.Should().Be("System administrator");
        result.Value.Active.Should().BeFalse();

        await _roleRepository.Received(1).GetByIdAsync(roleId);

        await _roleRepository.Received(1).UpdateAsync(
            Arg.Is<RoleEntity>(role =>
                role.Id == roleId &&
                role.Name == "Admin" &&
                role.NormalizedName == "ADMIN" &&
                role.Description == "System administrator" &&
                role.Active == false));

        Received.InOrder(() =>
        {
            _roleRepository.GetByIdAsync(roleId);
            _roleRepository.UpdateAsync(Arg.Any<RoleEntity>());
        });
    }

    [Fact]
    public async Task ExecuteAsync_WhenRoleDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        const long roleId = 999;

        var dto = new UpdateRoleDto(
            Name: "Admin",
            Description: "System administrator",
            "ADMIN",
            Active: true);

        _roleRepository.GetByIdAsync(roleId)
            .Returns((RoleEntity?)null);

        // Act
        var result = await _service.ExecuteAsync(roleId, dto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Errors.Should().ContainSingle();
        result.Errors.First().Should().Be("Role not found");

        await _roleRepository.Received(1).GetByIdAsync(roleId);
        await _roleRepository.DidNotReceive().UpdateAsync(Arg.Any<RoleEntity>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRepositoryThrowsUniqueConstraint_ShouldThrowDatabaseConflictException()
    {
        // Arrange
        const long roleId = 1;

        var existingRole = new RoleEntity
        {
            Id = roleId,
            Name = "User",
            NormalizedName = "USER",
            Description = "Old description",
            Active = true,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-10),
            UpdatedAt = DateTimeOffset.UtcNow.AddDays(-10)
        };

        var dto = new UpdateRoleDto(
            Name: "Admin",
            Description: "System administrator",
            NormalizedName: existingRole.NormalizedName,
            Active: true);

        _roleRepository.GetByIdAsync(roleId)
            .Returns(existingRole);

        _roleRepository.UpdateAsync(Arg.Any<RoleEntity>())
            .Throws(CreateUniqueViolation("ux_roles_name"));

        // Act
        var exception = await Assert.ThrowsAsync<DatabaseConflictException>(
            () => _service.ExecuteAsync(roleId, dto));

        // Assert
        exception.Message.Should().Be("ux_roles_name");

        await _roleRepository.Received(1).GetByIdAsync(roleId);
        await _roleRepository.Received(1).UpdateAsync(Arg.Any<RoleEntity>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRepositoryThrowsNotNullViolation_ShouldThrowDatabaseValidationException()
    {
        // Arrange
        const long roleId = 1;

        var existingRole = new RoleEntity
        {
            Id = roleId,
            Name = "User",
            NormalizedName = "USER",
            Description = "Old description",
            Active = true,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-10),
            UpdatedAt = DateTimeOffset.UtcNow.AddDays(-10)
        };

        var dto = new UpdateRoleDto(
            Name: "Admin",
            Description: null,
            "USER",
            Active: true);

        _roleRepository.GetByIdAsync(roleId)
            .Returns(existingRole);

        _roleRepository.UpdateAsync(Arg.Any<RoleEntity>())
            .Throws(CreateNotNullViolation("name"));

        // Act
        var exception = await Assert.ThrowsAsync<DatabaseValidationException>(
            () => _service.ExecuteAsync(roleId, dto));

        // Assert
        exception.Message.Should().Be("name");

        await _roleRepository.Received(1).GetByIdAsync(roleId);
        await _roleRepository.Received(1).UpdateAsync(Arg.Any<RoleEntity>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRepositoryThrowsUnexpectedException_ShouldBubbleUp()
    {
        // Arrange
        const long roleId = 1;

        var existingRole = new RoleEntity
        {
            Id = roleId,
            Name = "User",
            NormalizedName = "USER",
            Description = "Old description",
            Active = true,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-10),
            UpdatedAt = DateTimeOffset.UtcNow.AddDays(-10)
        };

        var dto = new UpdateRoleDto(
            Name: "Admin",
            Description: "System administrator",
            "USER",
            Active: true);

        _roleRepository.GetByIdAsync(roleId)
            .Returns(existingRole);

        _roleRepository.UpdateAsync(Arg.Any<RoleEntity>())
            .Throws(new Exception("Database connection failed"));

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _service.ExecuteAsync(roleId, dto));

        // Assert
        exception.Message.Should().Be("Database connection failed");

        await _roleRepository.Received(1).GetByIdAsync(roleId);
        await _roleRepository.Received(1).UpdateAsync(Arg.Any<RoleEntity>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNormalizeNameToUppercase()
    {
        // Arrange
        const long roleId = 1;

        var existingRole = new RoleEntity
        {
            Id = roleId,
            Name = "User",
            NormalizedName = "USER",
            Description = "Old description",
            Active = true,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-10),
            UpdatedAt = DateTimeOffset.UtcNow.AddDays(-10)
        };

        var dto = new UpdateRoleDto(
            Name: "manager",
            Description: "Operations manager",
            "MANAGER",
            Active: true);

        _roleRepository.GetByIdAsync(roleId)
            .Returns(existingRole);

        _roleRepository.UpdateAsync(Arg.Any<RoleEntity>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ExecuteAsync(roleId, dto);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _roleRepository.Received(1).UpdateAsync(
            Arg.Is<RoleEntity>(role =>
                role.Name == "manager" &&
                role.NormalizedName == "MANAGER"));
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

    private static PostgresException CreateNotNullViolation(string columnName)
    {
        return new PostgresException(
            messageText: "null value violates not-null constraint",
            severity: "ERROR",
            invariantSeverity: "ERROR",
            sqlState: "23502",
            columnName: columnName);
    }
}