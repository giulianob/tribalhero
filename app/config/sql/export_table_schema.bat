@ECHO OFF
if "%1" == "" goto error

mysqldump --skip-dump-date --skip-comments --create-options -d -uroot -p %1 > game.server.sql
mysqldump --skip-dump-date --skip-comments --create-options --no-create-info -uroot -p %1 schema_migrations >> game.server.sql

sed -i "s/AUTO_INCREMENT=[0-9]*\b/AUTO_INCREMENT=1/" game.server.sql

exit /B 0

:error
echo Usage "export_table_schema.bat [db-name]"
exit /B 1