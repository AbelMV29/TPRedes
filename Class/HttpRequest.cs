namespace TPRedes.Class;

public sealed class HttpRequest
{
    public string Method { get; init; } = "";
    public string Url { get; init; } = "";
    public string Path { get; init; } = "/";
    public string Body { get; init; } = "";
    public string RemoteIp { get; init; } = "";
    public Dictionary<string, string> Headers { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string> QueryParameters { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
