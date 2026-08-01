using App.Config.uow;
using App.Modules.Role.Model;
using App.Modules.Role.Repositories;
using App.Modules.Role.Services.Base;
using App.Modules.Role.Services.Provider;
using App.Utils.Result;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace App.Tests.Services.Role;

public sealed class FindRoleByIdServiceTest
{
    private readonly IUnitOfWork _uow;
    private readonly IRoleRepository _roleRepository;
    private readonly IFindRoleByIdService _service;

    private readonly RoleEntity _role = new()
    {
        Id = 1,
        Name = "Administrator",
        NormalizedName = "ADMINISTRATOR",
        Description = "System administrator",
        Active = true
    };

    public FindRoleByIdServiceTest()
    {
        _uow = Substitute.For<IUnitOfWork>();
        _roleRepository = Substitute.For<IRoleRepository>();

        _uow.RoleRepository.Returns(_roleRepository);

        _service = new FindRoleByIdService(_uow);
    }

    [Fact]
    public async Task FindByIdAsync_WhenRoleExists_ShouldReturnSuccess()
    {
        _roleRepository
            .GetByIdAsync(_role.Id)
            .Returns(_role);

        Result<RoleEntity> result =
            await _service.FindByIdAsync(_role.Id);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeEmpty();

        result.Value.Should().BeEquivalentTo(_role);

        await _roleRepository
            .Received(1)
            .GetByIdAsync(_role.Id);
    }

    [Fact]
    public async Task FindByIdAsync_WhenRoleDoesNotExist_ShouldReturnNotFound()
    {
        const long id = 999;

        _roleRepository
            .GetByIdAsync(id)
            .Returns((RoleEntity?)null);

        Result<RoleEntity> result =
            await _service.FindByIdAsync(id);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);

        result.Errors.Should()
            .Contain(e => e.Contains("Role not found"));

        await _roleRepository
            .Received(1)
            .GetByIdAsync(id);
    }

    [Fact]
    public async Task FindByIdAsync_WhenRepositoryThrowsException_ShouldBubbleUpException()
    {
        const long id = 10;

        var exception =
            new Exception("Database connection failure");

        _roleRepository
            .GetByIdAsync(id)
            .Throws(exception);

        Func<Task> act =
            () => _service.FindByIdAsync(id);

        await act.Should()
            .ThrowAsync<Exception>()
            .WithMessage("Database connection failure");

        await _roleRepository
            .Received(1)
            .GetByIdAsync(id);
    }
}