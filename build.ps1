# Determine the expected CLI version
$globalJsonPath = Join-Path $PSScriptRoot global.json
$sdk = Get-Content $globalJsonPath | ConvertFrom-Json | Select-Object -ExpandProperty sdk

$dotnetArch = 'x64'
$dotnetLocalInstallFolder = $env:DOTNET_INSTALL_DIR
if (!$dotnetLocalInstallFolder) {
    $dotnetLocalInstallFolder = "$env:USERPROFILE\.dotnet\$dotnetArch\"
}

# Check if we have the CLI and if it's the right version
Write-Host -ForegroundColor Green "Checking .NET CLI"
$haveCli = !!(Get-Command dotnet -ErrorAction SilentlyContinue)
if (!$haveCli -or ("$(dotnet --version)" -ne $sdk.version)) {
    # Install that version of the CLI
    $installScript = Join-Path (Join-Path $PSScriptRoot "build") "dotnet-install.ps1"
    & $installScript -Channel $sdk.channel -Version $sdk.version -InstallDir $dotnetLocalInstallFolder -Architecture $dotnetArch

    # Verify we're good now
    $haveCli = !!(Get-Command dotnet -ErrorAction SilentlyContinue)
    if (!$haveCli -or ("$(dotnet --version)" -ne $sdk.version)) {
        throw "Failed to set up .NET CLI"
    }
}

# Restore!
Write-Host -ForegroundColor Green "Restoring Packages"
dotnet restore

# Build!
Write-Host -ForegroundColor Green "Building"
dotnet build

# Publish apps
$ArtifactsRoot = Join-Path $PSScriptRoot "artifacts"
$ArtifactsPath = Join-Path $ArtifactsRoot "windows"
@("FrontEnd", "BackEnd") | ForEach-Object {
    $Project = Join-Path (Join-Path (Join-Path $PSScriptRoot "src") $_) "$_.csproj"
    $Output = Join-Path $ArtifactsPath $_
    Write-Host -ForegroundColor Green "Publishing $_"
    dotnet publish $Project -o $Output
}

Write-Host -ForegroundColor Green "Build Succeeded"