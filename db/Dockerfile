FROM postgres:15-alpine
# Копируем скрипт инициализации. Он автоматически выполнится при создании базы.
COPY init.sql /docker-entrypoint-initdb.d/