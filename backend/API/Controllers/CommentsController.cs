using Microsoft.AspNetCore.Mvc;
using TicketManager.Application.DTOs;
using TicketManager.Application.Services;

namespace TicketManager.API.Controllers
{
    [ApiController]
    [Route("api/tickets/{ticketId:guid}/comments")]
    public class CommentsController : ControllerBase
    {
        private readonly TicketService _ticketService;

        public CommentsController(TicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpGet]
        public async Task<ActionResult<List<CommentDto>>> GetComments(Guid ticketId)
        {
            var comments = await _ticketService.GetCommentsAsync(ticketId);
            return Ok(comments);
        }

        [HttpPost]
        public async Task<ActionResult<CommentDto>> Create(Guid ticketId, [FromBody] CreateCommentDto dto)
        {
            var user = HttpContext.Items["X-User"]?.ToString() ?? dto.CreatedBy;
            var comment = await _ticketService.AddCommentAsync(ticketId, dto with { CreatedBy = user });
            if (comment == null) return NotFound(new { error = $"Ticket with id {ticketId} not found" });
            return CreatedAtAction(nameof(GetComments), new { ticketId }, comment);
        }
    }
}
