@echo off
setlocal

set "SCRIPT_DIR=%~dp0"
for %%I in ("%SCRIPT_DIR%..") do set "REPO_ROOT=%%~fI"
set "SWAGGER_SOURCE=%~1"
if "%SWAGGER_SOURCE%"=="" set "SWAGGER_SOURCE=%REPO_ROOT%\frontend\openapi\proxy-validation.swagger.json"

echo Running Swagger quality gate: %SWAGGER_SOURCE%

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "$ErrorActionPreference='Stop';" ^
  "$requiredPaths=@('/api/proxy-validation/assert-sync','/api/configuration','/api/setup/status','/api/setup/guard-decision','/api/setup/test-db','/api/setup/test-redis','/api/setup/test-keycloak','/api/setup/save');" ^
  "$httpMethods=@('get','post','put','patch','delete','head','options','trace');" ^
  "$requiredProblemStatusCodes=@('400','401','403','500');" ^
  "$problemDetailsSchemaRef='#/components/schemas/ProblemDetails';" ^
  "$source='%SWAGGER_SOURCE%';" ^
  "if ($source.StartsWith('http://') -or $source.StartsWith('https://')) {" ^
  "  try {" ^
  "    $content = [string](Invoke-WebRequest -Uri $source -UseBasicParsing).Content;" ^
  "  } catch {" ^
  "    Write-Error ('Failed to fetch Swagger from ''{0}''.' -f $source);" ^
  "    exit 1;" ^
  "  }" ^
  "} else {" ^
  "  if ($source.StartsWith('file://')) {" ^
  "    $source = $source.Substring(7);" ^
  "  }" ^
  "  if (-not (Test-Path -LiteralPath $source -PathType Leaf)) {" ^
  "    Write-Error ('Swagger file not found at ''{0}''.' -f $source);" ^
  "    exit 1;" ^
  "  }" ^
  "  $content = Get-Content -LiteralPath $source -Raw;" ^
  "}" ^
  "if ([string]::IsNullOrWhiteSpace($content)) {" ^
  "  Write-Error 'Swagger document is empty.';" ^
  "  exit 1;" ^
  "}" ^
  "try {" ^
  "  $swagger = $content | ConvertFrom-Json;" ^
  "} catch {" ^
  "  Write-Error 'Swagger document is not valid JSON.';" ^
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
  "    Write-Error ('Required path ''{0}'' was not found in Swagger.' -f $requiredPath);" ^
  "    exit 1;" ^
  "  }" ^
  "}" ^
  "if (-not $swagger.components -or -not $swagger.components.schemas -or -not $swagger.components.schemas.ProblemDetails) {" ^
  "  Write-Error 'Swagger document is missing components.schemas.ProblemDetails.';" ^
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
  "Write-Host 'Swagger quality gate passed.'"
if errorlevel 1 exit /b 1

exit /b 0
