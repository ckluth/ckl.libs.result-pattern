@echo off
chcp 1252 >nul
setlocal

:: -------------------------------------------------------
:: all.cmd - the "proven-locally" gate: build then test,
:: both in Release, aborting on the first failure.
:: Run this green before pushing.
:: -------------------------------------------------------

set "LOCAL_BUILD=%~dp0"

echo.
echo === build ===
call "%LOCAL_BUILD%build.cmd"
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

echo.
echo === test ===
call "%LOCAL_BUILD%test.cmd"
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

echo.
echo [OK] Build - Test succeeded. State is proven locally.
exit /b 0
