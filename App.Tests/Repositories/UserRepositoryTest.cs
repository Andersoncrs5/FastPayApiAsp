using App.Config.Database;
using App.Modules.User.Model;
using App.Modules.User.Repositories;
using App.Tests.Config; 
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace App.Tests.Repositories;

public class UserRepositoryTest: BaseIntegrationTest
{
    private readonly DatabaseFixture _fixture;
    private readonly ITestOutputHelper _output;

    public UserRepositoryTest(DatabaseFixture fixture, ITestOutputHelper output) : base(fixture)
    {
        _output = output;
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateUser_ShouldCreateUser()
    {
        await CreateUser();
    }
    
    [Fact]
    public async Task ExistsById_Should_Return_False()
    {
        long id = _faker.Random.Long(100000, 999999);
        
        bool exists = await _userRepository.ExistsByIdAsync(id);
        
        exists.Should().BeFalse();
    }
    
    [Fact]
    public async Task ExistsById_Should_Return_True()
    {
        var user = await CreateUser();
        
        bool exists = await _userRepository.ExistsByIdAsync(user.Id);
        
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_ShouldDeleteUser()
    {
        var user = await CreateUser();
        
        await _userRepository.DeleteAsync(user.Id);
        
        bool exists = await _userRepository.ExistsByIdAsync(user.Id);
        
        exists.Should().BeFalse();
    }
    
    [Fact]
    public async Task DeleteAndCount_ShouldDeleteUser()
    {
        var user = await CreateUser();

        long count = await _userRepository.DeleteAndCountAsync(user.Id);

        bool exists = await _userRepository.ExistsByIdAsync(user.Id);
        
        exists.Should().BeFalse();
        count.Should().Be(1);
    }
    
    [Fact]
    public async Task Repository_Should_ExecuteAllBaseCrudOperationsSuccessfully()
    {
        var user = await CreateUser();

        // ====================================================================
        // 2. TESTAR GET BY ID & EXISTS
        // ====================================================================
        var dbUser = await _userRepository.GetByIdAsync(user.Id);
        var exists = await _userRepository.ExistsByIdAsync(user.Id);

        dbUser.Should().NotBeNull();
        dbUser!.Id.Should().Be(user.Id);
        dbUser.Email.Should().Be(user.Email);
        dbUser.FullName.Should().Be(user.FullName);
        exists.Should().BeTrue();

        // ====================================================================
        // 3. TESTAR UPDATE
        // ====================================================================
        dbUser.FullName = "Nome Alterado via Teste";
        dbUser.UpdatedAt = DateTime.UtcNow;
        
        await _userRepository.UpdateAsync(dbUser);
        
        var updatedDbUser = await _userRepository.GetByIdAsync(user.Id);
        updatedDbUser!.FullName.Should().Be("Nome Alterado via Teste");

        // ====================================================================
        // 4. TESTAR DELETE AND COUNT
        // ====================================================================
        var affectedRows = await _userRepository.DeleteAndCountAsync(user.Id);
        var existsAfterDelete = await _userRepository.ExistsByIdAsync(user.Id);

        affectedRows.Should().Be(1); // 1 linha deletada
        existsAfterDelete.Should().BeFalse();
    }
}