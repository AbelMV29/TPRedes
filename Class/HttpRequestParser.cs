using System.Net.Sockets;
using System.Text;

namespace TPRedes.Class;

public static class HttpRequestParser
{
    public static HttpRequest? Parse(NetworkStream stream, string remoteIp)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);

        string? requestLine = reader.ReadLine();
        if (string.IsNullOrWhiteSpace(requestLine)) return null;

        string[] requestParts = requestLine.Split(' ', 3);
        if (requestParts.Length < 2) throw new InvalidDataException("Solicitud HTTP mal formada.");

        Dictionary<string, string> headers = new(StringComparer.OrdinalIgnoreCase);
        string? line;

        while (!string.IsNullOrEmpty(line = reader.ReadLine()))
        {
            int separatorIndex = line.IndexOf(':');
            if (separatorIndex <= 0) continue;

            string key = line[..separatorIndex].Trim();
            string value = line[(separatorIndex + 1)..].Trim();
            headers[key] = value;
        }

        string method = requestParts[0].ToUpperInvariant();
        string url = requestParts[1];
        Uri uri = new Uri("http://localhost" + url);

        return new HttpRequest
        {
            Method = method,
            Url = url,
            Path = uri.AbsolutePath,
            Headers = headers,
            QueryParameters = ParseQueryParameters(uri.Query),
            Body = ReadBody(reader, headers),
            RemoteIp = remoteIp
        };
    }

    private static string ReadBody(StreamReader reader, Dictionary<string, string> headers)
    {
        if (!headers.TryGetValue("Content-Length", out string? lengthText)) return "";
        if (!int.TryParse(lengthText, out int contentLength) || contentLength <= 0) return "";

        char[] bodyBuffer = new char[contentLength];
        int totalRead = 0;

        while (totalRead < contentLength)
        {
            int read = reader.Read(bodyBuffer, totalRead, contentLength - totalRead);
            if (read == 0) break;

            totalRead += read;
        }

        return new string(bodyBuffer, 0, totalRead);
    }

    private static Dictionary<string, string> ParseQueryParameters(string query)
    {
        Dictionary<string, string> parameters = new(StringComparer.OrdinalIgnoreCase);

        foreach (string queryPart in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            string[] keyValue = queryPart.Split('=', 2);
            string key = Uri.UnescapeDataString(keyValue[0]);
            string value = keyValue.Length == 2 ? Uri.UnescapeDataString(keyValue[1]) : "";
            parameters[key] = value;
        }

        return parameters;
    }
}
