## Postgres.Sockets
A .NET6 web socket server integration with Postgres.

[![build-and-test](https://github.com/bebo-dot-dev/Csharp.Postgres.Sockets/actions/workflows/dotnet.yml/badge.svg)](https://github.com/bebo-dot-dev/Csharp.Postgres.Sockets/actions/workflows/dotnet.yml)

### Nuget packages used
```
<PackageReference Include="Mediator.SourceGenerator" Version="2.1.7" />
<PackageReference Include="Microsoft.CodeAnalysis" Version="4.7.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="7.0.11" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="7.0.11" />
<PackageReference Include="Microsoft.Net.Compilers.Toolset" Version="4.7.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
<PackageReference Include="Mediator.Abstractions" Version="2.1.7" />
<PackageReference Include="Riok.Mapperly" Version="3.2.0" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="7.0.11" />
```

### Project Layout
The projects in this solution are structured in a hexagonal ports and adaptors style enabled by `Mediator.SourceGenerator`

### Docker
There is a `docker-compose.yml` YAML file included that references the `Dockerfile` in the `Postgres.Sockets` API project.
The API has a dependency on a Postgres database and the `postgres` Container / Docker image
is included as the `postgres-db` service in the `docker-compose.yml`.

Docker and docker-compose run and debug were tested in Jetbrains Rider 2023.2.2

### EF Core
The `Postgres.Sockets.Database` project contains migrations and the application is setup to apply migrations at runtime.

### API
The goal of this project is to demonstrate .NET EF Core interaction with a Postgres database and client web socket message
notifications of data changes that are applied within the Postgres database. The includes controller is REST-like
but not strictly REST. The general concept modelled in the API is a single database "TestEntity" compromised of a
TestEntityId and a Name purely for the purposes of demonstrating database and web sockets behaviour. 

### Interacting with the API
The `Postgres.Sockets` API is Swashbuckle Swagger enabled and there is also a postman collection included in the `/test` directory.

### Interacting with Web Socket messages
The `Postgres.Sockets` includes an `index.html` page in the `wwwroot` directory that enables a user to connect a 
websocket to the API `/ws` endpoint. The `/ws` API endpoint enforces that a `WebSocketContextType` be passed in the 
websocket connect URI path to give socket connections context. Possible values for `WebSocketContextType` are:

```
public enum WebSocketContextType
{
    Insert = 0,
    Update,
    Delete
}
```

Once websockets are connected to the socket server with a given `WebSocketContextType`, data changes that are applied in 
the database of that data context type are broadcast to connected web socket clients.

![sockets screenshot](media/sockets.png?raw=true "Screenshot")
