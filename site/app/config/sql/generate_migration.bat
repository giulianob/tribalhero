@ECHO OFF
if "%1" == "" goto error

php ..\..\..\vendors\ruckusing\generate.php %1

exit /B 0

:error
echo Usage "generate_migration.bat [short_description]"
exit /B 1