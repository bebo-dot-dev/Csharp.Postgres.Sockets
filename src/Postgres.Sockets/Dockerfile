FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# copy csproj files and restore
COPY src/Postgres.Sockets/*.csproj src/Postgres.Sockets/
COPY src/Postgres.Sockets.Core/*.csproj src/Postgres.Sockets.Core/
COPY src/Postgres.Sockets.Database/*.csproj src/Postgres.Sockets.Database/
RUN dotnet restore "src/Postgres.Sockets/Postgres.Sockets.csproj"

# copy everything else and build app
COPY src/. src/
WORKDIR /src/src/Postgres.Sockets
RUN dotnet publish -c release -o /app --no-restore

# final stage/image
FROM base AS final
WORKDIR /app
COPY --from=build /app ./

ENTRYPOINT ["dotnet", "Postgres.Sockets.dll"]