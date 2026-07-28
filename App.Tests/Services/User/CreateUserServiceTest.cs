using App.Config.Security;
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

public sealed class CreateUserServiceTest
{
    private readonly IUnitOfWork _uow;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly UserMapper _mapper;
    private readonly CreateUserService _service;

    public CreateUserServiceTest()
    {
        _uow = Substitute.For<IUnitOfWork>();
        _userRepository = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _mapper = new UserMapper();

        _uow.UserRepository.Returns(_userRepository);

        _service = new CreateUserService(
            _uow,
            _passwordHasher,
            _mapper);
    }

    [Fact]
    public async Task Execute_WhenValidDto_ReturnsSuccess()
    {
        var dto = new CreateUserDto(
            UserName: "anderson",
            Email: "anderson@test.com",
            FullName: "Anderson Silva",
            Password: "123456");

        _passwordHasher.Hash(dto.Password)
            .Returns("hashed-password");

        _userRepository.CreateAsync(Arg.Any<UserEntity>())
            .Returns(Task.CompletedTask);

        var result = await _service.Execute(dto);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);

        _passwordHasher.Received(1).Hash(dto.Password);

        await _userRepository.Received(1).CreateAsync(
            Arg.Is<UserEntity>(user =>
                user.UserName == dto.UserName &&
                user.Email == dto.Email &&
                user.FullName == dto.FullName &&
                user.PasswordHash == "hashed-password" &&
                user.Active));
    }

    [Fact]
    public async Task Execute_WhenUsernameUniqueConstraint_ReturnsConflict()
    {
        var dto = new CreateUserDto(
            UserName: "anderson",
            Email: "anderson@test.com",
            FullName: "Anderson Silva",
            Password: "123456");

        _passwordHasher.Hash(dto.Password)
            .Returns("hashed-password");

        _userRepository.CreateAsync(Arg.Any<UserEntity>())
            .Throws(CreateUniqueViolation("ux_users_username"));

        var result = await _service.Execute(dto);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.Errors.First().Should().Be("Username already exists");

        _passwordHasher.Received(1).Hash(dto.Password);
        await _userRepository.Received(1).CreateAsync(Arg.Any<UserEntity>());
    }

    [Fact]
    public async Task Execute_WhenEmailUniqueConstraint_ReturnsConflict()
    {
        var dto = new CreateUserDto(
            UserName: "anderson",
            Email: "anderson@test.com",
            FullName: "Anderson Silva",
            Password: "123456");

        _passwordHasher.Hash(dto.Password)
            .Returns("hashed-password");

        _userRepository.CreateAsync(Arg.Any<UserEntity>())
            .Throws(CreateUniqueViolation("ux_users_email"));

        var result = await _service.Execute(dto);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.Errors.First().Should().Be("Email already exists");

        _passwordHasher.Received(1).Hash(dto.Password);
        await _userRepository.Received(1).CreateAsync(Arg.Any<UserEntity>());
    }

    [Fact]
    public async Task Execute_WhenUnexpectedException_BubblesUp()
    {
        var dto = new CreateUserDto(
            UserName: "anderson",
            Email: "anderson@test.com",
            FullName: "Anderson Silva",
            Password: "123456");

        _passwordHasher.Hash(dto.Password)
            .Returns("hashed-password");

        _userRepository.CreateAsync(Arg.Any<UserEntity>())
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
}