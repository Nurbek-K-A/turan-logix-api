namespace TuranLogix.Application.DTOs.Auth;

public record RegisterRequest(
    string FullName,
    string Email,
    string PhoneNumber,
    string Password,
    string? CompanyName,
    string? Bin);

public record LoginRequest(string Email, string Password);

public record RegisterResponse(int UserId);

public record LoginResponse(string Token);
