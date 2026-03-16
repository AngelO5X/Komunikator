using System.ComponentModel.DataAnnotations;

public class RegisterRequest
{
    [Required]
    public string Username { get; set; }

    [Required]
    [EmailAddress(ErrorMessage = "Nieprawidłowy format email.")]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Email musi mieć poprawną domenę.")]
    public string Email { get; set; }

    [Required]
    [MinLength(8, ErrorMessage = "Hasło musi mieć minimum 8 znaków.")]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*[!@#$%^&*(),.? hot "":{}|<>]).*$",
        ErrorMessage = "Hasło musi mieć 1 wielką literę i 1 znak specjalny.")]
    public string Password { get; set; }

    [Required]
    public string Language { get; set; } = "en";
}

public class LoginRequest
{
    [Required]
    public string UsernameOrEmail { get; set; }
    [Required]
    public string Password { get; set; }
}