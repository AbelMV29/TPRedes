namespace TPRedes.Class;

public sealed class ServerConfig
{
    public int Port { get; init; }
    public string Path { get; init; } = "Data";
    public string LogPath { get; init; } = "./Logs";
}
