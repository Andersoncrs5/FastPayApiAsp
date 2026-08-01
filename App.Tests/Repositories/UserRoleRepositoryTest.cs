using App.Modules.UserRole.Model;
using App.Tests.Config;
using FluentAssertions;
using Xunit.Abstractions;

namespace App.Tests.Repositories;

public sealed class UserRoleRepositoryTest : BaseIntegrationTest
{
    private readonly DatabaseFixture _fixture;
    private readonly ITestOutputHelper _output;

    public UserRoleRepositoryTest(
        DatabaseFixture fixture,
        ITestOutputHelper output) : base(fixture)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task CreateUserRole_ShouldCreateRelation()
    {
        var user = await CreateUser();
        var role = await CreateRole();

        UserRoleEntity userRole = await CreateUserRole(user.Id, role.Id);

        userRole.Should().NotBeNull();
        userRole.Id.Should().BeGreaterThan(0);
        userRole.UserId.Should().Be(user.Id);
        userRole.RoleId.Should().Be(role.Id);
        userRole.Active.Should().BeTrue();

        var dbUserRole = await UserRoleRepository.GetByUserIdAndRoleId(
            user.Id,
            role.Id);

        dbUserRole.Should().NotBeNull();
        dbUserRole!.UserId.Should().Be(user.Id);
        dbUserRole.RoleId.Should().Be(role.Id);
        dbUserRole.Active.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByUserIdAndRoleId_Should_Return_False_WhenRelationDoesNotExist()
    {
        var user = await CreateUser();
        var role = await CreateRole();

        bool exists = await UserRoleRepository.ExistsByUserIdAndRoleId(
            user.Id,
            role.Id);

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsByUserIdAndRoleId_Should_Return_True_WhenRelationExists()
    {
        var user = await CreateUser();
        var role = await CreateRole();

        await CreateUserRole(user.Id, role.Id);

        bool exists = await UserRoleRepository.ExistsByUserIdAndRoleId(
            user.Id,
            role.Id);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByUserIdAndRoleId_Should_Return_False_WhenRelationIsInactive()
    {
        var user = await CreateUser();
        var role = await CreateRole();

        var userRole = await CreateUserRole(user.Id, role.Id);

        userRole.Active = false;
        userRole.UpdatedAt = DateTimeOffset.UtcNow;

        await UserRoleRepository.UpdateAsync(userRole);

        bool exists = await UserRoleRepository.ExistsByUserIdAndRoleId(
            user.Id,
            role.Id);

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetByUserIdAndRoleId_Should_Return_Null_WhenRelationDoesNotExist()
    {
        var user = await CreateUser();
        var role = await CreateRole();

        var relation = await UserRoleRepository.GetByUserIdAndRoleId(
            user.Id,
            role.Id);

        relation.Should().BeNull();
    }

    [Fact]
    public async Task GetByUserIdAndRoleId_Should_Return_Relation_WhenItExists()
    {
        var user = await CreateUser();
        var role = await CreateRole();

        var created = await CreateUserRole(user.Id, role.Id);

        var relation = await UserRoleRepository.GetByUserIdAndRoleId(
            user.Id,
            role.Id);

        relation.Should().NotBeNull();
        relation!.Id.Should().Be(created.Id);
        relation.UserId.Should().Be(user.Id);
        relation.RoleId.Should().Be(role.Id);
        relation.Active.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_ShouldDeleteRelation()
    {
        var user = await CreateUser();
        var role = await CreateRole();

        var userRole = await CreateUserRole(user.Id, role.Id);

        await UserRoleRepository.DeleteAsync(userRole.Id);

        bool exists = await UserRoleRepository.ExistsByUserIdAndRoleId(
            user.Id,
            role.Id);

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAndCount_ShouldDeleteRelation()
    {
        var user = await CreateUser();
        var role = await CreateRole();

        var userRole = await CreateUserRole(user.Id, role.Id);

        int count = await UserRoleRepository.DeleteAndCountAsync(userRole.Id);

        bool exists = await UserRoleRepository.ExistsByUserIdAndRoleId(
            user.Id,
            role.Id);

        exists.Should().BeFalse();
        count.Should().Be(1);
    }

    [Fact]
    public async Task Repository_Should_ExecuteAllBaseCrudOperationsSuccessfully()
    {
        var user = await CreateUser();
        var role = await CreateRole();

        var userRole = await CreateUserRole(user.Id, role.Id);

        var dbUserRole = await UserRoleRepository.GetByIdAsync(userRole.Id);
        var exists = await UserRoleRepository.ExistsByIdAsync(userRole.Id);

        dbUserRole.Should().NotBeNull();
        dbUserRole!.Id.Should().Be(userRole.Id);
        dbUserRole.UserId.Should().Be(user.Id);
        dbUserRole.RoleId.Should().Be(role.Id);
        exists.Should().BeTrue();

        dbUserRole.Active = false;
        dbUserRole.UpdatedAt = DateTimeOffset.UtcNow;

        await UserRoleRepository.UpdateAsync(dbUserRole);

        var updated = await UserRoleRepository.GetByIdAsync(userRole.Id);
        updated.Should().NotBeNull();
        updated!.Active.Should().BeFalse();

        var affectedRows = await UserRoleRepository.DeleteAndCountAsync(userRole.Id);
        var existsAfterDelete = await UserRoleRepository.ExistsByIdAsync(userRole.Id);

        affectedRows.Should().Be(1);
        existsAfterDelete.Should().BeFalse();
    }

    
}