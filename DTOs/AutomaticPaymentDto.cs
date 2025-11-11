namespace EventManager.Api.Dtos
{
    public class AutomaticPaymentDto
    {
        public decimal Amount { get; set; }
        public string? Gateway { get; set; } // Paystack, Flutterwave, Paypal
        public string? Email { get; set; }
    }
}