$ErrorActionPreference = 'Stop'

Push-Location (Join-Path $PSScriptRoot '..')
try {
    docker compose down

    $databasePath = Join-Path (Get-Location) 'data\fraud.db'

    if (Test-Path $databasePath) {
        Remove-Item $databasePath -Force
        Write-Host "Deleted Docker database: $databasePath"
    }
    else {
        Write-Host "Docker database was already clean: $databasePath"
    }

    Write-Host 'The database will be recreated with the seeded customer when Docker starts.'
}
finally {
    Pop-Location
}
