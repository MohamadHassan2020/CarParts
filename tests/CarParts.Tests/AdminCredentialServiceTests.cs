using CarParts.Web.Services;
using CarParts.Web.Settings;
using Microsoft.Extensions.Options;

namespace CarParts.Tests;

public class AdminCredentialServiceTests
{
    private static IAdminCredentialService Build(string username, string passwordHash) =>
        new AdminCredentialService(Options.Create(new AdminSettings
        {
            Username = username,
            PasswordHash = passwordHash
        }));

    private const string ValidHash = "GcKCPVdHNAIdAfueSyD5CQ==.puNVUG6Ak7arJTei4DZO9cCRBp3e2+klUGlXYTF2Esk=";

    [Fact]
    public void Verify_CorrectCredentials_ReturnsTrue()
    {
        var svc = Build("admin", ValidHash);
        Assert.True(svc.Verify("admin", "Admin123!"));
    }

    [Fact]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        var svc = Build("admin", ValidHash);
        Assert.False(svc.Verify("admin", "WrongPassword"));
    }

    [Fact]
    public void Verify_WrongUsername_ReturnsFalse()
    {
        var svc = Build("admin", ValidHash);
        Assert.False(svc.Verify("notadmin", "Admin123!"));
    }

    [Fact]
    public void Verify_MalformedHash_ReturnsFalse()
    {
        var svc = Build("admin", "notavalidhash");
        Assert.False(svc.Verify("admin", "Admin123!"));
    }
}
