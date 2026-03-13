using System.Text.Json.Serialization;

namespace ModularAPITemplate.SharedKernel.Application;

/// <summary>
/// Resultado genérico para operações de aplicação.
/// Encapsula sucesso/falha com valor ou erros.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string[] Messages { get; } = [];
    [JsonIgnore]
    public string? Error { get; }

    protected Result(bool isSuccess, string? error = null, params string[] messages)
    {
        if (isSuccess && error is not null)
            throw new ArgumentException("Resultado de sucesso não pode conter erro.", nameof(error));
        if (!isSuccess && string.IsNullOrWhiteSpace(error))
            throw new ArgumentException("Resultado de falha deve conter mensagem de erro.", nameof(error));

        IsSuccess = isSuccess;
        Error = error;
        Messages = messages;
    }

    public static Result Success() => new(true);
    public static Result Success(params string[] messages) => new(true, null, messages);
    public static Result Failure(string error) => new(false, error);
    public static Result Failure(string error, params string[] messages) => new(false, error, messages);
    public static Result<T> Success<T>(T value, params string[] messages) => new(value, true, null, messages);
    public static Result<T> Failure<T>(string error, params string[] messages) => new(default, false, error, messages);
}

/// <summary>
/// Resultado genérico com valor tipado.
/// </summary>
public class Result<T> : Result
{
    public T? Value { get; }

    internal Result(T? value, bool isSuccess, string? error = null, params string[] messages)
        : base(isSuccess, error, messages)
    {
        Value = value;
    }
}
