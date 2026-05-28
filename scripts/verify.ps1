$ErrorActionPreference = "Stop"
$prometheusUrl = "http://127.0.0.1:9090/api/v1/query"
# Твой реальный запрос
$query = '1 - (sum(rate(http_requests_received_total{status=~"2.."}[5m])) / sum(rate(http_requests_received_total[5m])))'

try {
    $uri = "$prometheusUrl?query=$query"
    $response = Invoke-RestMethod -Uri $uri -Method Get -ErrorAction Stop
    
    # Выводим результат, чтобы увидеть его в логах GitHub Actions
    Write-Host "Prometheus response: $($response.data.result | ConvertTo-Json -Depth 5)"
    
    # Теперь проверяем само значение
    $value = $response.data.result.value[1]
    Write-Host "Calculated Error Rate: $value"
    
    # Если значение есть и оно меньше 0.05 — всё супер
    if ($null -ne $value -and [float]$value -lt 0.05) {
        Write-Host "Verification successful!" -ForegroundColor Green
        exit 0
    } else {
        Write-Host "Verification failed: Error rate is high or no data!" -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host "!!! VERIFICATION FAILED !!!" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}