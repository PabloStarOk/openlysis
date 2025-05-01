FROM mcr.microsoft.com/dotnet/sdk:8.0

# PostgreSQL CLI
RUN apt-get update \
    && apt-get install -y postgresql-client \
    && rm -rf /var/lib/apt/lists/* 

# Dotnet tools
RUN dotnet tool install --global dotnet-ef \
    && ln -s /root/.dotnet/tools/dotnet-ef /usr/local/bin/dotnet-ef 

WORKDIR /app

# Copy scripts
COPY ["docker/migrations/scripts/", "scripts/"]
RUN chmod +x scripts/init.sh

# Copy project files
COPY ["src/", "src/"]
WORKDIR /app/src/Openlysis.API

RUN dotnet restore
RUN dotnet add package Microsoft.EntityFrameworkCore.Design
RUN dotnet build --no-restore

ENTRYPOINT ["/app/scripts/init.sh"]
