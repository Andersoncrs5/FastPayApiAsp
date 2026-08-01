using App.Config.Exceptions.Classes;

namespace App.Utils.Result;

public sealed class Result<T> : IResultState
{
    private readonly T? _value;

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public IReadOnlyList<string> Errors { get; }

    public int StatusCode { get; }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException(
            "Cannot access the value of a failed result.");

    private Result(
        bool isSuccess,
        T? value,
        IReadOnlyList<string> errors,
        int statusCode)
    {
        IsSuccess = isSuccess;
        _value = value;
        Errors = errors;
        StatusCode = statusCode;
    }

    public static async Task<T> UnwrapOrDefault<T>(
        Task<Result<T>> task,
        T defaultValue)
    {
        var result = await task;
        return result.IsSuccess ? result.Value : defaultValue;
    }
    
    public static Result<T> Success(
        T value,
        int statusCode = 200)
        => new(
            true,
            value,
            Array.Empty<string>(),
            statusCode);

    public static Result<T> Created(T value)
        => new(
            true,
            value,
            Array.Empty<string>(),
            201);

    public static Result<T> Ok(
        int statusCode = 200,
        T? value = default)
        => new(
            true,
            value,
            Array.Empty<string>(),
            statusCode);

    public static Result<T> Failure(
        string error,
        int statusCode = 400)
        => new(
            false,
            default,
            new[] { error },
            statusCode);

    public static Result<T> Failure(
        IEnumerable<string> errors,
        int statusCode = 400)
        => new(
            false,
            default,
            errors.ToArray(),
            statusCode);

    public static Result<T> NotFound(string error)
        => Failure(error, 404);

    public async Task<Result<TOut>> Bind<TOut>(
        Func<T, Task<Result<TOut>>> next)
    {
        ArgumentNullException.ThrowIfNull(next);

        if (IsFailure)
            return Result<TOut>.Failure(Errors, StatusCode);

        return await next(Value);
    }

    public Result<TOut> Bind<TOut>(
        Func<T, Result<TOut>> next)
    {
        ArgumentNullException.ThrowIfNull(next);

        if (IsFailure)
            return Result<TOut>.Failure(Errors, StatusCode);

        return next(Value);
    }
    
    public static implicit operator Result<T>(T value)
        => Success(value);

    public static implicit operator Result<T>(string error)
        => Failure(error);

    public static implicit operator Result<T>(List<string> errors)
        => Failure(errors);
}

public static class ResultExtensions
{
    public static async Task<Result<TOut>> IfFailure<TIn, TOut>(
        this Task<Result<TIn>> task,
        Func<Result<TIn>, Result<TOut>> onFailure,
        Func<TIn, Task<Result<TOut>>> onSuccess)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(onFailure);
        ArgumentNullException.ThrowIfNull(onSuccess);

        var result = await task;

        if (result.IsFailure)
            return onFailure(result);

        return await onSuccess(result.Value);
    }
    
    public static async Task<TResult> IfPresentOrElse<T, TResult>(
        this Task<Result<T>> task,
        Func<T, Task<TResult>> onPresent,
        Func<Result<T>, TResult> onEmpty)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(onPresent);
        ArgumentNullException.ThrowIfNull(onEmpty);

        var result = await task;

        if (result.IsSuccess)
            return await onPresent(result.Value);

        return onEmpty(result);
    }
    
    public static async Task<TResult> IfPresent<T, TResult>(
        this Task<Result<T>> task,
        Func<T, TResult> action)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(action);

        var result = await task;

        if (result.IsFailure)
            return default!;

        return action(result.Value);
    }

    public static async Task<TResult> IfPresent<T, TResult>(
        this Task<Result<T>> task,
        Func<T, Task<TResult>> action)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(action);

        var result = await task;

        if (result.IsFailure)
            return default!;

        return await action(result.Value);
    }

    public static async Task<TResult> IfEmpty<T, TResult>(
        this Task<Result<T>> task,
        Func<Result<T>, TResult> action)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(action);

        var result = await task;

        if (result.IsSuccess)
            return default!;

        return action(result);
    }

    public static async Task<TResult> IfEmpty<T, TResult>(
        this Task<Result<T>> task,
        Func<Result<T>, Task<TResult>> action)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(action);

        var result = await task;

        if (result.IsSuccess)
            return default!;

        return await action(result);
    }
    
    // --- ALTERAÇÕES INSERIDAS AQUI ---

    public static T UnwrapOrThrow<T>(this Result<T> result)
    {
        if (result.IsFailure)
        {
            throw new ResultException(result.Errors, result.StatusCode);
        }
    
        return result.Value;
    }

    public static async Task<T> UnwrapOrThrow<T>(this Task<Result<T>> task)
    {
        var result = await task;
        
        if (result.IsFailure)
        {
            throw new ResultException(result.Errors, result.StatusCode);
        }
    
        return result.Value;
    }
}