$ErrorActionPreference = "Stop"
$prometheusUrl = "http://localhost:9090/api/v1/query"
$query = 'up' # Самый простой запрос, который точно должен вернуть данные

Write-Host "--- Starting Diagnostics ---"

try {
    $response = Invoke-RestMethod -Uri "$prometheusUrl?query=$query" -Method Get -ErrorAction Stop
    
    Write-Host "Successfully connected to Prometheus!"
    Write-Host "Response status: $($response.status)"
    Write-Host "Results found: $($response.data.result.Count)"
    
    # Если мы здесь, значит сеть работает!
    Write-Host "Diagnostics finished successfully."
    exit 0
}
catch {
    Write-Host "!!! CRITICAL ERROR !!!" -ForegroundColor Red
    Write-Host "Message: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Details: $(if ($_.ErrorDetails) { $_.ErrorDetails.Message } else { 'No extra details' })" -ForegroundColor Red
    exit 1
}