namespace AuthService.Data;

public class RegisterRequestDto
{
    public required string username { get; set; }
    public required string email { get; set; }
    public string? phone { get; set; }
    public required string password { get; set; }
}
public class LoginRequestDto
{
    public required string identifier { get; set; }
    public required string password { get; set; }
}

public class AuthResponse
{
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
}

public class VerifiedAuthResponse
{
    public int LoginId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public List<ProductMap>? ProductsList { get; set; } = new();
}
public class ProductMap
{
    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;
}

public class ApiResponse
{
    public int Code { get; set; }
    public bool Success { get; set; } 
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; } 

}

public class ExchangeCodeRequestDto
{
    public required string code { get; set; }
}