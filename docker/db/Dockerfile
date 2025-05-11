FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# Copy .csproj files.
WORKDIR /app/src
COPY ["src/Openlysis.API/Openlysis.API.csproj", "Openlysis.API/"]
COPY ["src/Openlysis.API.Authentication/Openlysis.API.Authentication.csproj", "Openlysis.API.Authentication/"]
COPY ["src/Openlysis.Domain/Openlysis.Domain.csproj", "Openlysis.Domain/"]
COPY ["src/Openlysis.Application/Openlysis.Application.csproj", "Openlysis.Application/"]
COPY ["src/Infrastructure/Openlysis.Infrastructure.Shared/Openlysis.Infrastructure.Shared.csproj", "Infrastructure/Openlysis.Infrastructure.Shared/"]
COPY ["src/Infrastructure/Openlysis.Infrastructure/Openlysis.Infrastructure.csproj", "Infrastructure/Openlysis.Infrastructure/"]
COPY ["src/Infrastructure/Evaluators/Openlysis.Evaluators.Shared/Openlysis.Evaluators.Shared.csproj", "Infrastructure/Evaluators/Openlysis.Evaluators.Shared/"]
COPY ["src/Infrastructure/Evaluators/Openlysis.Evaluators.Ipqs/Openlysis.Evaluators.Ipqs.csproj", "Infrastructure/Evaluators/Openlysis.Evaluators.Ipqs/"]
RUN dotnet restore "Openlysis.API/Openlysis.API.csproj"
# Copy project files.
COPY ["src/Openlysis.API/", "Openlysis.API/"]
COPY ["src/Openlysis.API.Authentication/", "Openlysis.API.Authentication/"]
COPY ["src/Openlysis.Domain/", "Openlysis.Domain/"]
COPY ["src/Openlysis.Application/", "Openlysis.Application/"]
COPY ["src/Infrastructure/Openlysis.Infrastructure.Shared/", "Infrastructure/Openlysis.Infrastructure.Shared/"]
COPY ["src/Infrastructure/Openlysis.Infrastructure/", "Infrastructure/Openlysis.Infrastructure/"]
COPY ["src/Infrastructure/Evaluators/", "Infrastructure/Evaluators/"]

WORKDIR /app/src/Openlysis.API
RUN dotnet add package Microsoft.EntityFrameworkCore.Design
RUN dotnet build --no-restore

WORKDIR /app/src
RUN dotnet tool install --global dotnet-ef \
    && ln -s /root/.dotnet/tools/dotnet-ef /usr/local/bin/dotnet-ef
RUN dotnet ef migrations script \
    --project Openlysis.API.Authentication \
    -s Openlysis.API \
    -i \
    -o /app/02_migrations.sql \
    --no-build

FROM postgres:17-alpine

COPY ["docker/db/scripts/init.sh", "/docker-entrypoint-initdb.d/"]
RUN chmod +x /docker-entrypoint-initdb.d/init.sh

USER postgres
WORKDIR /scripts
COPY ["docker/db/scripts/*.sql", "."]
COPY --from=build ["/app/02_migrations.sql", "."]
WORKDIR /
