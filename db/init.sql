-- Инициализация базы данных для RPG Game API
-- Этот скрипт выполняется автоматически при создании контейнера PostgreSQL

-- Создание таблиц (уже созданы через миграции Entity Framework, но добавляем проверку)
-- Вставляем начальные данные для тестирования

-- Очистка существующих данных (опционально, для чистого состояния)
TRUNCATE TABLE "Players", "Enemies", "Quests", "GameLocations", "Items" CASCADE;

-- Вставка тестовых локаций
INSERT INTO "GameLocations" ("Id", "Name", "Description", "Type", "RequiredLevel", "IsSafeZone", "CreatedAt", "UpdatedAt") VALUES
('11111111-1111-1111-1111-111111111111', 'Начальная деревня', 'Мирная деревня для новичков', 'Village', 1, true, NOW(), NOW()),
('22222222-2222-2222-2222-222222222222', 'Темный лес', 'Опасный лес, полный монстров', 'Forest', 5, false, NOW(), NOW()),
('33333333-3333-3333-3333-333333333333', 'Горная пещера', 'Глубокие пещеры с сокровищами', 'Cave', 10, false, NOW(), NOW());

-- Вставка тестовых предметов
INSERT INTO "Items" ("Id", "Name", "Description", "Type", "AttackModifier", "HealthModifier", "Value", "CreatedAt", "UpdatedAt") VALUES
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Меч новичка', 'Простой меч для начинающих', 'Weapon', 5, 0, 50, NOW(), NOW()),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'Кожаный доспех', 'Легкая защита', 'Armor', 0, 20, 75, NOW(), NOW()),
('cccccccc-cccc-cccc-cccc-cccccccccccc', 'Зелье здоровья', 'Восстанавливает 50 HP', 'Potion', 0, 50, 25, NOW(), NOW());

-- Вставка тестовых врагов
INSERT INTO "Enemies" ("Id", "Name", "Type", "Level", "Health", "MaxHealth", "Attack", "ExperienceReward", "GoldReward", "LocationId", "CreatedAt", "UpdatedAt") VALUES
('dddddddd-dddd-dddd-dddd-dddddddddddd', 'Гоблин', 1, 3, 30, 30, 8, 15, 5, '22222222-2222-2222-2222-222222222222', NOW(), NOW()),
('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', 'Орк', 2, 7, 80, 80, 15, 40, 20, '33333333-3333-3333-3333-333333333333', NOW(), NOW()),
('ffffffff-ffff-ffff-ffff-ffffffffffff', 'Скелет', 1, 5, 45, 45, 10, 25, 10, '22222222-2222-2222-2222-222222222222', NOW(), NOW());

-- Вставка тестовых игроков
INSERT INTO "Players" ("Id", "Name", "Email", "Level", "Experience", "Health", "MaxHealth", "Attack", "Gold", "LocationId", "CreatedAt", "UpdatedAt") VALUES
('99999999-9999-9999-9999-999999999999', 'Герой', 'hero@example.com', 1, 0, 100, 100, 10, 100, '11111111-1111-1111-1111-111111111111', NOW(), NOW()),
('88888888-8888-8888-8888-888888888888', 'Воин', 'warrior@example.com', 5, 1200, 150, 150, 25, 300, '22222222-2222-2222-2222-222222222222', NOW(), NOW());

-- Вставка тестовых квестов
INSERT INTO "Quests" ("Id", "Title", "Description", "Type", "RequiredLevel", "ExperienceReward", "GoldReward", "IsCompleted", "CreatedAt", "UpdatedAt") VALUES
('77777777-7777-7777-7777-777777777777', 'Охота на гоблинов', 'Убейте 5 гоблинов в Темном лесу', 'Hunting', 3, 100, 50, false, NOW(), NOW()),
('66666666-6666-6666-6666-666666666666', 'Сбор трав', 'Соберите 10 лечебных трав', 'Gathering', 2, 50, 25, false, NOW(), NOW());

-- Вывод информации о загруженных данных
SELECT 'База данных RPG Game успешно инициализирована!' AS message;