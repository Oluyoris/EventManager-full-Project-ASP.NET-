namespace EventManager.Api.Dtos
{
    public class PaymentMethodDto
    {
        public string? BankName { get; set; }
        public string? AccountName { get; set; }
        public string? AccountNumber { get; set; }
        public bool IsManualActive { get; set; }
        public string? PaystackPublicKey { get; set; }
        public string? PaystackSecretKey { get; set; }
        public bool IsPaystackActive { get; set; }
        public string? FlutterwavePublicKey { get; set; }
        public string? FlutterwaveSecretKey { get; set; }
        public bool IsFlutterwaveActive { get; set; }
        public string? PaypalClientId { get; set; }
        public string? PaypalClientSecret { get; set; }
        public bool IsPaypalActive { get; set; }
    }
}