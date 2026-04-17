@echo off
setlocal enabledelayedexpansion

set "SCRIPT_DIR=%~dp0"
for %%I in ("%SCRIPT_DIR%..") do set "REPO_ROOT=%%~fI"
set "DEFAULT_SWAGGER_URL=http://localhost:5000/swagger/v1/swagger.json"
set "SWAGGER_URL=%~1"
if "%SWAGGER_URL%"=="" set "SWAGGER_URL=%DEFAULT_SWAGGER_URL%"
set "OUTPUT_DIR=%REPO_ROOT%\frontend\src\app\shared\proxy"
set "GENERATOR_VERSION=%GENERATOR_VERSION%"
if "%GENERATOR_VERSION%"=="" set "GENERATOR_VERSION=0.29.0"
set "GENERATOR_PACKAGE=openapi-typescript-codegen@%GENERATOR_VERSION%"
set "SWAGGER_FILE=%TEMP%\qphising-swagger-%RANDOM%%RANDOM%.json"

echo Generating TypeScript proxy from: %SWAGGER_URL%
echo Output directory: %OUTPUT_DIR%
echo Generator package: %GENERATOR_PACKAGE%

where npx >nul 2>nul
if errorlevel 1 (
  echo Error: npx is required to generate TypeScript clients.
  echo Install Node.js ^(which includes npm/npx^) and rerun this script.
  exit /b 1
)

echo Validating Swagger preconditions from: %SWAGGER_URL%
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "$ErrorActionPreference='Stop';" ^
  "$requiredPath='/api/proxy-validation/assert-sync';" ^
  "$swaggerUrl='%SWAGGER_URL%';" ^
  "$swaggerFile='%SWAGGER_FILE%';" ^
  "try {" ^
  "  $response = Invoke-WebRequest -Uri $swaggerUrl -UseBasicParsing;" ^
  "} catch {" ^
  "  Write-Error ('Failed to fetch Swagger document from ''{0}''. Ensure the API is running.' -f $swaggerUrl);" ^
  "  exit 1;" ^
  "}" ^
  "if ([string]::IsNullOrWhiteSpace($response.Content)) {" ^
  "  Write-Error ('Swagger document at ''{0}'' is empty.' -f $swaggerUrl);" ^
  "  exit 1;" ^
  "}" ^
  "try {" ^
  "  $swagger = $response.Content | ConvertFrom-Json;" ^
  "} catch {" ^
  "  Write-Error 'Swagger response is not valid JSON.';" ^
  "  exit 1;" ^
  "}" ^
  "if ((-not $swagger.openapi) -and (-not $swagger.swagger)) {" ^
  "  Write-Error 'Swagger document is missing OpenAPI/Swagger version metadata.';" ^
  "  exit 1;" ^
  "}" ^
  "if (-not $swagger.paths) {" ^
  "  Write-Error 'Swagger document is missing paths.';" ^
  "  exit 1;" ^
  "}" ^
  "if (-not $swagger.paths.PSObject.Properties.Name.Contains($requiredPath)) {" ^
  "  Write-Error ('Required path ''{0}'' was not found in Swagger. Run backend contract updates first.' -f $requiredPath);" ^
  "  exit 1;" ^
  "}" ^
  "[System.IO.File]::WriteAllText($swaggerFile, $response.Content, (New-Object System.Text.UTF8Encoding($false)));" ^
  "exit 0"
if errorlevel 1 exit /b 1

if exist "%OUTPUT_DIR%" rd /s /q "%OUTPUT_DIR%"
mkdir "%OUTPUT_DIR%"

call npx --yes "%GENERATOR_PACKAGE%" ^
  --input "%SWAGGER_FILE%" ^
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
  if exist "%SWAGGER_FILE%" del /f /q "%SWAGGER_FILE%" >nul 2>nul
  exit /b 1
)

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "$ErrorActionPreference='Stop';" ^
  "$outputDir='%OUTPUT_DIR%';" ^
  "$extensions=@('.ts','.js','.json','.md');" ^
  "if (-not (Test-Path -LiteralPath $outputDir)) { exit 0 }" ^
  "Get-ChildItem -LiteralPath $outputDir -Recurse -File | Where-Object { $extensions -contains $_.Extension.ToLowerInvariant() } | ForEach-Object {" ^
  "  $content = Get-Content -LiteralPath $_.FullName -Raw;" ^
  "  $normalized = $content -replace \"`r`n\", \"`n\";" ^
  "  if ($normalized -ne $content) {" ^
  "    [System.IO.File]::WriteAllText($_.FullName, $normalized, (New-Object System.Text.UTF8Encoding($false)));" ^
  "  }" ^
  "};" ^
  "exit 0"
if errorlevel 1 (
  if exist "%SWAGGER_FILE%" del /f /q "%SWAGGER_FILE%" >nul 2>nul
  exit /b 1
)

if exist "%SWAGGER_FILE%" del /f /q "%SWAGGER_FILE%" >nul 2>nul

echo Proxy generation completed successfully.
exit /b 0
