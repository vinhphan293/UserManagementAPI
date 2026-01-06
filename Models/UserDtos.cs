namespace UserManagementAPI.Models;

public record CreateUserDto(
    string FirstName,
    string LastName,
    string Email,
    string Department
);
public record UpdateUserDto(
    string FirstName,
    string LastName,
    string Email,
    bool IsActive,
    string Department
);