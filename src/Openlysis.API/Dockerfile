FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /tests
COPY ["tests/Openlysis.TestTools.ServicesSimulation/Openlysis.TestTools.ServicesSimulation.csproj", "Openlysis.TestTools.ServicesSimulation/"]
COPY ["tests/Openlysis.TestTools.ServicesSimulation/", "Openlysis.TestTools.ServicesSimulation/"]
WORKDIR /src
# Copy .csproj files.
COPY ["src/Openlysis.API/Openlysis.API.csproj", "Openlysis.API/"]
COPY ["src/Openlysis.API.Authentication/Openlysis.API.Authentication.csproj", "Openlysis.API.Authentication/"]
COPY ["src/Openlysis.Domain/Openlysis.Domain.csproj", "Openlysis.Domain/"]
COPY ["src/Openlysis.Application/Openlysis.Application.csproj", "Openlysis.Application/"]
COPY ["src/Infrastructure/Openlysis.Infrastructure.Shared/Openlysis.Infrastructure.Shared.csproj", "Infrastructure/Openlysis.Infrastructure.Shared/"]
COPY ["src/Infrastructure/Openlysis.Infrastructure/Openlysis.Infrastructure.csproj", "Infrastructure/Openlysis.Infrastructure/"]
COPY ["src/Infrastructure/Evaluators/Openlysis.Evaluators.Shared/Openlysis.Evaluators.Shared.csproj", "Infrastructure/Evaluators/Openlysis.Evaluators.Shared/"]
COPY ["src/Infrastructure/Evaluators/Openlysis.Evaluators.Ipqs/Openlysis.Evaluators.Ipqs.csproj", "Infrastructure/Evaluators/Openlysis.Evaluators.Ipqs/"]
COPY ["src/Infrastructure/Analyzers/Openlysis.Analyzers.Shared/Openlysis.Analyzers.Shared.csproj", "Infrastructure/Analyzers/Openlysis.Analyzers.Shared/"]
RUN dotnet restore "Openlysis.API/Openlysis.API.csproj"
# Copy project files.
COPY ["src/Openlysis.API/", "Openlysis.API/"]
COPY ["src/Openlysis.API.Authentication/", "Openlysis.API.Authentication/"]
COPY ["src/Openlysis.Domain/", "Openlysis.Domain/"]
COPY ["src/Openlysis.Application/", "Openlysis.Application/"]
COPY ["src/Infrastructure/Openlysis.Infrastructure.Shared/", "Infrastructure/Openlysis.Infrastructure.Shared/"]
COPY ["src/Infrastructure/Openlysis.Infrastructure/", "Infrastructure/Openlysis.Infrastructure/"]
COPY ["src/Infrastructure/Evaluators/", "Infrastructure/Evaluators/"]
COPY ["src/Infrastructure/Analyzers/Openlysis.Analyzers.Shared/", "Infrastructure/Analyzers/Openlysis.Analyzers.Shared/"]
WORKDIR /src/Openlysis.API
RUN dotnet build "Openlysis.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Openlysis.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Openlysis.API.dll"]
