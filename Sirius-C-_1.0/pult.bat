@echo off
title ПУЛЬТ v7 (native C#)
cd /d "%~dp0"

rem --- автосборка через фреймворковый csc (С: \Windows\...\v4.0.30319) ---
"%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:exe /platform:anycpu /optimize+ /codepage:65001 /out:"%~dp0pult.exe" /r:System.dll /r:System.Core.dll /r:System.Web.Extensions.dll /r:System.Windows.Forms.dll /r:System.Drawing.dll "%~dp0pult.cs"
if errorlevel 1 goto :err

rem --- все аргументы пробрасываются в exe: pult.bat --getme = безопасная проверка без поллинга ---
if "%~1"=="" echo (боевой запуск: python-бот должен быть остановлен, иначе 409)
"%~dp0pult.exe" %*
exit /b 0

:err
echo.
echo [!] сборка сломалась - смотри ошибки выше
pause
exit /b 1
