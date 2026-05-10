using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace TuranLogix.Application.DTOs.Auth;

/// <summary>
/// Запрос на регистрацию нового пользователя
/// </summary>
public record RegisterRequest(
    [Required][SwaggerSchema("Полное имя пользователя")] string FullName,
    [Required][SwaggerSchema("Email — используется для входа")] string Email,
    [Required][SwaggerSchema("Номер телефона")] string PhoneNumber,
    [Required][SwaggerSchema("Пароль (минимум 6 символов)")] string Password,
    [SwaggerSchema("Название компании (для юридических лиц)")] string? CompanyName,
    [SwaggerSchema("БИН компании")] string? Bin);

/// <summary>
/// Запрос на вход в систему
/// </summary>
public record LoginRequest(
    [Required][SwaggerSchema("Email пользователя")] string Email,
    [Required][SwaggerSchema("Пароль пользователя")] string Password);

/// <summary>
/// Ответ на успешную регистрацию
/// </summary>
public record RegisterResponse(
    [SwaggerSchema("Id созданного пользователя")] int UserId);

/// <summary>
/// Ответ на успешный вход в систему
/// </summary>
public record LoginResponse(
    [SwaggerSchema("JWT Bearer токен для последующих запросов")] string Token);
