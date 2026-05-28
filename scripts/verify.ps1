$ErrorActionPreference = "Stop"
# Используем прямой IP-адрес для исключения проблем с резолвингом localhost
$prometheusUrl = "http://127.0.0.1:9090/api/v1/query"

Write-Host "--- Starting Diagnostic Verification ---"

try {
    # Прямая передача запроса через параметры для безопасности
    $params = @{
        Uri    = $prometheusUrl
        Method = "Get"
        # Запрос 'up' возвращает 1, если таргет доступен
        Body   = @{ query = "up" }
    }
    
    $response = Invoke-RestMethod @params
    
    Write-Host "Successfully connected to Prometheus!"
    Write-Host "Result count: $($response.data.result.Count)"
    
    exit 0
}
catch {
    Write-Host "!!! VERIFICATION FAILED !!!" -ForegroundColor Red
    Write-Host "Exception Message: $($_.Exception.Message)" -ForegroundColor Red
    # Если есть детали ошибки, выводим их
    if ($_.ErrorDetails) {
        Write-Host "Error Details: $($_.ErrorDetails.Message)" -ForegroundColor Red
    }
    exit 1
}