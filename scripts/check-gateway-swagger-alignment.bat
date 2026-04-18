@echo off
setlocal

set "SCRIPT_DIR=%~dp0"

where node >nul 2>nul
if errorlevel 1 (
  echo Error: node is required to validate gateway/Swagger alignment.
  exit /b 1
)

node "%SCRIPT_DIR%check-gateway-swagger-alignment.js" %*
if errorlevel 1 exit /b 1

exit /b 0
