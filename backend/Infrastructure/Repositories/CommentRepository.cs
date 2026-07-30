using System.Data;
using Dapper;
using TicketManager.Application.Interfaces;
using TicketManager.Domain.Entities;

namespace TicketManager.Infrastructure.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly IDbConnection _db;

        public CommentRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<List<Comment>> GetByTicketIdAsync(Guid ticketId)
        {
            var sql = "SELECT * FROM Comment WHERE TicketId = @TicketId ORDER BY CreatedAt DESC";
            var comments = await _db.QueryAsync<Comment>(sql, new { TicketId = ticketId });
            return comments.ToList();
        }

        public async Task AddAsync(Comment comment)
        {
            var sql = @"
                INSERT INTO Comment (Id, TicketId, Text, CreatedAt, CreatedBy)
                VALUES (@Id, @TicketId, @Text, @CreatedAt, @CreatedBy)";
            await _db.ExecuteAsync(sql, comment);
        }
    }
}
