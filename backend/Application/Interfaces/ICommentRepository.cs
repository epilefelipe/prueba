using TicketManager.Domain.Entities;

namespace TicketManager.Application.Interfaces
{
    public interface ICommentRepository
    {
        Task<List<Comment>> GetByTicketIdAsync(Guid ticketId);
        Task AddAsync(Comment comment);
        Task SaveChangesAsync();
    }
}
