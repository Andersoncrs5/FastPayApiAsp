using App.Config.Exceptions.Classes;
using App.Config.uow;
using App.Modules.Role.Dto.Requests;
using App.Modules.Role.Mapper;
using App.Modules.Role.Model;
using App.Modules.Role.Repositories;
using App.Modules.Role.Services.Provider;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Npgsql;
using Xunit;

namespace App.Tests.Services.Role;

public sealed class CreateRoleServiceTest
{
    private readonly IUnitOfWork _uow;
    private readonly IRoleRepository _roleRepository;
    private readonly RoleMapper _mapper;
    private readonly CreateRoleService _service;

    public CreateRoleServiceTest()
    {
        _uow = Substitute.For<IUnitOfWork>();
        _roleRepository = Substitute.For<IRoleRepository>();
        _mapper = new RoleMapper();

        _uow.RoleRepository.Returns(_roleRepository);

        _service = new CreateRoleService(
            _uow,
            _mapper);
    }

    [Fact]
    public async Task Execute_WhenValidDto_ShouldCreateRole()
    {
        var dto = new CreateRoleDto(
            Name: "Admin",
            Description: "Administrator",
            NormalizedName: "Admin".ToUpper(),
            Active: true
                );

        _roleRepository.CreateAsync(Arg.Any<RoleEntity>())
            .Returns(Task.CompletedTask);

        var result = await _service.Execute(dto);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);

        await _roleRepository.Received(1).CreateAsync(
            Arg.Is<RoleEntity>(role =>
                role.Name == "Admin" &&
                role.NormalizedName == "ADMIN" &&
                role.Description == "Administrator" &&
                role.Active));
    }

    [Fact]
    public async Task Execute_WhenRoleAlreadyExists_ShouldThrowDatabaseConflictException()
    {
        var dto = new CreateRoleDto(
            Name: "Admin",
            Description: "Administrator",
            NormalizedName: "Admin".ToUpper(),
            Active: true);

        _roleRepository.CreateAsync(Arg.Any<RoleEntity>())
            .Throws(CreateUniqueViolation("ux_roles_name"));

        var ex = await Assert.ThrowsAsync<DatabaseConflictException>(
            () => _service.Execute(dto));

        ex.Message.Should().Contain("ux_roles_name");

        await _roleRepository.Received(1)
            .CreateAsync(Arg.Any<RoleEntity>());
    }

    [Fact]
    public async Task Execute_WhenNotNullViolation_ShouldThrowDatabaseValidationException()
    {
        var dto = new CreateRoleDto(
            Name: "Admin",
            Description: "Administrator",
            NormalizedName: "Admin".ToUpper(),
            Active: true);

        _roleRepository.CreateAsync(Arg.Any<RoleEntity>())
            .Throws(CreateNotNullViolation("name"));

        var ex = await Assert.ThrowsAsync<DatabaseValidationException>(
            () => _service.Execute(dto));

        ex.Message.Should().Contain("name");
    }

    [Fact]
    public async Task Execute_WhenUnexpectedException_ShouldBubbleUp()
    {
        var dto = new CreateRoleDto(
            Name: "Admin",
            Description: "Administrator",
            NormalizedName: "Admin".ToUpper(),
            Active: true);

        _roleRepository.CreateAsync(Arg.Any<RoleEntity>())
            .Throws(new Exception("Database connection failed"));

        var ex = await Assert.ThrowsAsync<Exception>(
            () => _service.Execute(dto));

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