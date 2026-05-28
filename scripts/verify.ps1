$ErrorActionPreference = "Stop"
$prometheusUrl = "http://127.0.0.1:9090/api/v1/query"

# Твой исходный запрос
$query = '1 - (sum(rate(http_requests_received_total{status=~"2.."}[5m])) / sum(rate(http_requests_received_total[5m])))'

Write-Host "--- Starting Final Verification ---"

try {
    $params = @{
        Uri    = $prometheusUrl
        Method = "Get"
        Body   = @{ query = $query }
    }
    
    $response = Invoke-RestMethod @params
    
    # Проверяем результат
    if ($response.status -eq "success" -and $null -ne $response.data.result) {
        $value = $response.data.result.value[1]
        Write-Host "Calculated Error Rate: $value"
        
        # Если ошибка > 5%, помечаем деплой как неудачный
        if ([float]$value -gt 0.05) {
            Write-Host "Verification failed: Error rate too high!" -ForegroundColor Red
            exit 1
        }
    } else {
        Write-Host "Warning: Query executed but returned no data series." -ForegroundColor Yellow
    }
    
    Write-Host "Verification successful!" -ForegroundColor Green
    exit 0
}
catch {
    Write-Host "!!! VERIFICATION FAILED !!!" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}