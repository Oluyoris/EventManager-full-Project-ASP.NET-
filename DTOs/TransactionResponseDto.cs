namespace EventManager.Api.Dtos
{
    public class TransactionResponseDto
    {
        public string? Message { get; set; }
        public TransactionDto? Transaction { get; set; }
    }
}