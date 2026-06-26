using System.Security.Cryptography;
using System.Text;
using CarParts.Web.Settings;
using Microsoft.Extensions.Options;

namespace CarParts.Web.Services;

public class AdminCredentialService(IOptions<AdminSettings> options) : IAdminCredentialService
{
    public bool Verify(string username, string password)
    {
        var settings = options.Value;
        if (!string.Equals(username, settings.Username, StringComparison.OrdinalIgnoreCase))
            return false;

        var parts = settings.PasswordHash.Split('.');
        if (parts.Length != 2) return false;

        byte[] salt, storedHash;
        try
        {
            salt       = Convert.FromBase64String(parts[0]);
            storedHash = Convert.FromBase64String(parts[1]);
        }
        catch (FormatException)
        {
            return false;
        }

        var inputHash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password), salt, 100_000, HashAlgorithmName.SHA256, 32);

        return CryptographicOperations.FixedTimeEquals(inputHash, storedHash);
    }
}
