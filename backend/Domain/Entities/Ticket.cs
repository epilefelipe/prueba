using TicketManager.Domain.Enums;

namespace TicketManager.Domain.Entities
{
    public class Ticket
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Priority Priority { get; set; }
        public Status Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;

        public List<Comment> Comments { get; set; } = new();

        public static readonly Dictionary<Status, List<Status>> ValidTransitions = new()
        {
            { Status.Open, new() { Status.InProgress, Status.Closed } },
            { Status.InProgress, new() { Status.Resolved, Status.Closed } },
            { Status.Resolved, new() { Status.Closed } },
            { Status.Closed, new() }
        };

        public bool CanTransitionTo(Status newStatus)
        {
            return ValidTransitions.TryGetValue(Status, out var allowed) && allowed.Contains(newStatus);
        }
    }
}
