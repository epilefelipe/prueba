using TicketManager.Domain.Entities;
using TicketManager.Domain.Enums;

namespace TicketManager.Tests.Domain;

public class TicketTests
{
    [Theory]
    [InlineData(Status.Open, Status.InProgress, true)]
    [InlineData(Status.Open, Status.Closed, true)]
    [InlineData(Status.Open, Status.Resolved, false)]
    [InlineData(Status.InProgress, Status.Resolved, true)]
    [InlineData(Status.InProgress, Status.Closed, true)]
    [InlineData(Status.InProgress, Status.Open, false)]
    [InlineData(Status.Resolved, Status.Closed, true)]
    [InlineData(Status.Resolved, Status.Open, false)]
    [InlineData(Status.Resolved, Status.InProgress, false)]
    [InlineData(Status.Closed, Status.Open, false)]
    [InlineData(Status.Closed, Status.InProgress, false)]
    [InlineData(Status.Closed, Status.Resolved, false)]
    public void CanTransitionTo_ReturnsCorrectResult(Status current, Status target, bool expected)
    {
        var ticket = new Ticket { Id = Guid.NewGuid(), Status = current };
        var result = ticket.CanTransitionTo(target);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void NewTicket_HasOpenStatus()
    {
        var ticket = new Ticket();
        Assert.Equal(Status.Open, ticket.Status);
    }

    [Fact]
    public void Ticket_InitializesWithEmptyComments()
    {
        var ticket = new Ticket();
        Assert.NotNull(ticket.Comments);
        Assert.Empty(ticket.Comments);
    }
}
