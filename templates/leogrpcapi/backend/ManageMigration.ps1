#!/usr/bin/env pwsh

param(
    [string]$MigrationProject = "./LeoGRpcApi.Api.Persistence/LeoGRpcApi.Api.Persistence.csproj",
    [string]$StartupProject   = "./LeoGRpcApi.Api/LeoGRpcApi.Api.csproj"
)

function Add-Migration {
    param ([string]$migrationName)

    if ([string]::IsNullOrWhiteSpace($migrationName)) {
        $migrationName = "Initial"
    }

    Write-Host "`nAdding migration '$migrationName'..."
    dotnet ef migrations add $migrationName `
        --project $MigrationProject `
        --startup-project $StartupProject

}

function Update-Database {
    Write-Host "`nUpdating the database..."
    dotnet ef database update `
        --project $MigrationProject `
        --startup-project $StartupProject
}

Write-Host "=================================="
Write-Host "   EF Core Migration Management   "
Write-Host "=================================="
Write-Host "1) Add Migration"
Write-Host "2) Update Database"
Write-Host "3) Add + Update"
Write-Host "----------------------------------"

$choice = Read-Host "Please enter 1, 2 or 3"

switch ($choice)
{
    1 {
        $migrationName = Read-Host "Enter the migration name (leave blank for 'Initial')"
        Add-Migration $migrationName
    }

    2 {
        Update-Database
    }

    3 {
        $migrationName = Read-Host "Enter the migration name (leave blank for 'Initial')"
        Add-Migration $migrationName
        if ($LASTEXITCODE -eq 0) {
            Write-Host "--------------------------------------------------------------------"
            Update-Database
        } else {
            Write-Host "--------------------------------------------------------------------"
            Write-Host "Migration failed. Database update skipped."
        }
    }

    default {
        Write-Host "`nInvalid selection. Exiting..."
    }
}

Write-Host "`nDone!"
