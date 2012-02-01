@ECHO OFF

SET /P ANSWER=Migrate db to latest version (Y/N)?
if /i {%ANSWER%}=={y} (goto :yes)
if /i {%ANSWER%}=={yes} (goto :yes)
goto :no

:yes
php ..\..\..\vendors\ruckusing\main.php db:migrate
exit /b 0

:no
exit /b 1