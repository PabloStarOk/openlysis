FROM postgres:17-alpine

COPY ["docker/db/scripts/init.sh", "/docker-entrypoint-initdb.d/"]
RUN chmod +x /docker-entrypoint-initdb.d/init.sh

USER postgres
WORKDIR /scripts
COPY ["docker/db/scripts/*.sql", "."]
WORKDIR /
