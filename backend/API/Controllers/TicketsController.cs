using Microsoft.AspNetCore.Mvc;
using TicketManager.Application.DTOs;
using TicketManager.Application.Services;

namespace TicketManager.API.Controllers
{
    [ApiController]
    [Route("api/tickets")]
    [Produces("application/json")]
    public class TicketsController : ControllerBase
    {
        private readonly TicketService _ticketService;

        public TicketsController(TicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<TicketListItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<TicketListItemDto>>> GetTickets(
            [FromQuery] string? status,
            [FromQuery] string? priority,
            [FromQuery] string? q,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _ticketService.GetTicketsAsync(status, priority, q, page, pageSize);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TicketDto>> GetById(Guid id)
        {
            var ticket = await _ticketService.GetByIdAsync(id);
            if (ticket == null) return NotFound(new { error = $"Ticket with id {id} not found" });
            return Ok(ticket);
        }

        [HttpPost]
        [ProducesResponseType(typeof(TicketDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TicketDto>> Create([FromBody] CreateTicketDto dto)
        {
            var user = HttpContext.Items["X-User"]?.ToString() ?? dto.CreatedBy;
            var ticket = await _ticketService.CreateAsync(dto with { CreatedBy = user });
            return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ticket);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TicketDto>> Update(Guid id, [FromBody] UpdateTicketDto dto)
        {
            var ticket = await _ticketService.UpdateAsync(id, dto);
            if (ticket == null) return NotFound(new { error = $"Ticket with id {id} not found" });
            return Ok(ticket);
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TicketDto>> UpdateStatus(Guid id, [FromBody] UpdateStatusDto dto)
        {
            var ticket = await _ticketService.UpdateStatusAsync(id, dto);
            if (ticket == null) return NotFound(new { error = $"Ticket with id {id} not found" });
            return Ok(ticket);
        }
    }
}
