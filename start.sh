#!/bin/bash

echo "========================================="
echo "Запуск RPG Game API с Docker Compose"
echo "========================================="

echo "Очистка старых контейнеров (опционально)..."
docker-compose down

echo "Сборка и запуск базы данных и сервера RPG Game API..."
docker-compose up --build -d

echo "Ожидание запуска сервисов..."
sleep 10

echo "Проверка состояния контейнеров..."
docker-compose ps

echo ""
echo "========================================="
echo "Сервисы успешно запущены!"
echo "========================================="
echo "Сервер доступен по адресу: http://localhost:8080"
echo "Swagger UI: http://localhost:8080/swagger"
echo ""
echo "База данных PostgreSQL:"
echo "  Хост: localhost:5432"
echo "  База данных: ${POSTGRES_DB:-rpggame}"
echo "  Пользователь: ${POSTGRES_USER:-postgres}"
echo ""
echo "Для просмотра логов используй команду: docker-compose logs -f"
echo "Для остановки: docker-compose down"
echo "========================================="