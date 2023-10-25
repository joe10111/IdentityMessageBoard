using System;
using System.ComponentModel.DataAnnotations;

public class MessageViewModel
{
    [Required]
    public string Content { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Please enter a positive integer for number of days.")]
    public int ExpiresIn { get; set; }

    public DateTime GetExpirationDate()
    {
        return DateTime.UtcNow.AddDays(ExpiresIn);
    }
}