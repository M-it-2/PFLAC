#!/bin/bash

echo "=== PFLAC DEPLOY SCRIPT ==="
echo "requirements: sudo, docker"
echo ""

### ---------------------------
### Root check
### ---------------------------
if [ "$EUID" -ne 0 ]; then
  echo "[ERROR] This script must be run with sudo"
  echo "Run it like this:"

  echo "  sudo $0"
  exit 1
fi

### ---------------------------
### Check Docker
### ---------------------------
if ! command -v docker &> /dev/null
then
    echo "[INFO] Docker not found. Installing..."

    if [ -f /etc/debian_version ]; then
        # Debian / Ubuntu
        sudo apt update
        sudo apt install -y apt-transport-https ca-certificates curl gnupg lsb-release

        curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg
        echo \
        "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu \
        $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

        sudo apt update
        sudo apt install -y docker-ce docker-ce-cli containerd.io

    elif [ -f /etc/arch-release ]; then
        # Arch Linux
        sudo pacman -Sy --needed --noconfirm docker containerd runc
        
    elif [ -f /etc/fedora-release ]; then
        # Fedora
        sudo dnf -y install dnf-plugins-core
        sudo dnf config-manager --add-repo https://download.docker.com/linux/fedora/docker-ce.repo

        sudo dnf -y install docker-ce docker-ce-cli containerd.io

    elif [ -f /etc/centos-release ] || [ -f /etc/redhat-release ]; then
        # CentOS / RHEL
        sudo yum install -y yum-utils
        sudo yum-config-manager --add-repo https://download.docker.com/linux/centos/docker-ce.repo

        sudo yum install -y docker-ce docker-ce-cli containerd.io

    else
        echo "[ERROR] Unsupported OS. Please install Docker manually."
        exit 1
    fi

    sudo systemctl enable docker >/dev/null 2>&1
    sudo systemctl start docker >/dev/null 2>&1

    sudo systemctl enable docker.socket >/dev/null 2>&1
    sudo systemctl start docker.socket >/dev/null 2>&1

    if systemctl is-active --quiet docker; then
        echo "[OK] Docker installed and started."
    else
        echo "[ERROR] Docker service failed to start!"
        echo "Try running: sudo systemctl status docker"

        exit 1
    fi

else
    echo "[OK] Docker already installed."
fi

echo ""
echo ""

### ---------------------------
### Create .env file
### ---------------------------
read -p "Type NODE_ENV (prod/dev): " NODE_ENV
read -p "Type DB_PORT (3306): " DB_PORT
read -p "Type DB_USER: " DB_USER
read -p "Type DB_NAME: " DB_NAME
read -p "Type DB_PASS: " DB_PASS

PROJECT_ROOT=$(pwd)

echo ""
echo ""

### ---------------------------
### Created .env
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

echo "[OK] File .env created:"
cat "$PROJECT_ROOT/.env"

echo ""
echo ""

echo "------------------------------"
echo ""

### ---------------------------
### Clone API
### ---------------------------
API_DIR="$PROJECT_ROOT/pflac_api"
if [ ! -d "$API_DIR" ]; then
    echo "[INFO] Cloning API..."
    git clone https://github.com/fxhxyz4/pflac_api.git "$API_DIR"
else
    echo "[INFO] API folder already exists. Skipping clone."
fi

### ---------------------------
### Create Docker network
### ---------------------------
docker network create pflac_network >/dev/null 2>&1 || true
echo "[OK] Docker pflac_network 🟢🟢🟢"

echo ""
echo ""

### ---------------------------
### Run Redis (Docker)
### ---------------------------
echo "[INFO] Run Redis..."
docker rm -f redis_local >/dev/null 2>&1 || true

echo ""

docker run -d \
  --name redis_local \
  --network pflac_network \
  -p 6379:6379 \
  redis:latest

echo "[OK] Redis running: redis_local:6379"
echo ""

### ---------------------------
### Run MySQL (Docker)
### ---------------------------
read -p "Stop local MySQL for running this script ? (type: 1 or 0): " MYSQL_QUESTION_1

if [ "$MYSQL_QUESTION_1" = "1" ]; then
  echo "[INFO] Stopping local MySQL..."

  sudo systemctl stop mysql 2>/dev/null || true
  sudo systemctl stop mariadb 2>/dev/null || true
else
  echo "[INFO] Skipping local MySQL stop"
fi

echo "[INFO] Run MySQL..."
docker rm -f mysql_local >/dev/null 2>&1 || true

SCHEMA_FILE="$API_DIR/db/scheme.sql"

if [ ! -f "$SCHEMA_FILE" ]; then
    echo "[ERROR] SQL-file $SCHEMA_FILE not found!"
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

echo "[INFO] Waiting MySQL..."

MAX_TRIES=30
TRY=1

while true; do
  if ! docker ps --format "{{.Names}}" | grep -q "^mysql_local$"; then
    echo "[ERROR] MySQL container is not running!"
    docker logs mysql_local --tail 50 2>/dev/null || true

    exit 1
  fi

  if docker exec mysql_local mysql -u"$DB_USER" -p"$DB_PASS" -e "SELECT 1;" "$DB_NAME" >/dev/null 2>&1; then
    echo "[OK] MySQL running!"
    echo ""

    break
  fi

  echo "[INFO] MySQL waiting... ($TRY/$MAX_TRIES)"
  sleep 2

  if [ $TRY -ge $MAX_TRIES ]; then
    echo "[ERROR] MySQL did not become ready in time!"
    docker logs mysql_local --tail 50 2>/dev/null || true

    exit 1
  fi

  TRY=$((TRY+1))
done


### ---------------------------
### Import schema
### ---------------------------
echo "[INFO] Import SQL-schema..."
docker exec -i mysql_local mysql -u$DB_USER -p$DB_PASS $DB_NAME < $SCHEMA_FILE
if [ $? -eq 0 ]; then
    echo "[OK] Schema imported!"
    echo ""

    echo ""
else
    echo "[ERROR] Error with schema!"
    exit 1
fi

cd ..

### ---------------------------
### Run PHPMyAdmin (Docker)
### ---------------------------
echo "[INFO] Run PHPMyAdmin..."
echo ""

echo ""
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
echo ""

### ---------------------------
### Run API local (Docker)
### ---------------------------
echo "[INFO] Create API..."
docker build -t pflac_api_image "$API_DIR"

echo "[INFO] Run API..."
docker rm -f pflac_api_local >/dev/null 2>&1 || true

cd pflac/pflac_api

docker run -d \
  --name pflac_api_local \
  --network pflac_network \
  --env-file "$PROJECT_ROOT/.env" \
  -p 8000:8000 \
  pflac_api_image

echo "[OK] API running at http://localhost:8000"
echo "[INFO] Check API status: http://localhost:8000/status/"
echo "[INFO] In field 'Server' phpMyAdmin use: mysql_local"
