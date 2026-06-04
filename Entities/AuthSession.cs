namespace AuthService.Entities;

public class AuthSession
{
    public int id { get; set; }
    public required string session_code { get; set; }
    public long login_id { get; set; }
    public string? client_name { get; set; }
    public required string access_token { get; set; }
    public bool is_used { get; set; }
    public DateTime expires_date { get; set; }
    public DateTime create_date { get; set; }
}



