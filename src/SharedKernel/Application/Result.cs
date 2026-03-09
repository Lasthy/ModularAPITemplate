namespace ModularAPITemplate.SharedKernel.Application;

/// <summary>
/// Resultado genérico para operações de aplicação.
/// Encapsula sucesso/falha com valor ou erros.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }

    protected Result(bool isSuccess, string? error)
    {
        if (isSuccess && error is not null)
            throw new ArgumentException("Resultado de sucesso não pode conter erro.", nameof(error));
        if (!isSuccess && string.IsNullOrWhiteSpace(error))
            throw new ArgumentException("Resultado de falha deve conter mensagem de erro.", nameof(error));

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);
    public static Result<T> Success<T>(T value) => new(value, true, null);
    public static Result<T> Failure<T>(string error) => new(default, false, error);
}

/// <summary>
/// Resultado genérico com valor tipado.
/// </summary>
public class Result<T> : Result
{
    public T? Value { get; }

    internal Result(T? value, bool isSuccess, string? error)
        : base(isSuccess, error)
    {
        Value = value;
    }
}
