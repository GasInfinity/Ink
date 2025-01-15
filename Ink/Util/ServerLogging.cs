using Microsoft.Extensions.Logging;

namespace Ink.Util;

public static partial class ServerLogging
{
    [LoggerMessage(Level = LogLevel.Debug, Message = "Connection id \"{Id}\" requested status")]
    public static partial void StatusRequested(this ILogger logger, string id);

    [LoggerMessage(Level = LogLevel.Information, Message = "Connection id \"{Id}\" logged in as \"{Username}\" ({Uuid})")]
    public static partial void LoggedIn(this ILogger logger, string id, string username, Uuid uuid);

    [LoggerMessage(Level = LogLevel.Information, Message = "Connnection id \"{Id}\" disconnected due to: \"{Reason}\"")]
    public static partial void Disconnected(this ILogger logger, string id, string reason);
}
