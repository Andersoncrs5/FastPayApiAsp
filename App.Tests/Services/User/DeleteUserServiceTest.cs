using App.Config.uow;
using App.Modules.User.Repositories;
using App.Modules.User.Services.Base;
using App.Modules.User.Services.Provider;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace App.Tests.Services.User;

public class DeleteUserServiceTest
{
    private readonly IUnitOfWork _uow;
    private readonly IUserRepository _userRepository;
    private readonly IDeleteUserService _service;

    public DeleteUserServiceTest()
    {
        _uow = Substitute.For<IUnitOfWork>();
        _userRepository = Substitute.For<IUserRepository>();

        _uow.UserRepository.Returns(_userRepository);

        _service = new DeleteUserService(_uow);
    }

    [Fact]
    public async Task DeleteUser_WhenUserExists_ReturnsSuccessResult()
    {
        long userId = 123L;
        int rowsAffected = 1;
        
        _userRepository.DeleteAndCountAsync(userId)
            .Returns(Task.FromResult(rowsAffected));

        var result = await _service.DeleteUser(userId);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        
        await _userRepository.Received(1).DeleteAndCountAsync(userId);
    }

    [Fact]
    public async Task DeleteUser_WhenUserDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        long userId = 999L;
        int rowsAffected = 0; 
        
        _userRepository.DeleteAndCountAsync(userId)
            .Returns(Task.FromResult(rowsAffected));

        var result = await _service.DeleteUser(userId);

        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode); 
        Assert.Equal("User not found", result.Errors.First()); 
        
        await _userRepository.Received(1).DeleteAndCountAsync(userId);
    }

    [Fact]
    public async Task DeleteUser_WhenRepositoryThrowsException_BubblesUpException()
    {
        long userId = 123L;
        var expectedException = new Exception("Database connection failed");
        
        _userRepository.DeleteAndCountAsync(userId)
            .Throws(expectedException);

        var exception = await Assert.ThrowsAsync<Exception>(() => _service.DeleteUser(userId));
        Assert.Equal("Database connection failed", exception.Message);
    }
}