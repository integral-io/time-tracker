namespace TimeTracker.Data.Models
{
    public class BillingClient
    {
        public int BillingClientId { get; set; }
        public string Name { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string CityStateZip { get; set; }
        public string Email { get; set; }
        public int ValueToCompany { get; set; }
    }
}