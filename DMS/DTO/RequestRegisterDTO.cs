using System;

public class RegisterRequest
{
    public string Email { get; set; }

    public string Language { get; set; } = "en";

    public string Password { get; set; }

    public string Username { get; set; }
}

public class LoginRequest
{
    public string Password { get; set; }

    public string UsernameOrEmail { get; set; }
}
