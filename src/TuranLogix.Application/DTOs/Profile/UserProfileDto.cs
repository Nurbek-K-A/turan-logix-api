using Swashbuckle.AspNetCore.Annotations;

namespace TuranLogix.Application.DTOs.Profile;

/// <summary>
/// Ответ на обновление профиля пользователя
/// </summary>
public record UpdateProfileResponse(
    [SwaggerSchema("true — номер телефона был изменён и требует повторной OTP-верификации")] bool PhoneVerificationRequired);

/// <summary>
/// Профиль пользователя
/// </summary>
public record UserProfileDto(
    [SwaggerSchema("Id пользователя")] int Id,
    [SwaggerSchema("Полное имя")] string FullName,
    [SwaggerSchema("Email")] string Email,
    [SwaggerSchema("Номер телефона")] string PhoneNumber,
    [SwaggerSchema("Название компании")] string? CompanyName,
    [SwaggerSchema("БИН компании")] string? Bin,
    [SwaggerSchema("Признак верификации аккаунта")] bool IsVerified,
    [SwaggerSchema("Роль пользователя")] string Role);
