using TicketManager.Domain.Enums;

namespace TicketManager.Application.DTOs
{
    public record TicketDto(
        Guid Id,
        string Title,
        string Description,
        string Priority,
        string Status,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        string CreatedBy,
        int CommentCount
    );

    public record TicketListItemDto(
        Guid Id,
        string Title,
        string Priority,
        string Status,
        DateTime CreatedAt,
        string CreatedBy,
        int CommentCount
    );

    public record CreateTicketDto(string Title, string Description, string Priority, string CreatedBy);

    public record UpdateTicketDto(string Title, string Description, string Priority);

    public record UpdateStatusDto(string Status);

    public record CommentDto(Guid Id, string Text, DateTime CreatedAt, string CreatedBy);

    public record CreateCommentDto(string Text, string CreatedBy);

    public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize)
    {
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
