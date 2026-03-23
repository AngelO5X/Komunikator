using FluentValidation;

public class CreatePrivateMessageRequestValidator : AbstractValidator<CreatePrivateMessageRequest>
{
    public CreatePrivateMessageRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Treść wiadomości jest wymagana.")
            .MinimumLength(1).WithMessage("Treść nie może być pusta.");

        RuleFor(x => x.SenderUUID)
            .NotEmpty().WithMessage("SenderUUID jest wymagane.")
            .Must(g => g != Guid.Empty).WithMessage("Nieprawidłowy SenderUUID.");

        RuleFor(x => x.ReceiverUUID)
            .NotEmpty().WithMessage("ReceiverUUID jest wymagane.")
            .Must(g => g != Guid.Empty).WithMessage("Nieprawidłowy ReceiverUUID.");

        RuleFor(x => x)
            .Must(r => r.SenderUUID != r.ReceiverUUID)
            .WithMessage("SenderUUID i ReceiverUUID muszą być różne.");
    }
}
