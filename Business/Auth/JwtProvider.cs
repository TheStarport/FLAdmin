using Common.Auth;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Business.Auth;
public class JwtProvider : IJwtProvider
{
    private readonly IKeyProvider _keyProvider;

    public JwtProvider(IKeyProvider keyProvider)
    {
        _keyProvider = keyProvider;
    }

    public string? DecryptToken(string token)
    {
        var signingKey = _keyProvider.GetSigningKey();
        var encryptionKey = _keyProvider.GetEncryptionKey();

        // Verification
        var tokenValidationParameters = new TokenValidationParameters()
        {
            ValidIssuers = new string[] { "FLAdmin" },
            IssuerSigningKey = new SymmetricSecurityKey(signingKey),
            TokenDecryptionKey = new SymmetricSecurityKey(encryptionKey),
            ValidateAudience = false,
        };

        var handler = new JwtSecurityTokenHandler();

        try
        {
            handler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
            return validatedToken.ToString();
        }
        catch (Exception)
        {
            return null;
        }
    }

    public string GenerateToken(string name)
    {
        var signingKey = _keyProvider.GetSigningKey();
        var encryptionKey = _keyProvider.GetEncryptionKey();

        var claimName = new Claim(ClaimTypes.Name, name);

        var identity = new ClaimsIdentity(new[] { claimName }, "jwt");

        var descriptor = new SecurityTokenDescriptor()
        {
            Subject = identity,
            IssuedAt = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddYears(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(signingKey), SecurityAlgorithms.HmacSha256),
            Issuer = "FLAdmin",
            EncryptingCredentials = new(new SymmetricSecurityKey(encryptionKey), SecurityAlgorithms.Aes256KW, SecurityAlgorithms.Aes256CbcHmacSha512)
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateJwtSecurityToken(descriptor);

        return handler.WriteToken(token);
    }
}
