#!/usr/bin/env pwsh

param(
    [string]$MigrationProject = "./LeoWebApi.Persistence/LeoWebApi.Persistence.csproj",
    [string]$StartupProject   = "./LeoWebApi/LeoWebApi.csproj"
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

function Script-Migration {
    param (
        [string]$fromMigration = "0",
        [string]$toMigration,
        [string]$outputPath
    )

    if ([string]::IsNullOrWhiteSpace($toMigration)) {
        $toMigration = (dotnet ef migrations list --project $MigrationProject --startup-project $StartupProject | Select-Object -Last 1)
    }

    if ([string]::IsNullOrWhiteSpace($outputPath)) {
        $outputPath = "./migration.sql"
    }

    Write-Host "`nGenerating SQL script from migration '$fromMigration' to '$toMigration'..."
    dotnet ef migrations script $fromMigration $toMigration `
        --project $MigrationProject `
        --startup-project $StartupProject `
        --output $outputPath

    Write-Host "SQL script written to $outputPath"
}

function List-Migrations {
    Write-Host "`nAvailable migrations:"
    Write-Host "----------------------------------"
    $migrations = dotnet ef migrations list `
        --project $MigrationProject `
        --startup-project $StartupProject |
        Where-Object { $_ -match '^\d{14}_.+' }
    $i = 0
    foreach ($migration in $migrations) {
        Write-Host "[$i] $migration"
        $i++
    }
    if ($i -eq 1) {
        Write-Host "No migrations found."
    }
    Write-Host "----------------------------------"
}

Write-Host "=================================="
Write-Host "   EF Core Migration Management   "
Write-Host "=================================="
Write-Host "1) Add Migration"
Write-Host "2) Update Database"
Write-Host "3) Add + Update"
Write-Host "4) Generate SQL Migration Script"
Write-Host "5) List Migrations"
Write-Host "----------------------------------"

$choice = Read-Host "Please enter 1, 2, 3, 4 or 5"

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

    4 {
        Write-Host "`nGenerating SQL Migration Script..."
        List-Migrations
        $fromMigration = Read-Host "Enter the start migration (leave blank for '0')"
        $toMigration = Read-Host "Enter the target migration (leave blank for latest)"
        $outputPath = Read-Host "Enter output SQL file path (leave blank for './migration.sql')"
        Script-Migration $fromMigration $toMigration $outputPath
    }
    5 {
        List-Migrations
    }
    default {
        Write-Host "`nInvalid selection. Exiting..."
    }
}

Write-Host "`nDone!"
