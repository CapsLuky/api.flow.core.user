using System;
using System.Collections.Generic;

namespace Core.Common;

public readonly struct Result<T>
{
    public T? Value { get; }
    public bool IsSuccess { get; }
    public IReadOnlyList<ErrorDetail> Errors { get; }

    private Result(T? value, bool isSuccess, IReadOnlyList<ErrorDetail> errors)
    {
        Value = value;
        IsSuccess = isSuccess;
        Errors = errors;
    }

    // Factory para sucesso
    public static Result<T> Ok(T value)
        => new(value, true, Array.Empty<ErrorDetail>());

    // Factory para falha (pode receber vários erros)
    public static Result<T> Fail(IReadOnlyList<ErrorDetail> errors)
        => new(default, false, errors);

    // Conversão implícita de T para Result<T> (tratado como sucesso)
    public static implicit operator Result<T>(T value)
        => Ok(value);
}
