using App.Config.Exceptions.Classes;
using App.Config.Security.Jwt;
using App.Modules.Auth.Dto.Responses;
using App.Modules.Auth.Gateway;
using App.Modules.Auth.Services.Provider;
using App.Modules.RefreshToken.Dto.Responses;
using App.Modules.RefreshToken.Services.Base;
using App.Modules.Role.Model;
using App.Modules.Role.Services.Base;
using App.Modules.User.Dto.Requests;
using App.Modules.User.Model;
using App.Modules.User.Services.Base;
using App.Modules.UserRole.Services.Base;
using App.Utils.Result;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace App.Tests.Services.Auth;

public sealed class RegisterServiceTest
{
    private readonly ICreateUserService _createUser;
    private readonly IJwtService _jwtService;
    private readonly IFindRoleIdsByUserIdService _findAllUseRoleByUserIdService;
    private readonly IFindRolesByIdsService _findRolesByIds;
    private readonly ICreateRefreshTokenService _createRefreshTokenService;
    
    private readonly AuthGateway _gateway;
    private readonly RegisterService _service;

    public RegisterServiceTest()
    {
        _createUser = Substitute.For<ICreateUserService>();
        _jwtService = Substitute.For<IJwtService>();
        _findAllUseRoleByUserIdService = Substitute.For<IFindRoleIdsByUserIdService>();
        _findRolesByIds = Substitute.For<IFindRolesByIdsService>();
        _createRefreshTokenService = Substitute.For<ICreateRefreshTokenService>();

        _gateway = new AuthGateway(
            _createUser,
            _jwtService,
            _findAllUseRoleByUserIdService,
            _findRolesByIds,
            _createRefreshTokenService
        );

        _service = new RegisterService(_gateway);
    }

    [Fact]
    public async Task Execute_WhenRegistrationIsValid_ShouldReturnTokens()
    {
        var dto = new CreateUserDto("john_doe", "john@test.com", "John Doe", "password123");
        
        var mockUser = new UserEntity { Id = 10L, UserName = dto.UserName, Email = dto.Email, FullName = dto.FullName, PasswordHash = "hash" };
        var mockRoleIds = new List<long> { 1L };
        var mockRoles = new List<RoleEntity> { new() { Id = 1L, Name = "User", NormalizedName = "USER" } };
        
        var mockAccessToken = new AccessTokenResponse { Token = "access-token-123", ExpireAt = DateTime.UtcNow.AddHours(1) };
        var mockRefreshToken = new RefreshTokenResult("refresh-token-123", DateTimeOffset.UtcNow.AddDays(7));

        _createUser.Execute(dto)
            .Returns(Result<UserEntity>.Success(mockUser, 201));

        _findAllUseRoleByUserIdService.Execute(mockUser.Id)
            .Returns(Result<List<long>>.Success(mockRoleIds));

        _findRolesByIds.Execute(mockRoleIds)
            .Returns(Result<List<RoleEntity>>.Success(mockRoles));

        _jwtService.CreateToken(mockUser, mockRoles)
            .Returns(mockAccessToken);

        _createRefreshTokenService.Execute(mockUser.Id)
            .Returns(Result<RefreshTokenResult>.Success(mockRefreshToken));

        var result = await _service.Execute(dto);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        
        result.Value.Token.Should().Be(mockAccessToken.Token);
        result.Value.ExpiresAccessToken.Should().Be(mockAccessToken.ExpireAt);
        result.Value.RefreshToken.Should().Be(mockRefreshToken.Token);
        result.Value.ExpiresRefreshToken.Should().Be(mockRefreshToken.ExpiresAt);

        await _createUser.Received(1).Execute(dto);
        await _findAllUseRoleByUserIdService.Received(1).Execute(mockUser.Id);
        await _findRolesByIds.Received(1).Execute(mockRoleIds);
        _jwtService.Received(1).CreateToken(mockUser, mockRoles);
        await _createRefreshTokenService.Received(1).Execute(mockUser.Id);
    }

    [Fact]
    public async Task Execute_WhenGetRolesFails_ShouldReturnFailureWithoutCreatingTokens()
    {
        var dto = new CreateUserDto("john_doe", "john@test.com", "John Doe", "password123");
        var mockUser = new UserEntity { Id = 10L, UserName = dto.UserName };

        _createUser.Execute(dto)
            .Returns(Result<UserEntity>.Success(mockUser, 201));

        var expectedErrors = new List<string> { "Failed to retrieve user role relationships" };
        _findAllUseRoleByUserIdService.Execute(mockUser.Id)
            .Returns(Result<List<long>>.Failure(expectedErrors, 400));

        var result = await _service.Execute(dto);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Errors.Should().BeEquivalentTo(expectedErrors);

        _jwtService.DidNotReceiveWithAnyArgs().CreateToken(default!, default!);
        await _createRefreshTokenService.DidNotReceiveWithAnyArgs().Execute(default);
    }

    [Fact]
    public async Task Execute_WhenUserCreationFails_ShouldBubbleExceptionViaUnwrapOrThrow()
    {
        var dto = new CreateUserDto("john_doe", "john@test.com", "John Doe", "password123");
        var expectedErrors = new List<string> { "Email already exists" };

        _createUser.Execute(dto)
            .Returns(Result<UserEntity>.Failure(expectedErrors, 409));

        var act = async () => await _service.Execute(dto);

        var exception = await act.Should().ThrowAsync<ResultException>();
        exception.Which.StatusCode.Should().Be(409);
        exception.Which.Errors.Should().BeEquivalentTo(expectedErrors);

        await _findAllUseRoleByUserIdService.DidNotReceiveWithAnyArgs().Execute(default);
    }
}