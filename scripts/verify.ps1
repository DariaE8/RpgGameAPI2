$ErrorActionPreference = "Stop"
$prometheusUrl = "http://127.0.0.1:9090/api/v1/query"
$query = "up"

Write-Host "--- Starting Simple Diagnostics ---"

try {
    # Прямая передача URI без сложных конструкций
    $uri = $prometheusUrl + "?query=" + $query
    Write-Host "Attempting to connect to: $uri"
    
    $response = Invoke-RestMethod -Uri $uri -Method Get -ErrorAction Stop
    
    Write-Host "Successfully connected to Prometheus!"
    Write-Host "Status: $($response.status)"
    
    exit 0
}
catch {
    Write-Host "!!! CONNECTION FAILED !!!" -ForegroundColor Red
    Write-Host "Full Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}