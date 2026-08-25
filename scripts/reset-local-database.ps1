$ErrorActionPreference = 'Stop'

$apiProcesses = Get-CimInstance Win32_Process |
    Where-Object {
        $_.CommandLine -like '*FraudRuleEngine.Api*' -and
        $_.ProcessId -ne $PID
    }

foreach ($process in $apiProcesses) {
    Stop-Process -Id $process.ProcessId -Force -ErrorAction SilentlyContinue
}

$databasePath = Join-Path $PSScriptRoot '..\FraudRuleEngine.Api\data\fraud.db'
$databasePath = [System.IO.Path]::GetFullPath($databasePath)

if (Test-Path $databasePath) {
    Remove-Item $databasePath -Force
    Write-Host "Deleted local database: $databasePath"
}
else {
    Write-Host "Local database was already clean: $databasePath"
}

Write-Host 'The database will be recreated with the seeded customer on the next local API startup.'
