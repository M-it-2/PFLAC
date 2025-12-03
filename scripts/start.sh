#!/bin/bash

echo "=== PFLAC DEPLOY SCRIPT ==="
echo "requirements: sudo, docker"
echo ""

### ---------------------------
### 1. Ввод данных для .env
### ---------------------------
read -p "Введите NODE_ENV (prod/dev): " NODE_ENV
read -p "Введите DB_USER: " DB_USER
read -p "Введите DB_NAME: " DB_NAME
read -p "Введите DB_PASS: " DB_PASS
read -p "Введите DB_PORT (например 3306 или 3307, если локальный MySQL занят): " DB_PORT

### ---------------------------
### 2. Создаём .env
### ---------------------------
cd ..

cat <<EOF > .env
NODE_ENV=$NODE_ENV
DB_USER=$DB_USER
DB_NAME=$DB_NAME
DB_PASS=$DB_PASS
DB_PORT=$DB_PORT
DB_SERVER=mysql_local

# REDIS CONFIG
REDIS_KEY=app_state
REDIS_CONNECTION=redis://redis_local:6379
EOF

echo "[OK] Файл .env создан:"
cat .env
echo "------------------------------"

### ---------------------------
### 3. Клонируем API
### ---------------------------
if [ ! -d "pflac_api" ]; then
    echo "[INFO] Клонируем API..."
    git clone https://github.com/fxhxyz4/pflac_api.git
else
    echo "[INFO] API уже клонирован."
fi

### ---------------------------
### 4. Создаём сеть Docker
### ---------------------------
docker network create pflac_network >/dev/null 2>&1 || true
echo "[OK] Docker сеть pflac_network готова."

### ---------------------------
### 5. Запуск Redis (Docker)
### ---------------------------
echo "[INFO] Запуск Redis..."
docker rm -f redis_local >/dev/null 2>&1 || true

docker run -d \
  --name redis_local \
  --network pflac_network \
  -p 6379:6379 \
  redis:latest

echo "[OK] Redis запущен: redis_local:6379"

### ---------------------------
### 6. Запуск MySQL (Docker)
### ---------------------------
echo "[INFO] Запуск MySQL..."
docker rm -f mysql_local >/dev/null 2>&1 || true

cd pflac_api
SCHEMA_FILE="./db/scheme.sql"

if [ ! -f "$SCHEMA_FILE" ]; then
    echo "[ERROR] SQL-файл $SCHEMA_FILE не найден!"
    exit 1
fi

docker run -d \
  --name mysql_local \
  --network pflac_network \
  -e MYSQL_ROOT_PASSWORD=root \
  -e MYSQL_DATABASE=$DB_NAME \
  -e MYSQL_USER=$DB_USER \
  -e MYSQL_PASSWORD=$DB_PASS \
  -p $DB_PORT:3306 \
  mysql:8 --disable-log-bin

echo "[INFO] Ожидаем запуска MySQL..."
until docker exec mysql_local mysql -u$DB_USER -p$DB_PASS -e "SELECT 1;" $DB_NAME >/dev/null 2>&1; do
    echo "MySQL ещё не готов... ждём 2 сек"
    sleep 2
done
echo "[OK] MySQL готов!"

### ---------------------------
### 6.1 Импорт схемы
### ---------------------------
echo "[INFO] Импортируем SQL-схему..."
docker exec -i mysql_local mysql -u$DB_USER -p$DB_PASS $DB_NAME < $SCHEMA_FILE
if [ $? -eq 0 ]; then
    echo "[OK] Схема успешно импортирована!"
else
    echo "[ERROR] Ошибка при импорте схемы!"
    exit 1
fi

cd ..

### ---------------------------
### 7. Запуск PHPMyAdmin (Docker)
### ---------------------------
echo "[INFO] Запуск PHPMyAdmin..."
docker rm -f phpmyadmin_local >/dev/null 2>&1 || true

docker run -d \
  --name phpmyadmin_local \
  --network pflac_network \
  -e PMA_HOST=mysql_local \
  -e PMA_PORT=3306 \
  -e PMA_ARBITRARY=1 \
  -p 8080:80 \
  phpmyadmin/phpmyadmin:latest

echo ""
echo "[OK] PHPMyAdmin: http://localhost:8080"
echo "!!! В поле 'Сервер' phpMyAdmin используйте: mysql_local"
echo ""

### ---------------------------
### 8. Запуск локального API (Docker)
### ---------------------------
echo "[INFO] Строим образ API..."
docker build -t pflac_api_image ./pflac_api

echo "[INFO] Запуск API..."
docker rm -f pflac_api_local >/dev/null 2>&1 || true

docker run -d \
  --name pflac_api_local \
  --network pflac_network \
  --env-file .env \
  -p 8000:8000 \
  pflac_api_image

echo "[OK] API запущен: http://localhost:8000"
echo "[INFO] Проверить статус API: http://localhost:8000/status/"
