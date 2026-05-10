using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace TuranLogix.Application.DTOs.Phone;

/// <summary>
/// Запрос на отправку OTP-кода на номер телефона
/// </summary>
public record SendOtpRequest(
    [Required][SwaggerSchema("Номер телефона в формате E.164, например +77001234567")] string PhoneNumber);

/// <summary>
/// Ответ на успешную отправку OTP-кода
/// </summary>
public record SendOtpResponse(
    [SwaggerSchema("Идентификатор верификации для последующего подтверждения кода")] string VerifyId);

/// <summary>
/// Запрос на подтверждение OTP-кода
/// </summary>
public record ConfirmOtpRequest(
    [Required][SwaggerSchema("Идентификатор верификации из ответа send-otp")] string VerifyId,
    [Required][SwaggerSchema("Код из SMS-сообщения")] string Token);

/// <summary>
/// Ответ на подтверждение OTP-кода
/// </summary>
public record ConfirmOtpResponse(
    [SwaggerSchema("true — телефон успешно подтверждён; false — код неверен или истёк")] bool Success);
