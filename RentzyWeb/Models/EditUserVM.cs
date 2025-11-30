using System.ComponentModel.DataAnnotations;

public class EditUserVM
{
    public int UserId { get; set; }

    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; }

    public string PhoneNumber { get; set; }
}
