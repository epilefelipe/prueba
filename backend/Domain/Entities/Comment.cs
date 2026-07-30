namespace TicketManager.Domain.Entities
{
    public class Comment
    {
        public Guid Id { get; set; }
        public Guid TicketId { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;

        public Ticket Ticket { get; set; } = null!;
    }
}
