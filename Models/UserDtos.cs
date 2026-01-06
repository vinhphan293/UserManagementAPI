using System.ComponentModel.DataAnnotations;

namespace UserManagementAPI.Models;

public record CreateUserDto(
    [Required, StringLength(50, MinimumLength = 1)] string FirstName,
    [Required, StringLength(50, MinimumLength = 1)] string LastName,
    [Required, EmailAddress] string Email,
    [Required, StringLength(100, MinimumLength = 1)] string Department,
    bool IsActive = true
);

public record UpdateUserDto(
    [Required, StringLength(50, MinimumLength = 1)] string FirstName,
    [Required, StringLength(50, MinimumLength = 1)] string LastName,
    [Required, EmailAddress] string Email,
    [Required] bool IsActive,
    [Required, StringLength(100, MinimumLength = 1)] string Department
);