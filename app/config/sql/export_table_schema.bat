@ECHO OFF
if "%1" == "" goto error

mysqldump -d -uroot -p %1 > game.server.sql
mysqldump --no-create-info -uroot -p %1 schema_migrations >> game.server.sql

exit /B 0

:error
echo Usage "export_table_schema.bat [db-name]"
exit /B 1