namespace AuthService.Entities
{
    public class TranUserProductAccess
    {
        public int id { get; set; }
        public int login_id { get; set; }
        public int product_id { get; set; }
        public bool isactive { get; set; } = true;
        public DateTime create_date { get; set; } = DateTime.UtcNow;
        public DateTime? modify_date { get; set; }
    }
}
