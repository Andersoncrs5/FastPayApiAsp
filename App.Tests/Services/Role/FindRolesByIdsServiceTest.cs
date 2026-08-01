using App.Config.uow;
using App.Modules.Role.Model;
using App.Modules.Role.Repositories;
using App.Modules.Role.Services.Base;
using App.Modules.Role.Services.Provider;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace App.Tests.Services.Role;

public sealed class FindRolesByIdsServiceTest
{
    private readonly IUnitOfWork _uow;
    private readonly IRoleRepository _roleRepository;
    private readonly IFindRolesByIdsService _service;

    public FindRolesByIdsServiceTest()
    {
        _uow = Substitute.For<IUnitOfWork>();
        _roleRepository = Substitute.For<IRoleRepository>();

        _uow.RoleRepository.Returns(_roleRepository);

        _service = new FindRolesByIdsService(_uow);
    }

    [Fact]
    public async Task FindByIdsAsync_WhenIdsAreValid_ReturnsSuccessWithRoles()
    {
        var ids = new List<long> { 1, 2 };

        var roles = new List<RoleEntity>
        {
            new()
            {
                Id = 1,
                Name = "Admin",
                NormalizedName = "ADMIN",
                Description = "Administrator role",
                Active = true,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = 2,
                Name = "User",
                NormalizedName = "USER",
                Description = "Default role",
                Active = true,
                CreatedAt = DateTimeOffset.UtcNow
            }
        };

        _roleRepository.GetAllByIdsAsync(ids)
            .Returns(roles);

        var result = await _service.Execute(ids);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Value.Should().BeEquivalentTo(roles);

        await _roleRepository.Received(1)
            .GetAllByIdsAsync(ids);
    }

    [Fact]
    public async Task FindByIdsAsync_WhenIdsListIsEmpty_ReturnsEmptySuccess()
    {
        var ids = new List<long>();

        var result = await _service.Execute(ids);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();

        await _roleRepository.DidNotReceive()
            .GetAllByIdsAsync(Arg.Any<List<long>>());
    }

    [Fact]
    public async Task FindByIdsAsync_WhenRepositoryThrowsException_BubblesUp()
    {
        var ids = new List<long> { 1, 2 };

        _roleRepository.GetAllByIdsAsync(ids)
            .Throws(new Exception("Database connection failed"));

        var ex = await Assert.ThrowsAsync<Exception>(
            () => _service.Execute(ids));

        ex.Message.Should().Be("Database connection failed");

        await _roleRepository.Received(1)
            .GetAllByIdsAsync(ids);
    }
}