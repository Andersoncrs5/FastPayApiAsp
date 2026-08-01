using App.Config.uow;
using App.Modules.User.Model;
using App.Modules.User.Repositories;
using App.Modules.User.Services.Base;
using App.Modules.User.Services.Provider;
using App.Utils.Result;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace App.Tests.Services.User;

public class FindUserByIdServiceTest
{
    private readonly IUnitOfWork _uow;
    private readonly IUserRepository _userRepository;
    private readonly IFindUserByIdService _service; 
    private readonly UserEntity _user = new() { Id = 123, UserName = "John", Email = "john@gmail.com" };
    
    public FindUserByIdServiceTest()
    {
        _uow = Substitute.For<IUnitOfWork>();
        _userRepository = Substitute.For<IUserRepository>();

        _uow.UserRepository.Returns(_userRepository);

        _service = new FindUserByIdService(_uow);
    }

    [Fact]
    public async Task Execute_WhenUserExists_ShouldReturnSuccessWithUser()
    {
        // Arrange
        _userRepository.GetByIdAsync(_user.Id).Returns(_user);

        // Act
        Result<UserEntity> result = await _service.Execute(_user.Id);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Value.Should().BeEquivalentTo(_user);

        await _userRepository.Received(1).GetByIdAsync(_user.Id);
    }

    [Fact]
    public async Task Execute_WhenUserDoesNotExist_ShouldReturnNotFoundError()
    {
        long targetId = 999L;
        _userRepository.GetByIdAsync(targetId).Returns((UserEntity?)null);

        // Act
        Result<UserEntity> result = await _service.Execute(targetId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        
        result.Errors.Should().Contain(e => e.Contains("User not found")); 
        
        await _userRepository.Received(1).GetByIdAsync(targetId);
    }

    [Fact]
    public async Task Execute_WhenRepositoryThrowsException_ShouldBubbleUpException()
    {
        // Arrange
        long targetId = 123L;
        var expectedException = new Exception("Database failure connection");
        _userRepository.GetByIdAsync(targetId).Throws(expectedException);

        // Act
        Func<Task> act = async () => await _service.Execute(targetId);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Database failure connection");

        await _userRepository.Received(1).GetByIdAsync(targetId);
    }
}