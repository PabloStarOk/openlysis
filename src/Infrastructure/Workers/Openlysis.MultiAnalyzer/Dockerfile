FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /tests
COPY ["tests/Openlysis.TestTools.ServicesSimulation/Openlysis.TestTools.ServicesSimulation.csproj", "Openlysis.TestTools.ServicesSimulation/"]
COPY ["tests/Openlysis.TestTools.ServicesSimulation/", "Openlysis.TestTools.ServicesSimulation/"]
WORKDIR /src
# Copy .csproj files.
COPY ["src/Infrastructure/Workers/Openlysis.MultiAnalyzer/Openlysis.MultiAnalyzer.csproj", "Infrastructure/Workers/Openlysis.MultiAnalyzer/"]
COPY ["src/Openlysis.Domain/Openlysis.Domain.csproj", "Openlysis.Domain/"]
COPY ["src/Openlysis.Application/Openlysis.Application.csproj", "Openlysis.Application/"]
COPY ["src/Infrastructure/Openlysis.Infrastructure.Shared/Openlysis.Infrastructure.Shared.csproj", "Infrastructure/Openlysis.Infrastructure.Shared/"]
COPY ["src/Infrastructure/Analyzers/Openlysis.Analyzers.Shared/Openlysis.Analyzers.Shared.csproj", "Infrastructure/Analyzers/Openlysis.Analyzers.Shared/"]
COPY ["src/Infrastructure/Analyzers/Openlysis.Analyzers.VirusTotal/Openlysis.Analyzers.VirusTotal.csproj", "Infrastructure/Analyzers/Openlysis.Analyzers.VirusTotal/"]
COPY ["src/Infrastructure/Analyzers/Openlysis.Analyzers.URLQuery/Openlysis.Analyzers.URLQuery.csproj", "Infrastructure/Analyzers/Openlysis.Analyzers.URLQuery/"]
COPY ["src/Infrastructure/Analyzers/Openlysis.Analyzers.HybridAnalysis/Openlysis.Analyzers.HybridAnalysis.csproj", "Infrastructure/Analyzers/Openlysis.Analyzers.HybridAnalysis/"]
COPY ["src/Infrastructure/Analyzers/Openlysis.Analyzers.Filescan/Openlysis.Analyzers.Filescan.csproj", "Infrastructure/Analyzers/Openlysis.Analyzers.Filescan/"]
COPY ["src/Infrastructure/Evaluators/Openlysis.Evaluators.Shared/Openlysis.Evaluators.Shared.csproj", "Infrastructure/Evaluators/Openlysis.Evaluators.Shared/"]
COPY ["src/Shared/Doppler.NET/Doppler.NET.csproj", "Shared/Doppler.NET/"]
RUN dotnet restore "Infrastructure/Workers/Openlysis.MultiAnalyzer/Openlysis.MultiAnalyzer.csproj"
# Copy project files.
COPY ["src/Openlysis.Domain/", "Openlysis.Domain/"]
COPY ["src/Openlysis.Application/", "Openlysis.Application/"]
COPY ["src/Infrastructure/Openlysis.Infrastructure.Shared", "Infrastructure/Openlysis.Infrastructure.Shared/"]
COPY ["src/Infrastructure/Workers/", "Infrastructure/Workers/"]
COPY ["src/Infrastructure/Analyzers/", "Infrastructure/Analyzers/"]
COPY ["src/Infrastructure/Evaluators/Openlysis.Evaluators.Shared/", "Infrastructure/Evaluators/Openlysis.Evaluators.Shared/"]
COPY ["src/Shared/Doppler.NET/", "Shared/Doppler.NET/"]
WORKDIR /src/Infrastructure/Workers/Openlysis.MultiAnalyzer
RUN dotnet build "Openlysis.MultiAnalyzer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS migrations
RUN dotnet tool install --global dotnet-ef --version 9.*
ENV PATH="$PATH:/root/.dotnet/tools"

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Openlysis.MultiAnalyzer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Openlysis.MultiAnalyzer.dll"]