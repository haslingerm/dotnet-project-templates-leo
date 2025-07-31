#!/usr/bin/env pwsh

function Get-Architecture {
    if ([System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture -eq [System.Runtime.InteropServices.Architecture]::Arm64) {
        return "arm64"
    } else {
        return "x64"
    }
}

Push-Location -Path "LeoGRpcApi.Client"

$osName = if ($IsWindows) {
    "win"
} elseif ($IsMacOS) {
    "osx"
} elseif ($IsLinux) {
    "linux"
} else {
    Write-Error "Unknown operating system"
    exit 1
}

$arch = Get-Architecture
$rid = "$osName-$arch"

$publishDir = Join-Path -Path ".." -ChildPath "publish"

dotnet publish -c Release -r $rid -o $publishDir `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:PublishTrimmed=false `
  -p:DebugType=None `
  -p:DebugSymbols=false `
  --self-contained true

Pop-Location