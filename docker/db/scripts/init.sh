#!/bin/sh
# Constants
SCRIPTS_DIRPATH=/scripts
AUTH_SCRIPT=$SCRIPTS_DIRPATH/00_auth.sql
DB_SCRIPT=$SCRIPTS_DIRPATH/01_db.sql
TABLES_SCRIPT=$SCRIPTS_DIRPATH/02_tables.sql

# Create API user
psql -d "postgres" \
    -U "postgres" \
    -v API_DB_USER_NAME="$API_DB_USER_NAME" \
    -v API_USER_DB_PASSWORD="$API_USER_DB_PASSWORD" \
    -f "$AUTH_SCRIPT"

# Create API DB
psql -d "postgres" \
    -U "$API_DB_USER_NAME" \
    -v API_DB_NAME="$API_DB_NAME" \
    -f "$DB_SCRIPT"

# Create API tables
psql -d "$API_DB_NAME" \
    -U "$API_DB_USER_NAME" \
    -f "$TABLES_SCRIPT"
