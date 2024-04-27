# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy-arm64v8 AS build
WORKDIR /src

# copy everything else and build app
COPY . .
RUN dotnet restore ./src/Primal.Api/Primal.Api.csproj
RUN dotnet publish ./src/Primal.Api/Primal.Api.csproj -c release -o /app --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy-arm64v8
WORKDIR /app
COPY --from=build /app ./

ENTRYPOINT ["dotnet", "Primal.Api.dll"]
