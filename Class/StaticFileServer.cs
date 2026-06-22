using System.Net.Sockets;

namespace TPRedes.Class;

public sealed class StaticFileServer
{
    private readonly string rootFolder;

    public StaticFileServer(string rootFolder)
    {
        this.rootFolder = Path.GetFullPath(rootFolder);
    }

    public void Handle(NetworkStream stream, HttpRequest request)
    {
        LogRequest(request);

        if (request.Method is not "GET" and not "POST")
        {
            HttpResponseWriter.SendHtml(stream, "405 Method Not Allowed", "<html><body><h1>405 Method Not Allowed</h1></body></html>", request);
            return;
        }

        if (request.Method == "POST") Logger.LogInfo($"IP={request.RemoteIp} POST BODY: {request.Body}");

        string? fullPath = ResolveRequestedPath(request.Path);

        if (fullPath is null || !File.Exists(fullPath))
        {
            Send404(stream, request);
            return;
        }

        HttpResponseWriter.SendFile(stream, fullPath, request);
    }

    private void LogRequest(HttpRequest request)
    {
        Logger.LogInfo($"IP={request.RemoteIp} {request.Method} {request.Url}");

        foreach ((string key, string value) in request.QueryParameters)
        {
            Logger.LogInfo($"IP={request.RemoteIp} QueryParam: {key}={value}");
        }
    }

    private string? ResolveRequestedPath(string requestPath)
    {
        string decodedPath = Uri.UnescapeDataString(requestPath).Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);

        if (string.IsNullOrWhiteSpace(decodedPath)) decodedPath = "index.html";
        if (decodedPath.EndsWith(Path.DirectorySeparatorChar)) decodedPath = Path.Combine(decodedPath, "index.html");

        string fullPath = Path.GetFullPath(Path.Combine(rootFolder, decodedPath));
        if (!IsInsideRoot(fullPath)) return null;

        return fullPath;
    }

    private bool IsInsideRoot(string fullPath)
    {
        return fullPath.Equals(rootFolder, StringComparison.OrdinalIgnoreCase) || fullPath.StartsWith(rootFolder + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    private void Send404(NetworkStream stream, HttpRequest request)
    {
        string errorFile = Path.Combine(rootFolder, "404.html");
        string html = File.Exists(errorFile) ? File.ReadAllText(errorFile) : "<html><body><h1>404 Not Found</h1></body></html>";

        HttpResponseWriter.SendHtml(stream, "404 Not Found", html, request);
    }
}
