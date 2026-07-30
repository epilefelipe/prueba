using TicketManager.Domain.Entities;

namespace TicketManager.Application.Interfaces
{
    public interface ITicketRepository
    {
        IQueryable<Ticket> Query();
        Task<Ticket?> GetByIdAsync(Guid id);
        Task AddAsync(Ticket ticket);
        void Update(Ticket ticket);
        Task SaveChangesAsync();
    }
}
