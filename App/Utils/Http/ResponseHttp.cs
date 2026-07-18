namespace App.Utils.Http;

public record ResponseHttp<T>(
    T? Data,
    string Message,
    string TraceId,
    bool Success,
    DateTime Timestamp,
    string? DetailsError = null,
    string? Path = null,
    UInt16 ApiVersion = 1,
    IReadOnlyList<string>? Errors = null
);