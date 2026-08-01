namespace App.Config.Exceptions.Classes;

public class ResultException : Exception
{
    public IReadOnlyList<string> Errors { get; }
    public int StatusCode { get; }

    public ResultException(IReadOnlyList<string> errors, int statusCode) 
        : base(string.Join(", ", errors))
    {
        Errors = errors;
        StatusCode = statusCode;
    }
}