using FluentValidation;
using TicketManager.Application.DTOs;

namespace TicketManager.Application.Validators
{
    public class CreateTicketValidator : AbstractValidator<CreateTicketDto>
    {
        public CreateTicketValidator()
        {
            RuleFor(x => x.Title).NotEmpty().Length(5, 120);
            RuleFor(x => x.Description).NotEmpty().Length(10, 2000);
            RuleFor(x => x.Priority).NotEmpty().Must(p =>
                new[] { "Low", "Medium", "High", "Critical" }.Contains(p))
                .WithMessage("Priority must be Low, Medium, High, or Critical");
            RuleFor(x => x.CreatedBy).NotEmpty().EmailAddress();
        }
    }

    public class UpdateTicketValidator : AbstractValidator<UpdateTicketDto>
    {
        public UpdateTicketValidator()
        {
            RuleFor(x => x.Title).NotEmpty().Length(5, 120);
            RuleFor(x => x.Description).NotEmpty().Length(10, 2000);
            RuleFor(x => x.Priority).NotEmpty().Must(p =>
                new[] { "Low", "Medium", "High", "Critical" }.Contains(p));
        }
    }

    public class UpdateStatusValidator : AbstractValidator<UpdateStatusDto>
    {
        public UpdateStatusValidator()
        {
            RuleFor(x => x.Status).NotEmpty().Must(s =>
                new[] { "Open", "InProgress", "Resolved", "Closed" }.Contains(s));
        }
    }

    public class CreateCommentValidator : AbstractValidator<CreateCommentDto>
    {
        public CreateCommentValidator()
        {
            RuleFor(x => x.Text).NotEmpty().Length(2, 1000);
            RuleFor(x => x.CreatedBy).NotEmpty().EmailAddress();
        }
    }
}
