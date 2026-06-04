namespace AuthService.Entities
{
    public class ProductMaster
    {
        public int product_id { get; set; }
        public string product_name { get; set; } = string.Empty;
        public string unq_code { get; set; } = string.Empty;
    }
}
