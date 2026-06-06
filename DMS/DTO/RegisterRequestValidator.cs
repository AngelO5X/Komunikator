using FluentValidation;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Nazwa użytkownika jest wymagana.")
            .MinimumLength(3).WithMessage("Nazwa użytkownika musi mieć co najmniej 3 znaki.")
            .MaximumLength(50).WithMessage("Nazwa użytkownika może mieć maksymalnie 50 znaków.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email jest wymagany.")
            .EmailAddress().WithMessage("Nieprawidłowy format email.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Hasło jest wymagane.")
            .MinimumLength(8).WithMessage("Hasło musi mieć minimum 8 znaków.")
            .Matches("[A-Z]").WithMessage("Hasło musi zawierać co najmniej jedną wielką literę.")
            .Matches("[!@#$%^&*(),.?\":{}|<>]").WithMessage("Hasło musi zawierać co najmniej jeden znak specjalny.");

        RuleFor(x => x.Language)
            .NotEmpty().WithMessage("Język jest wymagany.")
            .Must(l => l == "pl" || l == "en").WithMessage("Dozwolone języki: 'pl', 'en'.");
    }
}
