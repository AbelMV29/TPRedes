using System.IO.Compression;
using System.Net.Sockets;
using System.Text;

namespace TPRedes.Class;

public static class HttpResponseWriter
{
    public static void SendFile(NetworkStream stream, string filePath, HttpRequest request)
    {
        byte[] body = File.ReadAllBytes(filePath);
        Send(stream, "200 OK", GetContentType(filePath), body, ShouldCompress(request));
    }

    public static void SendHtml(NetworkStream stream, string status, string html, HttpRequest request)
    {
        byte[] body = Encoding.UTF8.GetBytes(html);
        Send(stream, status, "text/html; charset=utf-8", body, ShouldCompress(request));
    }

    private static void Send(NetworkStream stream, string status, string contentType, byte[] body, bool compress)
    {
        byte[] responseBody = compress ? Compress(body) : body;
        string compressionHeader = compress ? "Content-Encoding: gzip\r\n" : "";

        string headers =
            $"HTTP/1.1 {status}\r\n" +
            $"Content-Type: {contentType}\r\n" +
            compressionHeader +
            $"Content-Length: {responseBody.Length}\r\n" +
            "Connection: close\r\n" +
            "\r\n";

        stream.Write(Encoding.UTF8.GetBytes(headers));
        stream.Write(responseBody);
    }

    private static bool ShouldCompress(HttpRequest request)
    {
        return request.Headers.TryGetValue("Accept-Encoding", out string? acceptEncoding) && acceptEncoding.Contains("gzip", StringComparison.OrdinalIgnoreCase);
    }

    private static byte[] Compress(byte[] body)
    {
        using MemoryStream output = new MemoryStream();

        using (GZipStream gzip = new GZipStream(output, CompressionLevel.SmallestSize, leaveOpen: true))
        {
            gzip.Write(body);
        }

        return output.ToArray();
    }

    private static string GetContentType(string filePath)
    {
        return Path.GetExtension(filePath).ToLowerInvariant() switch
        {
            ".html" => "text/html; charset=utf-8",
            ".css" => "text/css; charset=utf-8",
            ".js" => "application/javascript; charset=utf-8",
            ".json" => "application/json; charset=utf-8",
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".txt" => "text/plain; charset=utf-8",
            _ => "application/octet-stream"
        };
    }
}
