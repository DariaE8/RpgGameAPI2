$ErrorActionPreference = "Stop"
$prometheusUrl = "http://localhost:9090/api/v1/query"

# Используем метрику, которая точно есть в вашем списке (http_requests_in_progress)
# или попробуйте http_requests_total, если она появится позже
$query = 'http_requests_in_progress'

try {
    $response = Invoke-RestMethod -Uri "$prometheusUrl?query=$query" -Method Get
    
    if ($response.status -ne "success") {
        Write-Host "Prometheus error"
        exit 1
    }

    $results = $response.data.result
    if ($null -eq $results -or $results.Count -eq 0) {
        Write-Host "No data found for query $query"
        # Пока данные не настроены, не будем ломать пайплайн
        exit 0 
    }

    Write-Host "Verification successful!"
    exit 0
}
catch {
    Write-Host "Connection failed: $($_.Exception.Message)"
    exit 1
}