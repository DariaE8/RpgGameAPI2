$prometheusUrl = "http://localhost:9090/api/v1/query"
$query = '1 - (sum(rate(http_requests_received_total{status=~"2.."}[5m])) / sum(rate(http_requests_received_total[5m])))'

try {
    $response = Invoke-RestMethod -Uri "$prometheusUrl?query=$query" -Method Get
    
    if ($response.status -ne "success") {
        Write-Host "Error: Prometheus returned status $($response.status)" -ForegroundColor Red
        exit 1
    }

    $errorRate = $response.data.result.value[1]
    if ($null -eq $errorRate) { $errorRate = 0 }
    
    $errorRateFloat = [float]$errorRate
    Write-Host "Current Error Rate: $errorRateFloat"

    if ($errorRateFloat -gt 0.05) {
        Write-Host "Verification failed: Error rate is too high ($errorRateFloat)" -ForegroundColor Red
        exit 1
    }

    Write-Host "Verification successful!" -ForegroundColor Green
}
catch {
    Write-Host "Failed to connect to Prometheus or parse data." -ForegroundColor Red
    exit 1
}