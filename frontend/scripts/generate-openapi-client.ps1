$ErrorActionPreference = 'Stop'

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$FrontendDir = Resolve-Path (Join-Path $ScriptDir '..')

Push-Location $FrontendDir
try {
  node ./scripts/generate-openapi-client.mjs
  if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
  }
}
finally {
  Pop-Location
}
