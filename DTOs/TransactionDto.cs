namespace EventManager.Api.Dtos
{
    public class TransactionDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? EventId { get; set; }
        public decimal Amount { get; set; }
        public string? Gateway { get; set; }
        public string? Status { get; set; }
        public string? Reference { get; set; }
        public string? ProofFilePath { get; set; }
        public string? PaymentUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string? UserFullName { get; set; } // Added for frontend
        public string? UserUsername { get; set; } // Added for frontend
    }
}