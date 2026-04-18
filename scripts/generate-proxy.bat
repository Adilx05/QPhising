@echo off
setlocal enabledelayedexpansion

set "SCRIPT_DIR=%~dp0"
for %%I in ("%SCRIPT_DIR%..") do set "REPO_ROOT=%%~fI"
set "DEFAULT_SWAGGER_URL=https://localhost:7050/swagger/v1/swagger.json"
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
  "$requiredPaths=@('/api/proxy-validation/assert-sync','/api/configuration','/api/setup/status','/api/setup/guard-decision','/api/setup/test-db','/api/setup/test-redis','/api/setup/test-keycloak','/api/setup/save');" ^
  "$requiredOperations=@(@{Method='get';Path='/api/configuration';OperationId='Configuration_GetCurrent'},@{Method='post';Path='/api/configuration';OperationId='Configuration_Save'},@{Method='patch';Path='/api/configuration';OperationId='Configuration_Update'});" ^
  "$protectedPaths=@('/api/proxy-validation/assert-sync','/api/configuration');" ^
  "$httpMethods=@('get','post','put','patch','delete','head','options','trace');" ^
  "$requiredProblemStatusCodes=@('400','401','403','500');" ^
  "$problemDetailsSchemaRef='#/components/schemas/ProblemDetails';" ^
  "$swaggerUrl='%SWAGGER_URL%';" ^
  "$swaggerFile='%SWAGGER_FILE%';" ^
  "try {" ^
  "  $swaggerResponse = Invoke-WebRequest -Uri $swaggerUrl -UseBasicParsing;" ^
  "} catch {" ^
  "  Write-Error ('Failed to fetch Swagger document from ''{0}''. Ensure the API is running.' -f $swaggerUrl);" ^
  "  exit 1;" ^
  "}" ^
  "$swaggerContent = [string]$swaggerResponse.Content;" ^
  "if ([string]::IsNullOrWhiteSpace($swaggerContent)) {" ^
  "  Write-Error ('Swagger document at ''{0}'' is empty.' -f $swaggerUrl);" ^
  "  exit 1;" ^
  "}" ^
  "try {" ^
  "  $swagger = $swaggerContent | ConvertFrom-Json;" ^
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
  "foreach ($requiredPath in $requiredPaths) {" ^
  "  if (-not $swagger.paths.PSObject.Properties.Name.Contains($requiredPath)) {" ^
  "    Write-Error ('Required path ''{0}'' was not found in Swagger. Run backend contract updates first.' -f $requiredPath);" ^
  "    exit 1;" ^
  "  }" ^
  "}" ^
  "foreach ($requiredOperation in $requiredOperations) {" ^
  "  $pathItem = $swagger.paths.($requiredOperation.Path);" ^
  "  if (-not $pathItem) {" ^
  "    Write-Error ('Required operation ''{0} {1}'' was not found in Swagger.' -f $requiredOperation.Method.ToUpperInvariant(), $requiredOperation.Path);" ^
  "    exit 1;" ^
  "  }" ^
  "  $operation = $pathItem.($requiredOperation.Method);" ^
  "  if (-not $operation) {" ^
  "    Write-Error ('Required operation ''{0} {1}'' was not found in Swagger.' -f $requiredOperation.Method.ToUpperInvariant(), $requiredOperation.Path);" ^
  "    exit 1;" ^
  "  }" ^
  "  if ([string]$operation.operationId -ne [string]$requiredOperation.OperationId) {" ^
  "    Write-Error ('Required operation ''{0} {1}'' must use operationId ''{2}''.' -f $requiredOperation.Method.ToUpperInvariant(), $requiredOperation.Path, $requiredOperation.OperationId);" ^
  "    exit 1;" ^
  "  }" ^
  "}" ^
  "if (-not $swagger.components -or -not $swagger.components.schemas -or -not $swagger.components.schemas.ProblemDetails) {" ^
  "  Write-Error 'Swagger document is missing components.schemas.ProblemDetails required for standardized error contracts.';" ^
  "  exit 1;" ^
  "}" ^
  "$bearerSecurityScheme = $swagger.components.securitySchemes.Bearer;" ^
  "if (-not $bearerSecurityScheme -or $bearerSecurityScheme.type -ne 'http' -or $bearerSecurityScheme.scheme -ne 'bearer') {" ^
  "  Write-Error 'Swagger document is missing components.securitySchemes.Bearer (HTTP bearer) required for secured proxy generation.';" ^
  "  exit 1;" ^
  "}" ^
  "foreach ($pathProperty in $swagger.paths.PSObject.Properties) {" ^
  "  $path = $pathProperty.Name;" ^
  "  $pathItem = $pathProperty.Value;" ^
  "  foreach ($operationProperty in $pathItem.PSObject.Properties) {" ^
  "    $method = $operationProperty.Name.ToLowerInvariant();" ^
  "    if (-not $httpMethods.Contains($method)) { continue }" ^
  "    $operation = $operationProperty.Value;" ^
  "    $operationName = ('{0} {1}' -f $method.ToUpperInvariant(), $path);" ^
  "    if (-not $operation.operationId -or [string]::IsNullOrWhiteSpace([string]$operation.operationId)) {" ^
  "      Write-Error ('Operation ''{0}'' is missing a non-empty operationId.' -f $operationName);" ^
  "      exit 1;" ^
  "    }" ^
  "    if (-not $operation.responses) {" ^
  "      Write-Error ('Operation ''{0}'' is missing responses.' -f $operationName);" ^
  "      exit 1;" ^
  "    }" ^
  "    if ($protectedPaths.Contains($path)) {" ^
  "      $hasBearerSecurity = $false;" ^
  "      if ($operation.security) {" ^
  "        foreach ($securityRequirement in $operation.security) {" ^
  "          if ($securityRequirement.PSObject.Properties.Name.Contains('Bearer')) {" ^
  "            $hasBearerSecurity = $true;" ^
  "            break;" ^
  "          }" ^
  "        }" ^
  "      }" ^
  "      if (-not $hasBearerSecurity) {" ^
  "        Write-Error ('Protected operation ''{0}'' is missing Bearer security requirement.' -f $operationName);" ^
  "        exit 1;" ^
  "      }" ^
  "    }" ^
  "    foreach ($statusCode in $requiredProblemStatusCodes) {" ^
  "      $response = $operation.responses.$statusCode;" ^
  "      if (-not $response) {" ^
  "        Write-Error ('Operation ''{0}'' is missing standardized {1} response.' -f $operationName, $statusCode);" ^
  "        exit 1;" ^
  "      }" ^
  "      $problemJsonRef = $response.content.'application/problem+json'.schema.'$ref';" ^
  "      $jsonRef = $response.content.'application/json'.schema.'$ref';" ^
  "      if (($problemJsonRef -ne $problemDetailsSchemaRef) -and ($jsonRef -ne $problemDetailsSchemaRef)) {" ^
  "        Write-Error ('Operation ''{0}'' {1} response must reference {2}.' -f $operationName, $statusCode, $problemDetailsSchemaRef);" ^
  "        exit 1;" ^
  "      }" ^
  "    }" ^
  "  }" ^
  "}" ^
  "$utf8NoBom = New-Object System.Text.UTF8Encoding -ArgumentList $false;" ^
  "[System.IO.File]::WriteAllText($swaggerFile, $swaggerContent, $utf8NoBom);" ^
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
  "    $utf8NoBom = New-Object System.Text.UTF8Encoding -ArgumentList $false;" ^
  "    [System.IO.File]::WriteAllText($_.FullName, $normalized, $utf8NoBom);" ^
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
