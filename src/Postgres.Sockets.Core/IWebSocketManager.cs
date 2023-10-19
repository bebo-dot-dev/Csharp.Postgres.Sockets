namespace Postgres.Sockets.Core;

/// <summary>
/// The contract for a type that manages websockets and websocket messages.
/// </summary>
public interface IWebSocketManager
{
    /// <summary>
    /// Adds a new websocket to the <see cref="IWebSocketManager"/> asynchronously.
    /// </summary>
    /// <param name="context">The context for the websocket.</param>
    /// <returns><see cref="Task"/></returns>
    Task AddWebSocketAsync(WebSocketContext context);

    /// <summary>
    /// Handles a data change notification message asynchronously.
    /// </summary>
    /// <param name="messagePayload">The payload of the notification message as a string.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see cref="Task"/></returns>
    Task HandleDataChangeNotificationAsync(string messagePayload, CancellationToken cancellationToken);
}