:: AIR application packaging
:: More information:
:: http://livedocs.adobe.com/flex/3/html/help.html?content=CommandLineTools_5.html#1035959

cls
@echo off

:: Path to Flex SDK and ANDROID binaries
set FLEX_SDK=C:\flex_sdk\air+flex\bin
set PATH=%PATH%;%FLEX_SDK%

:: Path to Android SDK
set ANDROID_SDK=C:\Program Files (x86)\android-sdk-windows\
set PATH=%PATH%;%ANDROID_SDK%;%ANDROID_SDK%\tools;%ANDROID_SDK%\platform-tools\

:: certificate
set CERTIFICATE=..\certificates\SelfSigned.pfx

:certificate
set SIGNING_OPTIONS=-storetype pkcs12 -keystore %CERTIFICATE%
if not exist %CERTIFICATE% goto certificate_error

:: apk type
:apk_type_menu
echo.
echo. What kind of apk do you wish to create?
echo.
echo.  [1] apk-debug ...... for the emulator or a real device
echo.  [2] apk ............ for a real device only, no debugging/trace()
echo.  [3] apk-emulator ... for the emulator only, no debugging/trace()
echo.
goto apk_type_choice

:apk_type_choice
set /P T=[Option]:
if "%T%"=="1" ( 
	set APK_FILE=bin\AndroidProject-debug.apk
	set APK_TYPE=apk-debug
)
if "%T%"=="2" ( 
	set APK_FILE=bin\AndroidProject.apk
	set APK_TYPE=apk
)
if "%T%"=="3" ( 
	set APK_FILE=bin\AndroidProject-emulator.apk
	set APK_TYPE=apk-emulator
)
echo. You have chosen '%APK_TYPE%'.
echo. 

if exist %APK_FILE% del %APK_FILE%

:: Input
set APP_XML=application.xml 
set FILE_OR_DIR=-C bin .

echo. APK creation will take just a few seconds, please be patient
echo. --------------------------------------------------------
echo Signing APK setup using certificate %CERTIFICATE%.
call adt -package -target %APK_TYPE% %SIGNING_OPTIONS% %APK_FILE% %APP_XML% %FILE_OR_DIR%
if errorlevel 1 goto failed
echo. --------------------------------------------------------
echo. SUCCESS: APK created in (%APK_FILE%)
echo.


:: .air and .apk created, now it`S time to test you Application
:apk_menu
echo.
echo. Do you want to install and test your Application?:
echo.
echo.  [1]Yes, install Application
echo.  [2]No
echo.
goto apk_choice

:apk_choice
set /P C=[Option]:
if "%C%"=="1" goto install;
if "%C%"=="2" goto end;
goto :apk_menu

:install
:: APK INSTALL
:: echo Uninstalling app: GameAndroid
call adb uninstall GameAndroid
echo Installing package... %APK_FILE%
call adb install -r %APK_FILE%
if errorlevel 1 goto failed2

:: echo.
:: echo Android setup installed: %APK_FILE%
:: echo.
goto start_logging_menu

:start_logging_menu
echo.
echo. Do you want to start the LogCat Logger?:
echo.
echo.  [1]Yes, start Logger
echo.  [2]No
echo.
goto logging_choice

:logging_choice
set /P C=[Option]:
if "%C%"=="1" goto start_logging
if "%C%"=="2" goto end;
goto end

:start_logging
adb logcat
goto end;

:certificate_error
echo Certificate not found: %CERTIFICATE%
echo.
echo Troubleshotting: 
echo A certificate is required, generate one using 'CreateCertificate.bat'.
echo.
goto end

:failed
echo Android APK creation FAILED.
echo.
goto end

:failed2
echo Android install FAILED.
echo.

:end
pause
