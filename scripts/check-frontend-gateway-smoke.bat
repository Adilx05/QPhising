@echo off
setlocal

set "SCRIPT_DIR=%~dp0"
node "%SCRIPT_DIR%check-frontend-gateway-smoke.js" %*
