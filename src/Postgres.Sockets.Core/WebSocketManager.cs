using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace Postgres.Sockets.Core;

internal class WebSocketManager : IWebSocketManager
{
    private readonly ILogger<WebSocketManager> _logger;
    
    private readonly ConcurrentDictionary<Guid, WebSocketContext> _insertSockets = new();
    private readonly ConcurrentDictionary<Guid, WebSocketContext> _updateSockets = new();
    private readonly ConcurrentDictionary<Guid, WebSocketContext> _deleteSockets = new();
    
    private readonly Dictionary<WebSocketContextType, ConcurrentDictionary<Guid, WebSocketContext>> _socketDictionary = new ();
    
    private readonly JsonSerializerOptions _serializerOptions;

    public WebSocketManager(ILogger<WebSocketManager> logger)
    {
        _logger = logger;
        
        _socketDictionary.Add(WebSocketContextType.Insert, _insertSockets);
        _socketDictionary.Add(WebSocketContextType.Update, _updateSockets);
        _socketDictionary.Add(WebSocketContextType.Delete, _deleteSockets);
        
        _serializerOptions = new JsonSerializerOptions()
        {
            Converters = { new JsonStringEnumConverter() },
            PropertyNameCaseInsensitive = true,
            IncludeFields = true
        };
    }

    public async Task AddWebSocketAsync(WebSocketContext context)
    {
        context.SocketId = Guid.NewGuid();
        var sockets = _socketDictionary[context.ContextType];
        sockets.TryAdd(context.SocketId, context);
        await WebSocketListenAsync(context);
    }

    public async Task HandleDataChangeNotificationAsync(string messagePayload, CancellationToken cancellationToken)
    {
        var message = JsonSerializer.Deserialize<NotificationMessage>(messagePayload, _serializerOptions);
        if (message is null) return;
        
        var sockets = _socketDictionary[message.Operation];
        await BroadcastMessageAsync(messagePayload, sockets.Values, cancellationToken);
    }

    private void RemoveWebSocket(WebSocketContext context)
    {
        var sockets = _socketDictionary[context.ContextType];
        var removed = sockets.TryRemove(context.SocketId, out _);
        
        if (!removed || context.Socket.State == WebSocketState.Aborted) return;
        
        _logger.LogInformation(
            "{ContextType} websocket with Id {SocketId} closed and removed.", 
            context.ContextType,
            context.SocketId);
        
        context.Socket.Abort();
        context.SocketFinishedTcs.TrySetResult(context.Socket);
        context.Socket.Dispose();
    }
    
    private async Task WebSocketListenAsync(WebSocketContext context)
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(60));
        cts.Token.Register(() => { RemoveWebSocket(context); });
        
        _logger.LogInformation(
            "New websocket with Id {SocketId} added and listening for {ContextType} data changes.", 
            context.SocketId,
            context.ContextType);
        
        var buffer = new byte[1];
        var webSocket = context.Socket;
        var receiveResult = await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), cts.Token);

        while (!receiveResult.CloseStatus.HasValue && !cts.IsCancellationRequested)
        {
            cts.CancelAfter(TimeSpan.FromSeconds(60));
            receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
        }
        
        if (receiveResult.CloseStatus.HasValue)
            await context.Socket.CloseAsync(
                receiveResult.CloseStatus.Value,
                receiveResult.CloseStatusDescription,
                cts.Token);
        
        cts.Cancel();
    }

    private static async Task BroadcastMessageAsync(
        string messagePayload, 
        ICollection<WebSocketContext> contexts, 
        CancellationToken cancellationToken)
    {
        if (contexts.Count == 0) return;
        var messageBytes = Encoding.UTF8.GetBytes(messagePayload);
        var buffer = new ArraySegment<byte>(messageBytes, 0, messageBytes.Length);

        foreach (var webSocketContext in contexts)
        {
            await webSocketContext.Socket.SendAsync(
                buffer,
                WebSocketMessageType.Text,
                true,
                cancellationToken);
        }
    }
}

public record WebSocketContext(
    WebSocketContextType ContextType,
    WebSocket Socket,
    TaskCompletionSource<WebSocket> SocketFinishedTcs)
{
    internal Guid SocketId { get; set; }
}
