namespace CarParts.Web.Services;

public interface IAdminCredentialService
{
    bool Verify(string username, string password);
}
