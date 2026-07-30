using FluentValidation;
using TicketManager.Application.DTOs;
using TicketManager.Application.Interfaces;
using TicketManager.Domain.Entities;
using TicketManager.Domain.Enums;

namespace TicketManager.Application.Services
{
    public class TicketService
    {
        private readonly ITicketRepository _ticketRepo;
        private readonly ICommentRepository _commentRepo;
        private readonly IValidator<CreateTicketDto> _createValidator;
        private readonly IValidator<UpdateTicketDto> _updateValidator;
        private readonly IValidator<UpdateStatusDto> _statusValidator;
        private readonly IValidator<CreateCommentDto> _commentValidator;
        private readonly ILogger<TicketService> _logger;

        public TicketService(
            ITicketRepository ticketRepo,
            ICommentRepository commentRepo,
            IValidator<CreateTicketDto> createValidator,
            IValidator<UpdateTicketDto> updateValidator,
            IValidator<UpdateStatusDto> statusValidator,
            IValidator<CreateCommentDto> commentValidator,
            ILogger<TicketService> logger)
        {
            _ticketRepo = ticketRepo;
            _commentRepo = commentRepo;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _statusValidator = statusValidator;
            _commentValidator = commentValidator;
            _logger = logger;
        }

        public async Task<PagedResult<TicketListItemDto>> GetTicketsAsync(
            string? status, string? priority, string? q, int page = 1, int pageSize = 10)
        {
            var (tickets, totalCount) = await _ticketRepo.GetPagedAsync(status, priority, q, page, pageSize);

            var items = tickets.Select(t => new TicketListItemDto(
                t.Id, t.Title, t.Priority.ToString(),
                t.Status.ToString(), t.CreatedAt, t.CreatedBy,
                t.Comments.Count)).ToList();

            return new PagedResult<TicketListItemDto>(items, totalCount, page, pageSize);
        }

        public async Task<TicketDto?> GetByIdAsync(Guid id)
        {
            var ticket = await _ticketRepo.GetByIdAsync(id);
            if (ticket == null) return null;

            return new TicketDto(
                ticket.Id, ticket.Title, ticket.Description,
                ticket.Priority.ToString(), ticket.Status.ToString(),
                ticket.CreatedAt, ticket.UpdatedAt, ticket.CreatedBy,
                ticket.Comments.Count);
        }

        public async Task<TicketDto> CreateAsync(CreateTicketDto dto)
        {
            await _createValidator.ValidateAndThrowAsync(dto);

            var ticket = new Ticket
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                Priority = Enum.Parse<Priority>(dto.Priority),
                Status = Status.Open,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = dto.CreatedBy
            };

            await _ticketRepo.AddAsync(ticket);

            _logger.LogInformation("Ticket {Id} created by {User}", ticket.Id, dto.CreatedBy);

            return new TicketDto(
                ticket.Id, ticket.Title, ticket.Description,
                ticket.Priority.ToString(), ticket.Status.ToString(),
                ticket.CreatedAt, ticket.UpdatedAt, ticket.CreatedBy, 0);
        }

        public async Task<TicketDto?> UpdateAsync(Guid id, UpdateTicketDto dto)
        {
            await _updateValidator.ValidateAndThrowAsync(dto);

            var ticket = await _ticketRepo.GetByIdAsync(id);
            if (ticket == null) return null;

            ticket.Title = dto.Title;
            ticket.Description = dto.Description;
            ticket.Priority = Enum.Parse<Priority>(dto.Priority);
            ticket.UpdatedAt = DateTime.UtcNow;

            await _ticketRepo.UpdateAsync(ticket);

            _logger.LogInformation("Ticket {Id} updated", id);

            return new TicketDto(
                ticket.Id, ticket.Title, ticket.Description,
                ticket.Priority.ToString(), ticket.Status.ToString(),
                ticket.CreatedAt, ticket.UpdatedAt, ticket.CreatedBy,
                ticket.Comments.Count);
        }

        public async Task<TicketDto?> UpdateStatusAsync(Guid id, UpdateStatusDto dto)
        {
            await _statusValidator.ValidateAndThrowAsync(dto);

            var ticket = await _ticketRepo.GetByIdAsync(id);
            if (ticket == null) return null;

            var newStatus = Enum.Parse<Status>(dto.Status);

            if (!ticket.CanTransitionTo(newStatus))
                throw new InvalidOperationException(
                    $"Cannot transition from {ticket.Status} to {newStatus}");

            ticket.Status = newStatus;
            ticket.UpdatedAt = DateTime.UtcNow;

            await _ticketRepo.UpdateAsync(ticket);

            _logger.LogInformation("Ticket {Id} status changed to {Status}", id, dto.Status);

            return new TicketDto(
                ticket.Id, ticket.Title, ticket.Description,
                ticket.Priority.ToString(), ticket.Status.ToString(),
                ticket.CreatedAt, ticket.UpdatedAt, ticket.CreatedBy,
                ticket.Comments.Count);
        }

        public async Task<List<CommentDto>> GetCommentsAsync(Guid ticketId)
        {
            var comments = await _commentRepo.GetByTicketIdAsync(ticketId);
            return comments.Select(c => new CommentDto(c.Id, c.Text, c.CreatedAt, c.CreatedBy)).ToList();
        }

        public async Task<CommentDto?> AddCommentAsync(Guid ticketId, CreateCommentDto dto)
        {
            await _commentValidator.ValidateAndThrowAsync(dto);

            var ticket = await _ticketRepo.GetByIdAsync(ticketId);
            if (ticket == null) return null;

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                TicketId = ticketId,
                Text = dto.Text,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = dto.CreatedBy
            };

            await _commentRepo.AddAsync(comment);

            _logger.LogInformation("Comment added to ticket {TicketId} by {User}", ticketId, dto.CreatedBy);

            return new CommentDto(comment.Id, comment.Text, comment.CreatedAt, comment.CreatedBy);
        }
    }
}
