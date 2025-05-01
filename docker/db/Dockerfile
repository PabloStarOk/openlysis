FROM postgres:17-alpine
USER postgres
WORKDIR /scripts
COPY ["scripts/*.sql", "."]
WORKDIR /
COPY ["scripts/init.sh", "/docker-entrypoint-initdb.d/"]
