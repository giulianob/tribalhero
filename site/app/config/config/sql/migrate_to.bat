@ECHO OFF

if "%1" == "" goto error

SET /P ANSWER=Migrate db to version %1 (Y/N)?
if /i {%ANSWER%}=={y} (goto :yes)
if /i {%ANSWER%}=={yes} (goto :yes)
goto :no

:yes
php ..\..\..\vendors\ruckusing\main.php db:migrate VERSION=%1
exit /b 0

:no
exit /b 1

:error
echo Usage "migrate_to.bat [version]"
exit /B 1