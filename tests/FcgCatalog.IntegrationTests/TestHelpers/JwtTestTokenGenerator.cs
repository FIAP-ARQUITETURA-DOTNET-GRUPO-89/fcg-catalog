using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FcgCatalog.SharedKernel.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FcgCatalog.IntegrationTests.TestHelpers;

public class JwtTestTokenGenerator(IOptions<JwtSettings> settings)
{
    private readonly JwtSettings _settings = settings.Value;

    // Gera um Guid consistente baseado no hash do e-mail para que o ID seja sempre o mesmo para o mesmo usuário nos testes
    public static Guid GetDeterministicId(string email)
    {
        using var provider = MD5.Create();
        byte[] inputBytes = Encoding.UTF8.GetBytes(email.ToLowerInvariant());
        byte[] hashBytes = provider.ComputeHash(inputBytes);
        return new Guid(hashBytes);
    }

    public string Generate(string email, string role)
    {
        var userId = GetDeterministicId(email);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Name, email),
            new(ClaimTypes.Role, role)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_settings.SecurityKey));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: null,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_settings.ExpirationHours),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
