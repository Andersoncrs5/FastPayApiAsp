using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using App.Config.Options;
using App.Config.Security.Jwt;
using App.Modules.Role.Model;
using App.Modules.User.Model;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace App.Tests.Security.Jwt;

public sealed class JwtServiceTest
{
    private readonly JwtOptions _options;
    private readonly JwtService _service;

    public JwtServiceTest()
    {
        _options = new JwtOptions
        {
             
            
            SecretKey = "uma-chave-super-secreta-muito-longa-para-testes-seguros",
            ValidIssuer = "FastPay",
            ValidAudience = "FastPayClients",
            TokenValidityInMinutes = 15,
            RefreshTokenValidityInMinutes = 60
        };

        
        _service = new JwtService(Options.Create(_options));
    }

    [Fact]
    public void CreateToken_WhenValidData_ShouldGenerateValidJwtToken()
    {
        
        var user = new UserEntity 
        { 
            Id = 10L, 
            UserName = "johndoe", 
            Email = "john@test.com", 
            FullName = "John Doe" 
        };
        
        var roles = new List<RoleEntity> 
        { 
            new() { Id = 1L, Name = "Admin", NormalizedName = "ADMIN" } 
        };

        
        var result = _service.CreateToken(user, roles);

        
        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrWhiteSpace();
        result.ExpireAt.Should().BeCloseTo(
            DateTime.UtcNow.AddMinutes(_options.TokenValidityInMinutes), 
            TimeSpan.FromSeconds(5)
        );

        
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(result.Token);

        jwtToken.Issuer.Should().Be(_options.ValidIssuer);
        jwtToken.Audiences.Should().Contain(_options.ValidAudience);
        
        
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == "10");
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "john@test.com");
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Name && c.Value == "johndoe");
        jwtToken.Claims.Should().Contain(c => c.Type == "full_name" && c.Value == "John Doe");
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Jti); 
    }

    [Fact]
    public void CreateToken_WhenUserIsNull_ShouldThrowArgumentNullException()
    {
        
        Action act = () => _service.CreateToken(null!, new List<RoleEntity>());

        
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("user");
    }

    [Fact]
    public void CreateToken_WhenRolesAreNull_ShouldThrowArgumentNullException()
    {
        
        var user = new UserEntity { Id = 1L, UserName = "johndoe" };

        
        Action act = () => _service.CreateToken(user, null!);

        
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("roles");
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_WhenTokenIsValid_ShouldReturnPrincipal()
    {
        
        var user = new UserEntity { Id = 10L, UserName = "johndoe", FullName = "John Doe", Email = "john@test.com" };
        var roles = new List<RoleEntity> { new() { Id = 1L, Name = "Admin", NormalizedName = "ADMIN" } };
        
        var tokenResponse = _service.CreateToken(user, roles);

        
        var principal = _service.GetPrincipalFromExpiredToken(tokenResponse.Token);
        
        principal.Should().NotBeNull();
        principal.Identity.Should().NotBeNull();
        principal.Identity!.IsAuthenticated.Should().BeTrue();
        
        principal.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == "Admin").Should().BeTrue();
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_WhenTokenIsMalformed_ShouldThrowException()
    {
        
        var invalidToken = "isso.nao.e_um_token_valido";

        
        Action act = () => _service.GetPrincipalFromExpiredToken(invalidToken);

        
        
        act.Should().Throw<ArgumentException>(); 
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_WhenTokenIsNull_ShouldThrowArgumentNullException()
    {
        
        Action act = () => _service.GetPrincipalFromExpiredToken(null!);

        
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("token");
    }
}