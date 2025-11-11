namespace EventManager.Api.Dtos
{
    public class ManualPaymentDto
    {
        public decimal Amount { get; set; }
        public string? ProofFileBase64 { get; set; } // Base64 encoded file content
        public string? ProofFileName { get; set; }   // Original file name
        public string? ContentType { get; set; }     // File content type (e.g., "image/jpeg")
    }
}