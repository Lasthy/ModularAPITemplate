using System.Text.Json.Serialization;

namespace ModularAPITemplate.SharedKernel.Application;

/// <summary>
/// Represents the result of an application operation.
/// Encapsulates success/failure state, optional value, and related messages.
/// </summary>
public class Result
{
    /// <summary>
    /// Indicates whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Indicates whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Optional informational or warning messages.
    /// </summary>
    public string[] Messages { get; } = [];

    /// <summary>
    /// Optional error message when the operation failed.
    /// </summary>
    [JsonIgnore]
    public string? Error { get; }

    protected Result(bool isSuccess, string? error = null, params string[] messages)
    {
        if (isSuccess && error is not null)
            throw new ArgumentException("A successful result cannot have an error message.", nameof(error));
        if (!isSuccess && string.IsNullOrWhiteSpace(error))
            throw new ArgumentException("A failure result must have an error message.", nameof(error));

        IsSuccess = isSuccess;
        Error = error;
        Messages = messages;
    }

    /// <summary>
    /// Creates a successful result without a value.
    /// </summary>
    public static Result Success() => new(true);

    /// <summary>
    /// Creates a successful result with informational messages.
    /// </summary>
    public static Result Success(params string[] messages) => new(true, null, messages);

    /// <summary>
    /// Creates a failure result with an error message.
    /// </summary>
    public static Result Failure(string error) => new(false, error);

    /// <summary>
    /// Creates a failure result with an error message and additional messages.
    /// </summary>
    public static Result Failure(string error, params string[] messages) => new(false, error, messages);

    /// <summary>
    /// Creates a successful result with a typed value.
    /// </summary>
    public static Result<T> Success<T>(T value, params string[] messages) => new(value, true, null, messages);

    /// <summary>
    /// Creates a failure result for a typed operation.
    /// </summary>
    public static Result<T> Failure<T>(string error, params string[] messages) => new(default, false, error, messages);
}

/// <summary>
/// Represents a result containing a typed value.
/// </summary>
/// <typeparam name="T">Type of the value.</typeparam>
public class Result<T> : Result
{
    /// <summary>
    /// Value returned on success. May be null for reference types.
    /// </summary>
    public T? Value { get; }

    internal Result(T? value, bool isSuccess, string? error = null, params string[] messages)
        : base(isSuccess, error, messages)
    {
        Value = value;
    }
}
