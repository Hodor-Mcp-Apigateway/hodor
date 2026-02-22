# Hodor API - PowerShell sample (Windows, Mac, Linux)
# Run: .\powershell.ps1  or  pwsh powershell.ps1

$baseUrl = if ($env:HODOR_URL) { $env:HODOR_URL } else { "http://localhost:8080" }

Write-Host "=== Hodor MCP Gateway - PowerShell sample ===" -ForegroundColor Cyan
Write-Host "Base URL: $baseUrl`n"

@("/health", "/ready", "/api/tools") | ForEach-Object {
    try {
        $r = Invoke-RestMethod -Uri "$baseUrl$_" -Method Get
        Write-Host "$_ : $($r | ConvertTo-Json -Compress | Select-Object -First 80)..." -ForegroundColor Green
    } catch {
        Write-Host "$_ : Error - $_" -ForegroundColor Red
    }
}
