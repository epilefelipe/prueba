using Microsoft.EntityFrameworkCore;
using TicketManager.Application.Interfaces;
using TicketManager.Domain.Entities;
using TicketManager.Infrastructure.Data;

namespace TicketManager.Infrastructure.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly TicketDbContext _context;

        public CommentRepository(TicketDbContext context)
        {
            _context = context;
        }

        public async Task<List<Comment>> GetByTicketIdAsync(Guid ticketId)
        {
            return await _context.Comments
                .Where(c => c.TicketId == ticketId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(Comment comment)
        {
            await _context.Comments.AddAsync(comment);
        }

        public Task SaveChangesAsync() => _context.SaveChangesAsync();
    }
}
