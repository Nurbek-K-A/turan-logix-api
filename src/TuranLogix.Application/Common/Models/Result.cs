using TuranLogix.Domain.Errors;

namespace TuranLogix.Application.Common.Models;

/// <summary>
/// Результат операции без возвращаемого значения
/// </summary>
public class Result
{
    /// <param name="isSuccess">Признак успешности операции</param>
    /// <param name="error">Ошибка (должна быть Error.None при успехе)</param>
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException();
        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException();

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>Операция завершилась успешно</summary>
    public bool IsSuccess { get; }

    /// <summary>Операция завершилась с ошибкой</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>Ошибка операции (Error.None при успехе)</summary>
    public Error Error { get; }

    /// <summary>Создать успешный результат</summary>
    public static Result Success() => new(true, Error.None);

    /// <summary>
    /// Создать результат с ошибкой
    /// </summary>
    /// <param name="error">Описание ошибки</param>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Создать успешный результат со значением
    /// </summary>
    /// <typeparam name="T">Тип возвращаемого значения</typeparam>
    /// <param name="value">Значение</param>
    public static Result<T> Success<T>(T value) => new(value, true, Error.None);

    /// <summary>
    /// Создать результат с ошибкой (для типизированного результата)
    /// </summary>
    /// <typeparam name="T">Тип возвращаемого значения</typeparam>
    /// <param name="error">Описание ошибки</param>
    public static Result<T> Failure<T>(Error error) => new(default!, false, error);
}

/// <summary>
/// Результат операции с возвращаемым значением
/// </summary>
/// <typeparam name="T">Тип возвращаемого значения</typeparam>
public class Result<T> : Result
{
    private readonly T _value;

    /// <param name="value">Значение результата</param>
    /// <param name="isSuccess">Признак успешности операции</param>
    /// <param name="error">Ошибка</param>
    protected internal Result(T value, bool isSuccess, Error error) : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// Значение результата
    /// </summary>
    /// <exception cref="InvalidOperationException">Если результат содержит ошибку</exception>
    public T Value => IsSuccess
        ? _value
        : throw new InvalidOperationException("Нельзя получить значение из неуспешного результата");

    /// <summary>Неявное преобразование значения в успешный Result</summary>
    public static implicit operator Result<T>(T value) => Success(value);
}
