#!/bin/bash

# ============================================================
# Gridiron Database Deployment Script (Linux/Mac)
# ============================================================
# This script will:
# 1. Apply EF Core migrations to create database schema
# 2. Run seed data to populate teams and players
# ============================================================

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
CYAN='\033[0;36m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Parse arguments
CONNECTION_STRING=""
USE_USER_SECRETS=false
SKIP_SEED_DATA=false

while [[ $# -gt 0 ]]; do
    case $1 in
        --connection-string)
            CONNECTION_STRING="$2"
            shift 2
            ;;
        --use-user-secrets)
            USE_USER_SECRETS=true
            shift
            ;;
        --skip-seed-data)
            SKIP_SEED_DATA=true
            shift
            ;;
        *)
            echo -e "${RED}Unknown parameter: $1${NC}"
            exit 1
            ;;
    esac
done

function write_section() {
    echo ""
    echo -e "${CYAN}============================================================${NC}"
    echo -e "${CYAN}  $1${NC}"
    echo -e "${CYAN}============================================================${NC}"
    echo ""
}

# Navigate to DataAccessLayer
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
DATA_ACCESS_LAYER_PATH="$SCRIPT_DIR/DataAccessLayer"

if [ ! -d "$DATA_ACCESS_LAYER_PATH" ]; then
    echo -e "${RED}ERROR: DataAccessLayer directory not found at $DATA_ACCESS_LAYER_PATH${NC}"
    exit 1
fi

cd "$DATA_ACCESS_LAYER_PATH"

# ============================================================
# Step 1: Configure Connection String
# ============================================================
write_section "Step 1: Configuring Connection String"

if [ "$USE_USER_SECRETS" = true ]; then
    echo -e "${CYAN}Using User Secrets for connection string...${NC}"
    echo -e "${GREEN}? User Secrets ID: gridiron-football-sim-2024${NC}"
elif [ -n "$CONNECTION_STRING" ]; then
    echo -e "${CYAN}Setting connection string in User Secrets...${NC}"
    dotnet user-secrets set "ConnectionStrings:GridironDb" "$CONNECTION_STRING"
    if [ $? -ne 0 ]; then
        echo -e "${RED}ERROR: Failed to set connection string in User Secrets${NC}"
        exit 1
    fi
    echo -e "${GREEN}? Connection string configured${NC}"
else
    echo -e "${YELLOW}WARNING: No connection string provided!${NC}"
    echo ""
    echo "Please provide connection string using one of these methods:"
    echo "  1. Run with --connection-string parameter:"
    echo "     ./deploy-database.sh --connection-string \"Server=...\""
    echo ""
    echo "  2. Or manually set it first, then run with --use-user-secrets:"
    echo "     cd DataAccessLayer"
    echo "     dotnet user-secrets set \"ConnectionStrings:GridironDb\" \"Server=...\""
    echo "     cd .."
    echo "     ./deploy-database.sh --use-user-secrets"
    echo ""
    exit 1
fi

# ============================================================
# Step 2: Apply EF Core Migrations
# ============================================================
write_section "Step 2: Applying Database Migrations"

echo -e "${CYAN}Running: dotnet ef database update${NC}"
dotnet ef database update

if [ $? -ne 0 ]; then
    echo -e "${RED}ERROR: Migration failed!${NC}"
    echo ""
    echo "Common issues:"
    echo "  - Connection string is incorrect"
    echo "  - Azure SQL firewall blocks your IP address"
    echo "  - SQL authentication is not enabled"
    echo "  - Database credentials are wrong"
    echo ""
    exit 1
fi

echo -e "${GREEN}? Database schema created successfully${NC}"

# ============================================================
# Step 3: Seed Data (Optional)
# ============================================================
if [ "$SKIP_SEED_DATA" = false ]; then
    write_section "Step 3: Seeding Initial Data"
    
    echo -e "${CYAN}Running: dotnet run --project .${NC}"
    dotnet run --project .
    
    if [ $? -ne 0 ]; then
        echo -e "${RED}ERROR: Data seeding failed!${NC}"
        echo ""
        echo "Database schema was created successfully, but seeding failed."
        echo "You can manually run seeding later by executing:"
        echo "  cd DataAccessLayer"
        echo "  dotnet run"
        echo ""
        exit 1
    fi
    
    echo -e "${GREEN}? Data seeding completed successfully${NC}"
else
    echo -e "${YELLOW}Skipping data seeding (--skip-seed-data flag provided)${NC}"
fi

# ============================================================
# Summary
# ============================================================
write_section "Deployment Complete!"

echo -e "${GREEN}? Database schema created${NC}"
if [ "$SKIP_SEED_DATA" = false ]; then
    echo -e "${GREEN}? Initial data seeded (Falcons and Eagles)${NC}"
fi
echo -e "${GREEN}? Database is ready to use${NC}"

echo ""
echo "Next steps:"
echo "  1. Test the database connection in Visual Studio"
echo "  2. Run your application: dotnet run --project GridironConsole"
echo "  3. Verify teams and players in database using SQL Server extension"
echo ""

# Return to original directory
cd "$SCRIPT_DIR"
