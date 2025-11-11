namespace EventManager.Api.Dtos
{
    public class VerifyPaymentDto
    {
        public string? Reference { get; set; }
        public string? Gateway { get; set; }
    }
}