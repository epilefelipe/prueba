using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using TicketManager.Application.DTOs;
using TicketManager.Application.Interfaces;
using TicketManager.Application.Services;
using TicketManager.Application.Validators;
using TicketManager.Domain.Entities;
using TicketManager.Domain.Enums;

namespace TicketManager.Tests.Services;

public class TicketServiceTests
{
    private readonly Mock<ITicketRepository> _ticketRepo;
    private readonly Mock<ICommentRepository> _commentRepo;
    private readonly TicketService _service;

    public TicketServiceTests()
    {
        _ticketRepo = new Mock<ITicketRepository>();
        _commentRepo = new Mock<ICommentRepository>();
        var logger = Mock.Of<ILogger<TicketService>>();

        _service = new TicketService(
            _ticketRepo.Object,
            _commentRepo.Object,
            new CreateTicketValidator(),
            new UpdateTicketValidator(),
            new UpdateStatusValidator(),
            new CreateCommentValidator(),
            logger);
    }

    [Fact]
    public async Task CreateAsync_ValidDto_CreatesTicket()
    {
        var dto = new CreateTicketDto("Valid title here", "A valid description that is long enough", "High", "user@test.com");

        Ticket? saved = null;
        _ticketRepo.Setup(r => r.AddAsync(It.IsAny<Ticket>()))
            .Callback<Ticket>(t => saved = t);
        _ticketRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(dto);

        Assert.NotNull(saved);
        Assert.Equal(dto.Title, saved.Title);
        Assert.Equal(Status.Open, saved.Status);
        Assert.Equal("user@test.com", saved.CreatedBy);
        Assert.Equal(result.Id, saved.Id);
    }

    [Fact]
    public async Task CreateAsync_InvalidTitle_ThrowsValidationException()
    {
        var dto = new CreateTicketDto("abc", "Valid description here...", "Low", "user@test.com");
        await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task GetByIdAsync_ExistingTicket_ReturnsDto()
    {
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            Title = "Test ticket",
            Description = "Description of the test ticket",
            Priority = Priority.Medium,
            Status = Status.Open,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "user@test.com"
        };
        _ticketRepo.Setup(r => r.GetByIdAsync(ticket.Id)).ReturnsAsync(ticket);

        var result = await _service.GetByIdAsync(ticket.Id);

        Assert.NotNull(result);
        Assert.Equal(ticket.Title, result.Title);
        Assert.Equal(ticket.Status.ToString(), result.Status);
    }

    [Fact]
    public async Task GetByIdAsync_NonExisting_ReturnsNull()
    {
        _ticketRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(null as Ticket);
        var result = await _service.GetByIdAsync(Guid.NewGuid());
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateStatusAsync_ValidTransition_UpdatesStatus()
    {
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            Status = Status.Open,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _ticketRepo.Setup(r => r.GetByIdAsync(ticket.Id)).ReturnsAsync(ticket);
        _ticketRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new UpdateStatusDto("InProgress");
        var result = await _service.UpdateStatusAsync(ticket.Id, dto);

        Assert.NotNull(result);
        Assert.Equal("InProgress", result.Status);
    }

    [Fact]
    public async Task UpdateStatusAsync_InvalidTransition_ThrowsInvalidOperationException()
    {
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            Status = Status.Closed,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _ticketRepo.Setup(r => r.GetByIdAsync(ticket.Id)).ReturnsAsync(ticket);

        var dto = new UpdateStatusDto("Open");
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateStatusAsync(ticket.Id, dto));
        Assert.Contains("transition", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetCommentsAsync_ReturnsMappedComments()
    {
        var ticketId = Guid.NewGuid();
        var comments = new List<Comment>
        {
            new() { Id = Guid.NewGuid(), TicketId = ticketId, Text = "First comment", CreatedAt = DateTime.UtcNow, CreatedBy = "user@test.com" },
            new() { Id = Guid.NewGuid(), TicketId = ticketId, Text = "Second comment", CreatedAt = DateTime.UtcNow, CreatedBy = "user@test.com" }
        };
        _commentRepo.Setup(r => r.GetByTicketIdAsync(ticketId)).ReturnsAsync(comments);

        var result = await _service.GetCommentsAsync(ticketId);

        Assert.Equal(2, result.Count);
        Assert.Equal("First comment", result[0].Text);
    }
}
