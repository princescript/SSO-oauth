namespace AuthService.Entities;

public class UserMaster
{
    public int login_id { get; set; }
    public string user_name { get; set; } = string.Empty;
    public string email { get; set; } = string.Empty;
    public string? phone { get; set; }
    public string password_hash { get; set; } = string.Empty;
    public int? role { get; set; }
    public bool isactive { get; set; } = true;
    public DateTime create_date { get; set; }
    public int? create_by { get; set; }
    public DateTime? modify_date { get; set; }
    public int? modify_by { get; set; }
    public string? ip_address { get; set; }
}
