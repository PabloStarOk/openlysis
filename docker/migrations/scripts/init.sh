#!/bin/sh
# Marker file
MARKER_FILE="/app/markers/.init_done"

# Run migrations
if [ -f "$MARKER_FILE" ]; then
     echo "Skpping: initialization already performed."
else
        echo "Initializing: setting up database tables."
        
        # Run migrations
        dotnet ef database update \
          --connection "$MIGRATIONS_CONNECTION_STRING" \
          -p /app/src/Openlysis.API.Authentication/Openlysis.API.Authentication.csproj \
          -c AuthenticationDbContext \
          -s /app/src/Openlysis.API/Openlysis.API.csproj \
          --no-build

        # Create API tables
        psql -h "$API_DB_SERVER" \
          -p "$API_DB_PORT" \
          -d "$API_DB_NAME" \
          -U "$API_DB_USER" \
          -f "$MIGRATIONS_SQL_SCRIPT_PATH"

        touch $MARKER_FILE
fi

exec "$@"
