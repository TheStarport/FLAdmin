namespace Common.Auth;
public class AdminUser
{
	public string Name { get; set; } = string.Empty;
	public string Token { get; set; } = string.Empty;
	public Role[] Roles { get; set; } = Array.Empty<Role>();
}
