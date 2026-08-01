using App.Modules.Role.Model;
using App.Tests.Config;
using FluentAssertions;
using Xunit.Abstractions;

namespace App.Tests.Repositories;

public class RoleRepositoryTest : BaseIntegrationTest
{
    private readonly DatabaseFixture _fixture;
    private readonly ITestOutputHelper _output;

    public RoleRepositoryTest(DatabaseFixture fixture, ITestOutputHelper output) : base(fixture)
    {
        _output = output;
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateRole_ShouldCreateRole()
    {
        var role = await CreateRole();
        
        role.Should().NotBeNull();
        role.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ExistsById_Should_Return_False()
    {
        long id = Faker.Random.Long(100000, 999999);
        
        bool exists = await RoleRepository.ExistsByIdAsync(id);
        
        exists.Should().BeFalse();
    }
    
    [Fact]
    public async Task ExistsById_Should_Return_True()
    {
        var role = await CreateRole();
        
        bool exists = await RoleRepository.ExistsByIdAsync(role.Id);
        
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_ShouldDeleteRole()
    {
        var role = await CreateRole();
        
        await RoleRepository.DeleteAsync(role.Id);
        
        bool exists = await RoleRepository.ExistsByIdAsync(role.Id);
        
        exists.Should().BeFalse();
    }
    
    [Fact]
    public async Task DeleteAndCount_ShouldDeleteRole()
    {
        var role = await CreateRole();

        long count = await RoleRepository.DeleteAndCountAsync(role.Id);

        bool exists = await RoleRepository.ExistsByIdAsync(role.Id);
        
        exists.Should().BeFalse();
        count.Should().Be(1);
    }
    
    [Fact]
    public async Task Repository_Should_ExecuteAllBaseCrudOperationsSuccessfully()
    {
        var role = await CreateRole();

        var dbRole = await RoleRepository.GetByIdAsync(role.Id);
        var exists = await RoleRepository.ExistsByIdAsync(role.Id);

        dbRole.Should().NotBeNull();
        dbRole!.Id.Should().Be(role.Id);
        dbRole.Name.Should().Be(role.Name);
        dbRole.NormalizedName.Should().Be(role.NormalizedName);
        exists.Should().BeTrue();

        dbRole.Description = "New Description";
        dbRole.UpdatedAt = DateTime.UtcNow;
        
        await RoleRepository.UpdateAsync(dbRole);
        
        var updatedDbRole = await RoleRepository.GetByIdAsync(role.Id);
        updatedDbRole!.Description.Should().Be("New Description");

        var affectedRows = await RoleRepository.DeleteAndCountAsync(role.Id);
        var existsAfterDelete = await RoleRepository.ExistsByIdAsync(role.Id);

        affectedRows.Should().Be(1);
        existsAfterDelete.Should().BeFalse();
    }
    
    [Fact]
    public async Task ExistsByName_Should_Return_True_WhenNameHasSameCase()
    {
        var role = await CreateRole();

        bool exists = await RoleRepository.ExistsByNameAsync(
            role.NormalizedName);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByName_Should_Return_True_WhenNameHasDifferentCase()
    {
        var role = await CreateRole();

        bool exists = await RoleRepository.ExistsByNameAsync(role.NormalizedName);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByName_Should_Return_True_WhenNameIsLowerCase()
    {
        var role = await CreateRole();

        bool exists = await RoleRepository.ExistsByNameAsync(
            role.NormalizedName.ToLowerInvariant());

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByName_Should_Return_False_WhenNameDoesNotExist()
    {
        bool exists = await RoleRepository.ExistsByNameAsync(
            "role_that_does_not_exist");

        exists.Should().BeFalse();
    }
    
    [Fact]
    public async Task GetAllByIds_Should_Return_OnlyRequestedRoles()
    {
        var role1 = await CreateRole();
        var role2 = await CreateRole();
        var role3 = await CreateRole();

        var result = await RoleRepository.GetAllByIdsAsync(
        [
            role1.Id,
            role3.Id
        ]);

        result.Should().HaveCount(2);

        result.Select(r => r.Id)
            .Should()
            .BeEquivalentTo([role1.Id, role3.Id]);
    }

    [Fact]
    public async Task GetAllByIds_Should_Return_Empty_WhenIdsDoNotExist()
    {
        var ids = new List<long>
        {
            Faker.Random.Long(1_000_000, 2_000_000),
            Faker.Random.Long(2_000_001, 3_000_000)
        };

        var result = await RoleRepository.GetAllByIdsAsync(ids);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllByIds_Should_Return_Empty_WhenIdsListIsEmpty()
    {
        var result = await RoleRepository.GetAllByIdsAsync([]);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllByIds_Should_NotReturnDuplicateRoles()
    {
        var role = await CreateRole();

        var result = await RoleRepository.GetAllByIdsAsync(
        [
            role.Id,
            role.Id,
            role.Id
        ]);

        result.Should().HaveCount(1);
        result.First().Id.Should().Be(role.Id);
    }
    
}