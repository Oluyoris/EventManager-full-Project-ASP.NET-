namespace EventManager.Api.Dtos
{
    public class PaymentResponseDto
    {
        public string? Reference { get; set; }
        public string? PaymentUrl { get; set; }
        public string? Status { get; set; }
    }
}