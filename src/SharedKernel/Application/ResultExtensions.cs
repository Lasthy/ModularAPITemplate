namespace ModularAPITemplate.SharedKernel.Application;

public static class ResultExtensions
{
    // Transform the value inside a successful Result<T>
    public static Result<TOut> Map<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> map)
    {
        if (result.IsFailure)
            return Result.Failure<TOut>(result.Error!);

        return Result.Success(map(result.Value!));
    }

    // Chain an operation that itself returns a Result (flatMap / SelectMany)
    public static Result<TOut> Bind<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Result<TOut>> bind)
    {
        if (result.IsFailure)
            return Result.Failure<TOut>(result.Error!);

        return bind(result.Value!);
    }

    // Async variants
    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, TOut> map)
    {
        var result = await resultTask;
        return result.Map(map);
    }

    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<Result<TOut>>> bind)
    {
        var result = await resultTask;
        if (result.IsFailure)
            return Result.Failure<TOut>(result.Error!);

        return await bind(result.Value!);
    }

    // Synchronous
    public static TOut Match<T, TOut>(
        this Result<T> result,
        Func<T, TOut> onSuccess,
        Func<string, TOut> onFailure)
    {
        return result.IsSuccess
            ? onSuccess(result.Value!)
            : onFailure(result.Error!);
    }

    // Non-generic Result
    public static TOut Match<TOut>(
        this Result result,
        Func<TOut> onSuccess,
        Func<string, TOut> onFailure)
    {
        return result.IsSuccess
            ? onSuccess()
            : onFailure(result.Error!);
    }

    // Async — result is already resolved, callbacks may be async
    public static async Task<TOut> MatchAsync<T, TOut>(
        this Result<T> result,
        Func<T, Task<TOut>> onSuccess,
        Func<string, Task<TOut>> onFailure)
    {
        return result.IsSuccess
            ? await onSuccess(result.Value!)
            : await onFailure(result.Error!);
    }

    // Most common async variant: awaiting the result itself
    public static async Task<TOut> MatchAsync<T, TOut>(
        this Task<Result<T>> resultTask,
        Func<T, TOut> onSuccess,
        Func<string, TOut> onFailure)
    {
        var result = await resultTask;
        return result.IsSuccess
            ? onSuccess(result.Value!)
            : onFailure(result.Error!);
    }
}