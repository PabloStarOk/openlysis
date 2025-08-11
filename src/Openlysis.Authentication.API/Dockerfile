FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-alpine AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Openlysis.Authentication.API/Openlysis.Authentication.API.csproj", "Openlysis.Authentication.API/"]
COPY ["src/Openlysis.Domain/Openlysis.Domain.csproj", "Openlysis.Domain/"]
RUN dotnet restore -r linux-musl-x64 "Openlysis.Authentication.API/Openlysis.Authentication.API.csproj"
COPY ["src/Openlysis.Authentication.API/", "Openlysis.Authentication.API/"]
COPY ["src/Openlysis.Domain/", "Openlysis.Domain/"]
WORKDIR "/src/Openlysis.Authentication.API"
RUN dotnet build "Openlysis.Authentication.API.csproj" \
    --no-restore -c $BUILD_CONFIGURATION -r linux-musl-x64 \
    -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Openlysis.Authentication.API.csproj" \
    --no-restore -c $BUILD_CONFIGURATION -o /app/publish -r linux-musl-x64 --sc \
    /p:UseAppHost=true /p:PublishAot=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["/app/Openlysis.Authentication.API"]
