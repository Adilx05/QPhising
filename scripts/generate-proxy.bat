@echo off
setlocal enabledelayedexpansion

set "SCRIPT_DIR=%~dp0"
for %%I in ("%SCRIPT_DIR%..") do set "REPO_ROOT=%%~fI"
set "DEFAULT_SWAGGER_URL=http://localhost:5000/swagger/v1/swagger.json"
set "SWAGGER_URL=%~1"
if "%SWAGGER_URL%"=="" set "SWAGGER_URL=%DEFAULT_SWAGGER_URL%"
set "OUTPUT_DIR=%REPO_ROOT%\frontend\src\app\shared\proxy"

echo Generating TypeScript proxy from: %SWAGGER_URL%
echo Output directory: %OUTPUT_DIR%

where npx >nul 2>nul
if errorlevel 1 (
  echo Error: npx is required to generate TypeScript clients.
  echo Install Node.js (which includes npm/npx) and rerun this script.
  exit /b 1
)

if exist "%OUTPUT_DIR%" rd /s /q "%OUTPUT_DIR%"
mkdir "%OUTPUT_DIR%"

call npx --yes openapi-typescript-codegen ^
  --input "%SWAGGER_URL%" ^
  --output "%OUTPUT_DIR%" ^
  --client axios ^
  --useOptions ^
  --exportCore true ^
  --exportServices true ^
  --exportModels true ^
  --indent 2
if errorlevel 1 exit /b 1

if not exist "%OUTPUT_DIR%\index.ts" (
  echo Error: proxy generation did not produce %OUTPUT_DIR%\index.ts
  exit /b 1
)

echo Proxy generation completed successfully.
exit /b 0
