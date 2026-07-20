namespace App.Utils.Result;

public class Result<T> : IResultState
{
    private readonly T? _value;
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public IReadOnlyList<string> Errors { get; }
    public int StatusCode { get; }

    public T Value => IsSuccess 
        ? _value! 
        : throw new InvalidOperationException("Não é possível acessar o valor de um resultado que falhou.");

    private Result(bool isSuccess, T? value, List<string>? errors, int statusCode)
    {
        IsSuccess = isSuccess;
        _value = value;
        Errors = errors?.AsReadOnly() == null? Array.Empty<string>() : errors;
        StatusCode = statusCode;
    }

    public static Result<T> Success(T value, int statusCode = 200) 
        => new(true, value, null, statusCode);

    public static Result<T> Ok(int statusCode = 200, T? value = default) 
        => new(true, value, null, statusCode);

    public static Result<T> Failure(List<string> errors, int statusCode = 400) 
        => new(false, default, errors ?? throw new ArgumentNullException(nameof(errors)), statusCode);

    public static Result<T> Failure(string error, int statusCode = 400) 
        => new(false, default, new List<string> { error }, statusCode);

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(List<string> errors) => Failure(errors);
    public static implicit operator Result<T>(string error) => Failure(error);
}

public class Result: IResultState
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public IReadOnlyList<string> Errors { get; }
    public int StatusCode { get; }

    private Result(bool isSuccess, List<string>? errors, int statusCode)
    {
        IsSuccess = isSuccess;
        Errors = errors?.AsReadOnly() == null? Array.Empty<string>() : errors;
        StatusCode = statusCode;
    }

    public static Result Success(int statusCode = 204) => new(true, null, statusCode);
    
    public static Result Ok() => new(true, null, 200);
    
    public static Result Failure(List<string> errors, int statusCode = 400) => new(false, errors, statusCode);
    
    public static Result Failure(string error, int statusCode = 400) => new(false, new List<string> { error }, statusCode);

    public static implicit operator Result(List<string> errors) => Failure(errors);
    public static implicit operator Result(string error) => Failure(error);
}