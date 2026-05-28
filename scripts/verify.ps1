Write-Host "--- Запуск проверки деплоя (Acceptance Gate) ---"
# Даем контейнерам 10 секунд, чтобы они успели полностью подняться
Start-Sleep -Seconds 10 

# 1. Проверка, отвечает ли само приложение
try {
    $response = Invoke-WebRequest -Uri "http://localhost/health" -Method Get -TimeoutSec 5
    Write-Host "Приложение доступно. Статус: $($response.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "ОШИБКА: Приложение не отвечает по адресу http://localhost/health" -ForegroundColor Red
    exit 1
}

# 2. Проверка уровеня ошибок через API Prometheus
# Запрашиваем среднюю скорость ошибок 5xx за последнюю минуту
$prometheusUrl = "http://localhost:9090/api/v1/query?query=rate(http_requests_received_total{code=~'5..'}[1m])"

try {
    $promResponse = Invoke-RestMethod -Uri $prometheusUrl -Method Get
    
    $errorRate = 0
    if ($promResponse.data.result.Count -gt 0) {
        # Извлекаем числовое значение из ответа Prometheus
        $errorRate = [double]($promResponse.data.result[0].value[1])
    }

    Write-Host "Текущий уровень ошибок (5xx): $errorRate"

    # Если ошибок больше 0.05 в секунду (это наш порог качества)
    if ($errorRate -gt 0.05) {
        Write-Host "ВЕРИФИКАЦИЯ НЕ ПРОЙДЕНА: Слишком много ошибок!" -ForegroundColor Red
        exit 1
    } else {
        Write-Host "ВЕРИФИКАЦИЯ ПРОЙДЕНА: Уровень ошибок в пределах нормы." -ForegroundColor Green
        exit 0
    }
} catch {
    Write-Host "ОШИБКА: Не удалось получить данные из Prometheus." -ForegroundColor Red
    exit 1
}