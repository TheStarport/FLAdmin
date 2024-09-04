
namespace FlAdmin.Common.Models.Auth;

public class AccessToken
{
    public AccessToken(string token, string userName, List<Role> roles)
    {
        Token = token;
        UserName = userName;
        Roles = roles;
    }
    
    public string Token { get; set; }
    public string UserName { get; set; }
    List<Role> Roles { get; set; }
}