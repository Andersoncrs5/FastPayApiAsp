using App.Config.Exceptions.Classes;
using App.Config.uow;
using App.Modules.UserRole.Dto.Requests;
using App.Modules.UserRole.Mapper;
using App.Modules.UserRole.Model;
using App.Modules.UserRole.Repositories;
using App.Modules.UserRole.Services.Provider;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Npgsql;

namespace App.Tests.Services.UserRole;

public sealed class CreateUserRoleServiceTest
{
    private readonly IUnitOfWork _uow;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly UserRoleMapper _mapper;
    private readonly CreateUserRoleService _service;

    public CreateUserRoleServiceTest()
    {
        _uow = Substitute.For<IUnitOfWork>();
        _userRoleRepository = Substitute.For<IUserRoleRepository>();
        _mapper = new UserRoleMapper();

        _uow.UserRoleRepository.Returns(_userRoleRepository);

        _service = new CreateUserRoleService(
            _uow,
            null!,
            _mapper);
    }

    [Fact]
    public async Task Execute_WhenValidDto_ShouldCreateUserRole()
    {
        var dto = new CreateUserRoleDto(
            UserId: 10,
            RoleId: 20,
            Active: true,
            AssignedByUserId: 1);
        
        _userRoleRepository.CreateAsync(
                Arg.Any<UserRoleEntity>())
            .Returns(Task.CompletedTask);
        
        var result = await _service.Execute(dto);
        
        result.Should().NotBeNull();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);

        result.Value.Should().NotBeNull();

        await _userRoleRepository
            .Received(1)
            .CreateAsync(
                Arg.Is<UserRoleEntity>(entity =>
                    entity.UserId == dto.UserId &&
                    entity.RoleId == dto.RoleId &&
                    entity.Active == dto.Active &&
                    entity.AssignedByUserId == dto.AssignedByUserId));
    }

    [Fact]
    public async Task Execute_WhenUserAlreadyHasRole_ShouldThrowConflictException()
    {
        var dto = new CreateUserRoleDto(
            UserId: 10,
            RoleId: 20,
            Active: true,
            AssignedByUserId: 1);
        
        _userRoleRepository.CreateAsync(
                Arg.Any<UserRoleEntity>())
            .Throws(
                CreateUniqueViolation(
                    "ux_user_roles_user_role"));
        
        var act = async () =>
            await _service.Execute(dto);
        
        var exception = await act.Should()
            .ThrowAsync<DatabaseConflictException>();
        
        exception.Which.Message
            .Should()
            .Be("ux_user_roles_user_role");
        
        await _userRoleRepository
            .Received(1)
            .CreateAsync(
                Arg.Any<UserRoleEntity>());
    }
    
    [Fact]
    public async Task Execute_WhenDatabaseFails_ShouldBubbleException()
    {
        var dto = new CreateUserRoleDto(
            UserId: 10,
            RoleId: 20,
            Active: true,
            AssignedByUserId: 1);

        _userRoleRepository.CreateAsync(
                Arg.Any<UserRoleEntity>())
            .Throws(
                new Exception(
                    "Database connection failed"));
        
        var act = async () =>
            await _service.Execute(dto);
        
        await act.Should()
            .ThrowAsync<Exception>()
            .WithMessage(
                "Database connection failed");
        
        await _userRoleRepository
            .Received(1)
            .CreateAsync(
                Arg.Any<UserRoleEntity>());
    }

    [Fact]
    public async Task Execute_ShouldMapAllPropertiesCorrectly()
    {
        var dto = new CreateUserRoleDto(
            UserId: 99,
            RoleId: 77,
            Active: false,
            AssignedByUserId: 123);
        
        await _service.Execute(dto);
        
        await _userRoleRepository
            .Received(1)
            .CreateAsync(
                Arg.Is<UserRoleEntity>(entity =>
                    entity.UserId == 99 &&
                    entity.RoleId == 77 &&
                    entity.Active == false &&
                    entity.AssignedByUserId == 123));
    }

    private static PostgresException CreateUniqueViolation(
        string constraintName)
    {
        return new PostgresException(
            messageText:
            "duplicate key value violates unique constraint",
            severity:
            "ERROR",
            invariantSeverity:
            "ERROR",
            sqlState:
            "23505",
            constraintName:
            constraintName);
    }
}