namespace EventManager.Api.Dtos
{
    public class ApprovePaymentDto
    {
        public int TransactionId { get; set; }
        public bool IsApproved { get; set; }
    }
}