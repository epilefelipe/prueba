using TicketManager.Domain.Entities;

namespace TicketManager.Application.Interfaces
{
    public interface ITicketRepository
    {
        Task<(List<Ticket> Items, int TotalCount)> GetPagedAsync(string? status, string? priority, string? q, int page, int pageSize);
        Task<Ticket?> GetByIdAsync(Guid id);
        Task AddAsync(Ticket ticket);
        Task UpdateAsync(Ticket ticket);
    }
}
