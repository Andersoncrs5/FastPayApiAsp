using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using App.Config.Options;
using App.Modules.Auth.Dto.Responses;
using App.Modules.Role.Model;
using App.Modules.User.Model;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace App.Config.Security.Jwt;

public class AccessTokenResponse
{
    public required string Token { get; set; }
    public DateTime ExpireAt { get; set; }
}

public sealed class JwtService : IJwtService
{
    private readonly JwtOptions _options; 
    private readonly JwtSecurityTokenHandler _tokenHandler = new(); 
    public JwtService(IOptions<JwtOptions> options) { _options = options.Value; }

    public AccessTokenResponse CreateToken(UserEntity user, List<RoleEntity> roles)
    {
        ArgumentNullException.ThrowIfNull(user); 
        ArgumentNullException.ThrowIfNull(roles); 
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()), 
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty), 
            new(JwtRegisteredClaimNames.Name, user.UserName ?? string.Empty), 
            new("full_name", user.FullName), 
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Name));
        } 
        
        var key = new SymmetricSecurityKey( Encoding.UTF8.GetBytes(_options.SecretKey)); 
        var creds = new SigningCredentials( key, SecurityAlgorithms.HmacSha256);
        var exp = DateTime.UtcNow.AddMinutes(_options.TokenValidityInMinutes);
        
        var token = new JwtSecurityToken( 
            issuer: _options.ValidIssuer, 
            audience: _options.ValidAudience, 
            claims: claims, 
            expires: exp, 
            signingCredentials: creds
        ); 
        
        var accessToken = _tokenHandler.WriteToken(token);

        return new AccessTokenResponse
        {
            Token = accessToken,
            ExpireAt = exp
        };
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        ArgumentNullException.ThrowIfNull(token); 
        var principal = _tokenHandler.ValidateToken( token, CreateValidationParameters(validateLifetime: false), out var securityToken); if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals( SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase)) { throw new SecurityTokenException("Invalid token"); } return principal;
    }

    private TokenValidationParameters CreateValidationParameters(bool validateLifetime)
    {
        return new TokenValidationParameters { ValidateIssuer = true, ValidIssuer = _options.ValidIssuer, ValidateAudience = true, ValidAudience = _options.ValidAudience, ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey( Encoding.UTF8.GetBytes(_options.SecretKey)), ValidateLifetime = validateLifetime, ClockSkew = TimeSpan.Zero };
    }
}