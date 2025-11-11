using System;

namespace EventManager.Api.Models
{
    public enum TransactionGateway { Manual, Paystack, Flutterwave, Paypal }
    public enum TransactionStatus { Pending, Completed, Failed }
    public class Transaction
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public int? EventId { get; set; }
        public Event? Event { get; set; }
        public decimal Amount { get; set; }
        public TransactionGateway Gateway { get; set; }
        public TransactionStatus Status { get; set; }
        public string? Reference { get; set; }
        public string? ProofFilePath { get; set; }
        public string? PaymentUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}