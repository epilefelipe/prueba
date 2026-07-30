using Microsoft.EntityFrameworkCore;
using TicketManager.Application.Interfaces;
using TicketManager.Domain.Entities;
using TicketManager.Infrastructure.Data;

namespace TicketManager.Infrastructure.Repositories
{
    public class TicketRepository : ITicketRepository
    {
        private readonly TicketDbContext _context;

        public TicketRepository(TicketDbContext context)
        {
            _context = context;
        }

        public IQueryable<Ticket> Query() => _context.Tickets.Include(t => t.Comments).AsQueryable();

        public async Task<Ticket?> GetByIdAsync(Guid id)
        {
            return await _context.Tickets.Include(t => t.Comments)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task AddAsync(Ticket ticket)
        {
            await _context.Tickets.AddAsync(ticket);
        }

        public void Update(Ticket ticket)
        {
            _context.Tickets.Update(ticket);
        }

        public Task SaveChangesAsync() => _context.SaveChangesAsync();
    }
}
