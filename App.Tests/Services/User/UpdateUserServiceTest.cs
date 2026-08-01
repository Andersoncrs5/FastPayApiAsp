using App.Config.Exceptions.Classes;
using App.Config.Security;
using App.Config.Security.Password;
using App.Config.uow;
using App.Modules.User.Dto.Requests;
using App.Modules.User.Mapper;
using App.Modules.User.Model;
using App.Modules.User.Repositories;
using App.Modules.User.Services.Provider;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Npgsql;
using Xunit;

namespace App.Tests.Services.User;

public sealed class UpdateUserServiceTest
{
    private readonly IUnitOfWork _uow;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly UserMapper _mapper;

    private readonly UpdateUserService _service;


    public UpdateUserServiceTest()
    {
        _uow = Substitute.For<IUnitOfWork>();

        _userRepository = Substitute.For<IUserRepository>();

        _passwordHasher = Substitute.For<IPasswordHasher>();

        _mapper = new UserMapper();

        _uow.UserRepository
            .Returns(_userRepository);
        
        _service = new UpdateUserService(
            _uow,
            _passwordHasher,
            _mapper);
    }


    [Fact]
    public async Task Execute_WhenUserExists_ReturnsSuccess()
    {
        long id = 1;

        var user = CreateUser();
        
        var dto = new UpdateUserDto(
            UserName: "new_username",
            Email: "new@email.com",
            FullName: "New Name",
            Password: null
        );

        _userRepository.GetByIdAsync(id)
            .Returns(user);

        _userRepository.UpdateAsync(Arg.Any<UserEntity>())
            .Returns(Task.CompletedTask);

        var result = await _service.Execute(id, dto);
        
        result.IsSuccess.Should().BeTrue();

        result.StatusCode.Should().Be(200);

        user.UserName.Should()
            .Be(dto.UserName);

        user.Email.Should()
            .Be(dto.Email);

        user.FullName.Should()
            .Be(dto.FullName);
        
        await _userRepository
            .Received(1)
            .UpdateAsync(
                Arg.Is<UserEntity>(x =>
                    x.Id == id &&
                    x.UserName == dto.UserName &&
                    x.Email == dto.Email &&
                    x.FullName == dto.FullName));
    }

    [Fact]
    public async Task Execute_WhenUserDoesNotExist_ReturnsNotFound()
    {
        long id = 999;


        var dto = new UpdateUserDto(
            "username",
            "email@test.com",
            "Name",
            null);



        _userRepository.GetByIdAsync(id)
            .Returns((UserEntity?)null);



        var result = await _service.Execute(id, dto);



        result.IsSuccess.Should()
            .BeFalse();


        result.StatusCode.Should()
            .Be(404);


        result.Errors.First()
            .Should()
            .Be("User not found");


        await _userRepository
            .DidNotReceive()
            .UpdateAsync(Arg.Any<UserEntity>());
    }
    
    [Fact]
    public async Task Execute_WhenPasswordProvided_ShouldHashPassword()
    {
        long id = 1;


        var user = CreateUser();


        var dto = new UpdateUserDto(
            "anderson",
            "anderson@test.com",
            "Anderson",
            "123456");



        _userRepository.GetByIdAsync(id)
            .Returns(user);


        _passwordHasher.Hash(dto.Password!)
            .Returns("new-password-hash");



        var result = await _service.Execute(id, dto);



        result.IsSuccess.Should()
            .BeTrue();


        user.PasswordHash.Should()
            .Be("new-password-hash");


        _passwordHasher
            .Received(1)
            .Hash(dto.Password!);
    }

    [Fact]
    public async Task Execute_WhenUsernameAlreadyExists_ShouldThrowConflictException()
    {
        long id = 1;

        var user = CreateUser();

        var dto = new UpdateUserDto(
            "duplicated",
            "email@test.com",
            "Name",
            null);
        
        _userRepository.GetByIdAsync(id)
            .Returns(user);
        _userRepository.UpdateAsync(Arg.Any<UserEntity>())
            .Throws(CreateUniqueViolation("ux_users_username"));

        var exception = await Assert.ThrowsAsync<DatabaseConflictException>(
            () => _service.Execute(id, dto));
        
        exception.Message
            .Should()
            .Be("ux_users_username");
    }
    
    [Fact]
    public async Task Execute_WhenEmailAlreadyExists_ShouldThrowConflictException()
    {
        long id = 1;

        var user = CreateUser();

        var dto = new UpdateUserDto(
            "username",
            "duplicated@email.com",
            "Name",
            null);
        
        _userRepository.GetByIdAsync(id)
            .Returns(user);
        
        _userRepository.UpdateAsync(Arg.Any<UserEntity>())
            .Throws(CreateUniqueViolation("ux_users_email"));
        
        var exception = await Assert.ThrowsAsync<DatabaseConflictException>(
            () => _service.Execute(id, dto));
        
        exception.Message
            .Should()
            .Be("ux_users_email");
    }
    
    [Fact]
    public async Task Execute_WhenRepositoryThrowsUnexpectedException_ShouldBubbleException()
    {
        long id = 1;
        
        var user = CreateUser();
        
        var dto = new UpdateUserDto(
            "username",
            "email@test.com",
            "Name",
            null);
        
        _userRepository.GetByIdAsync(id)
            .Returns(user);

        _userRepository.UpdateAsync(Arg.Any<UserEntity>())
            .Throws(new Exception("Database unavailable"));
        
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _service.Execute(id, dto));
        
        exception.Message
            .Should()
            .Be("Database unavailable");
    }
    
    private static UserEntity CreateUser()
    {
        return new UserEntity
        {
            Id = 1,
            UserName = "old_username",
            Email = "old@email.com",
            FullName = "Old Name",
            PasswordHash = "old_hash",
            Active = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }



    private static PostgresException CreateUniqueViolation(
        string constraintName)
    {
        return new PostgresException(
            messageText:
            "duplicate key value violates unique constraint",
            severity: "ERROR",
            invariantSeverity: "ERROR",
            sqlState: "23505",
            constraintName: constraintName);
    }
}