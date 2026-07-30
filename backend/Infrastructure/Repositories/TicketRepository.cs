using System.Data;
using Dapper;
using TicketManager.Application.Interfaces;
using TicketManager.Domain.Entities;
using TicketManager.Domain.Enums;

namespace TicketManager.Infrastructure.Repositories
{
    public class EnumStringHandler<T> : SqlMapper.TypeHandler<T> where T : struct, Enum
    {
        public override T Parse(object value) => Enum.Parse<T>((string)value);
        public override void SetValue(IDbDataParameter parameter, T value) => parameter.Value = value.ToString();
    }

    public class GuidStringHandler : SqlMapper.TypeHandler<Guid>
    {
        public override Guid Parse(object value) => Guid.Parse((string)value);
        public override void SetValue(IDbDataParameter parameter, Guid value) => parameter.Value = value.ToString();
    }

    public class TicketRepository : ITicketRepository
    {
        private readonly IDbConnection _db;

        public TicketRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<(List<Ticket> Items, int TotalCount)> GetPagedAsync(
            string? status, string? priority, string? q, int page, int pageSize)
        {
            var where = new List<string>();
            var pars = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(status))
            {
                where.Add("t.Status = @Status");
                pars.Add("@Status", status);
            }
            if (!string.IsNullOrWhiteSpace(priority))
            {
                where.Add("t.Priority = @Priority");
                pars.Add("@Priority", priority);
            }
            if (!string.IsNullOrWhiteSpace(q))
            {
                where.Add("(t.Title LIKE @Q OR t.Description LIKE @Q)");
                pars.Add("@Q", $"%{q}%");
            }

            var sqlWhere = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "";

            var countSql = $"SELECT COUNT(*) FROM Ticket t {sqlWhere}";
            var totalCount = await _db.ExecuteScalarAsync<int>(countSql, pars);

            var dataSql = $@"
                SELECT t.Id, t.Title, t.Description, t.Priority, t.Status,
                       t.CreatedAt, t.UpdatedAt, t.CreatedBy,
                       c.Id, c.TicketId, c.Text, c.CreatedAt, c.CreatedBy
                FROM Ticket t
                LEFT JOIN Comment c ON c.TicketId = t.Id
                {sqlWhere}
                ORDER BY t.CreatedAt DESC
                LIMIT @Limit OFFSET @Offset";

            pars.Add("@Limit", pageSize);
            pars.Add("@Offset", (page - 1) * pageSize);

            var lookup = new Dictionary<Guid, Ticket>();
            await _db.QueryAsync<Ticket, Comment, Ticket>(dataSql, (ticket, comment) =>
            {
                if (!lookup.TryGetValue(ticket.Id, out var existing))
                    lookup.Add(ticket.Id, existing = ticket);
                if (comment != null && comment.Id != Guid.Empty)
                    existing.Comments.Add(comment);
                return existing;
            }, pars, splitOn: "Id");

            var items = lookup.Values.ToList();
            return (items, totalCount);
        }

        public async Task<Ticket?> GetByIdAsync(Guid id)
        {
            var sql = @"
                SELECT t.*, c.Id, c.TicketId, c.Text, c.CreatedAt, c.CreatedBy
                FROM Ticket t
                LEFT JOIN Comment c ON c.TicketId = t.Id
                WHERE t.Id = @Id
                ORDER BY c.CreatedAt DESC";

            var lookup = new Dictionary<Guid, Ticket>();
            await _db.QueryAsync<Ticket, Comment, Ticket>(sql, (ticket, comment) =>
            {
                if (!lookup.TryGetValue(ticket.Id, out var existing))
                    lookup.Add(ticket.Id, existing = ticket);
                if (comment != null && comment.Id != Guid.Empty)
                    existing.Comments.Add(comment);
                return existing;
            }, new { Id = id }, splitOn: "Id");

            return lookup.Values.FirstOrDefault();
        }

        public async Task AddAsync(Ticket ticket)
        {
            var sql = @"
                INSERT INTO Ticket (Id, Title, Description, Priority, Status, CreatedAt, UpdatedAt, CreatedBy)
                VALUES (@Id, @Title, @Description, @Priority, @Status, @CreatedAt, @UpdatedAt, @CreatedBy)";
            await _db.ExecuteAsync(sql, ticket);
        }

        public async Task UpdateAsync(Ticket ticket)
        {
            var sql = @"
                UPDATE Ticket
                SET Title = @Title, Description = @Description, Priority = @Priority,
                    Status = @Status, UpdatedAt = @UpdatedAt
                WHERE Id = @Id";
            await _db.ExecuteAsync(sql, ticket);
        }
    }
}
