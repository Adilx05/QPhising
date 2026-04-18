@echo off
setlocal

set "SCRIPT_DIR=%~dp0"
for %%I in ("%SCRIPT_DIR%..") do set "REPO_ROOT=%%~fI"
set "SWAGGER_FIXTURE=%REPO_ROOT%\frontend\openapi\proxy-validation.swagger.json"

where git >nul 2>nul
if errorlevel 1 (
  echo Error: git is required to validate generated proxy drift.
  exit /b 1
)

call "%SCRIPT_DIR%check-swagger-quality.bat" "%SWAGGER_FIXTURE%"
if errorlevel 1 exit /b 1

call "%SCRIPT_DIR%generate-proxy.bat" "file:///%SWAGGER_FIXTURE:\=/%"
if errorlevel 1 (
  git -C "%REPO_ROOT%" checkout -- frontend/src/app/shared/proxy >nul 2>nul
  exit /b 1
)

git -C "%REPO_ROOT%" diff --quiet -- frontend/src/app/shared/proxy
if errorlevel 1 (
  echo Error: generated proxies are not in sync with %SWAGGER_FIXTURE%.
  echo Review and commit updated files under frontend/src/app/shared/proxy.
  exit /b 1
)

echo Proxy generation validation passed ^(no drift detected^).
exit /b 0
