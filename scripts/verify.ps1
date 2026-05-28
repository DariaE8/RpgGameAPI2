$ErrorActionPreference = "Stop"
$prometheusBaseUri = "http://127.0.0.1:9090/api/v1/query"
$query = '1 - (sum(rate(http_requests_received_total{status=~"2.."}[5m])) / sum(rate(http_requests_received_total[5m])))'

Write-Host "--- Starting Secure Verification ---"

try {
    # Передаем параметры через -Body, чтобы избежать ошибок парсинга URI
    $params = @{
        Uri    = $prometheusBaseUri
        Method = "Get"
        Body   = @{ query = $query }
    }
    
    $response = Invoke-RestMethod @params
    
    # Выводим статус для отладки
    Write-Host "Response Status: $($response.status)"
    
    # Проверяем наличие данных
    if ($null -eq $response.data.result -or $response.data.result.Count -eq 0) {
        Write-Host "Warning: No metric data found for this query!" -ForegroundColor Yellow
        exit 0
    }

    $value = $response.data.result.value[1]
    Write-Host "Calculated Error Rate: $value"
    
    exit 0
}
catch {
    Write-Host "!!! VERIFICATION FAILED !!!" -ForegroundColor Red
    Write-Host "Error details: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}