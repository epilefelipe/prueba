using Microsoft.AspNetCore.Mvc;
using TicketManager.Application.DTOs;
using TicketManager.Application.Services;

namespace TicketManager.API.Controllers
{
    [ApiController]
    [Route("api/tickets")]
    public class TicketsController : ControllerBase
    {
        private readonly TicketService _ticketService;

        public TicketsController(TicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpGet]
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
        public async Task<ActionResult<TicketDto>> GetById(Guid id)
        {
            var ticket = await _ticketService.GetByIdAsync(id);
            if (ticket == null) return NotFound(new { error = $"Ticket with id {id} not found" });
            return Ok(ticket);
        }

        [HttpPost]
        public async Task<ActionResult<TicketDto>> Create([FromBody] CreateTicketDto dto)
        {
            var ticket = await _ticketService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ticket);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<TicketDto>> Update(Guid id, [FromBody] UpdateTicketDto dto)
        {
            var ticket = await _ticketService.UpdateAsync(id, dto);
            if (ticket == null) return NotFound(new { error = $"Ticket with id {id} not found" });
            return Ok(ticket);
        }

        [HttpPatch("{id:guid}/status")]
        public async Task<ActionResult<TicketDto>> UpdateStatus(Guid id, [FromBody] UpdateStatusDto dto)
        {
            var ticket = await _ticketService.UpdateStatusAsync(id, dto);
            if (ticket == null) return NotFound(new { error = $"Ticket with id {id} not found" });
            return Ok(ticket);
        }
    }
}
