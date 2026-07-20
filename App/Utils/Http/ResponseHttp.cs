namespace App.Utils.Http;

public record ResponseHttp<T>(
    T? Data,
    string TraceId,
    bool Success,
    DateTime Timestamp,
    string? Path = null,
    UInt16 ApiVersion = 1,
    IReadOnlyList<string>? Errors = null
);