# https://hub.docker.com/_/microsoft-dotnet
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG TARGETARCH
WORKDIR /src

# copy everything else and build app
COPY . .
RUN dotnet restore ./src/Primal.Api/Primal.Api.csproj --arch $TARGETARCH
RUN dotnet publish ./src/Primal.Api/Primal.Api.csproj -c release -o /app --no-restore --arch $TARGETARCH

# final stage/image
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app ./

ENTRYPOINT ["dotnet", "Primal.Api.dll"]
