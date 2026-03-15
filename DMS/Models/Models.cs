using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class User
{
    [Key]
    public Guid UUID { get; set; } = Guid.NewGuid();

    [Required]
    public string Username { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }

    // Relacja 1:1 z Info_Użytkownicy
    public virtual UserInfo Info { get; set; }
}

public class UserInfo
{
    [Key, ForeignKey("User")]
    public Guid UUID { get; set; }

    public string? Region { get; set; }

    [Required]
    public string Language { get; set; } = "pl";

    [Required]
    [RegularExpression("^(jasny|ciemny)$", ErrorMessage = "Dozwolone wartości: jasny, ciemny")]
    public string DisplayMode { get; set; } = "jasny";

    public virtual User User { get; set; }
}

public class PrivateMessage
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MessageId { get; set; }

    [Required]
    public Guid SenderUUID { get; set; }

    [Required]
    public Guid ReceiverUUID { get; set; }

    [Required]
    [MinLength(1)]
    public string Content { get; set; } // tresc wiadomosci

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}