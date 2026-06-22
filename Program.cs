using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;
using TPRedes.Class;

internal class Program
{
    private static void Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        ServerConfig serverConfig = configuration.GetSection("ServerConfiguration").Get<ServerConfig>() ?? new ServerConfig();
        Logger.LogPath = serverConfig.LogPath;

        StaticFileServer fileServer = new StaticFileServer(serverConfig.Path);
        TcpListener listener = new TcpListener(IPAddress.Any, serverConfig.Port);

        listener.Start();
        Logger.LogInfo($"Servidor iniciado en puerto {serverConfig.Port}");

        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            Task.Run(() => HandleClient(client, fileServer));
        }
    }

    private static void HandleClient(TcpClient client, StaticFileServer fileServer)
    {
        try
        {
            using var stream = client.GetStream();
            string remoteIp = client.Client.RemoteEndPoint?.ToString() ?? "desconocida";
            HttpRequest? request = HttpRequestParser.Parse(stream, remoteIp);

            if (request is null) return;

            fileServer.Handle(stream, request);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.ToString());
        }
        finally
        {
            client.Close();
        }
    }
}
