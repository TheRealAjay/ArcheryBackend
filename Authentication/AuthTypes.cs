namespace ArcheryBackend.Authentication;

public class AuthResponse
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Token { get; set; } = null!;
    public string Base64String { get; set; } = "";
}

public class AuthRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}