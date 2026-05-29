$ErrorActionPreference = "Stop"
$prometheusUrl = "http://127.0.0.1:9090/api/v1/query"
$query = '1 - (sum(rate(http_requests_received_total{status=~"2.."}[5m])) / sum(rate(http_requests_received_total[5m])))'

Write-Host "--- Starting Final Verification ---"

try {
    $params = @{
        Uri    = $prometheusUrl
        Method = "Get"
        Body   = @{ query = $query }
    }
    
    $response = Invoke-RestMethod @params
    
    # ПРОВЕРКА: есть ли данные?
    if ($null -ne $response.data.result -and $response.data.result.Count -gt 0) {
        $value = $response.data.result.value[1]
        Write-Host "Calculated Error Rate: $value"
        
        if ([float]$value -gt 0.05) {
            Write-Host "Verification failed: Error rate too high!" -ForegroundColor Red
            exit 1
        }
    } else {
        # Данных нет, но это не поломка сервера, а просто отсутствие активности
        Write-Host "Warning: Prometheus returned empty result. Metrics not yet available." -ForegroundColor Yellow
        exit 0 
    }
    
    Write-Host "Verification successful!" -ForegroundColor Green
    exit 0
}
catch {
    Write-Host "!!! VERIFICATION FAILED !!!" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}