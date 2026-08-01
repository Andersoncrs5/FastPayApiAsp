namespace App.Utils.Result;

public interface IResultState
{
    bool IsSuccess { get; }
    bool IsFailure { get; }
    int StatusCode { get; }
}