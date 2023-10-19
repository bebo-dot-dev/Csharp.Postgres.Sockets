namespace Postgres.Sockets.Core;

// Root myDeserializedClass = JsonConvert.DeserializeObject<NotificationMessage>(myJsonResponse);

public record NotificationMessage(
    string Table,
    WebSocketContextType Operation,
    TableData Data
);

public record TableData(
    int TestEntityId,
    string Name
);

