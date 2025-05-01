CREATE ROLE api_role CREATEDB;

CREATE USER openlysis_api PASSWORD :'API_USER_DB_PASSWORD';
GRANT api_role TO openlysis_api;

