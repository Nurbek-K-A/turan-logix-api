namespace TuranLogix.Application.DTOs.Profile;

public record UserProfileDto(
    int Id,
    string FullName,
    string Email,
    string PhoneNumber,
    string? CompanyName,
    string? Bin,
    bool IsVerified,
    string Role);
