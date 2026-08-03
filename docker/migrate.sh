#!/usr/bin/env bash
set -euo pipefail

: "${SQL_SERVER:=sqlserver}"
: "${SQL_DATABASE:=Netrin}"
: "${SQL_USER:=sa}"
: "${SQL_PASSWORD:?SQL_PASSWORD must be set}"

sqlcmd=(/opt/mssql-tools18/bin/sqlcmd -C -S "$SQL_SERVER" -U "$SQL_USER" -P "$SQL_PASSWORD" -b)

echo "Waiting for SQL Server..."
until "${sqlcmd[@]}" -d master -Q "SELECT 1" >/dev/null 2>&1; do
  sleep 2
done

"${sqlcmd[@]}" -d master -Q "IF DB_ID(N'$SQL_DATABASE') IS NULL BEGIN CREATE DATABASE [$SQL_DATABASE]; END;"
"${sqlcmd[@]}" -d "$SQL_DATABASE" -Q "
IF OBJECT_ID(N'dbo.__SchemaMigrations', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.__SchemaMigrations
    (
        MigrationId NVARCHAR(150) NOT NULL PRIMARY KEY,
        AppliedAt DATETIME2(7) NOT NULL CONSTRAINT DF___SchemaMigrations_AppliedAt DEFAULT SYSUTCDATETIME()
    );
END;"

shopt -s nullglob
for migration in /migrations/*.up.sql; do
  filename="$(basename "$migration")"
  migration_id="${filename%.up.sql}"
  migration_id_sql="${migration_id//\'/\'\'}"
  applied="$("${sqlcmd[@]}" -d "$SQL_DATABASE" -h -1 -W -Q "SET NOCOUNT ON; IF EXISTS (SELECT 1 FROM dbo.__SchemaMigrations WHERE MigrationId = N'$migration_id_sql') SELECT 1 ELSE SELECT 0;" | tr -d '[:space:]')"

  if [[ "$applied" == "1" ]]; then
    echo "Already applied: $migration_id"
    continue
  fi

  echo "Applying: $migration_id"
  "${sqlcmd[@]}" -d "$SQL_DATABASE" -i "$migration"
  "${sqlcmd[@]}" -d "$SQL_DATABASE" -Q "INSERT INTO dbo.__SchemaMigrations (MigrationId) VALUES (N'$migration_id_sql');"
done

echo "Database migrations are up to date."
