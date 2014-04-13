:: Installs the runtime on the device or emulator

cls
@echo off

:: set Android SDK path
set ANDROID_SDK=C:\Program Files (x86)\android-sdk-windows\tools
set PATH=%PATH%;%ANDROID_SDK%

:: set apk file to install
set APK_FILE=.\bin_android\runtime.apk

:install
:: APK INSTALL
echo Installing package... %APK_FILE%
call adb install %APK_FILE%
if errorlevel 1 goto failed2

:: echo.
:: echo Android setup installed: %APK_FILE%
:: echo.
goto end

:failed2
echo Android install FAILED.
echo.

:end
cmd
