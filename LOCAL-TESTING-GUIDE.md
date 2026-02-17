# Local Testing Guide - ReminderSaaS Phase 1

## Prerequisites

### 1. Install Required Tools
```bash
# Azure Functions Core Tools (already installed if you have Backend.Function)
brew install azure-functions-core-tools@4

# SQL Server (if not using Docker)
# Option A: Docker
docker pull mcr.microsoft.com/mssql/server:2022-latest

# Option B: Local SQL Server instance
# Download from: https://www.microsoft.com/en-us/sql-server/sql-server-downloads
```

### 2. Verify Installations
```bash
func --version
dotnet --version
```

## Step 1: Setup Database

### Option A: Docker SQL Server (Recommended)
```bash
# Start SQL Server in Docker
docker run -e "ACCEPT_EULA=Y" \
  -e "SA_PASSWORD=YourPassword123!" \
  -p 1433:1433 \
  -d \
  mcr.microsoft.com/mssql/server:2022-latest

# Verify connection
sqlcmd -S localhost,1433 -U sa -P "YourPassword123!" -Q "SELECT 1"
```

### Option B: Update Connection String
Edit `/Backend.Function/local.settings.json`:
```json
{
  "SqlConnection": "Server=localhost,1433;Database=ReminderDb;User Id=sa;Password=YourPassword123!;Encrypt=false;"
}
```

### Step 2: Create Database and Apply Migrations
```bash
cd /Users/Suni/Documents/src

# Install EF Core CLI (if not installed)
dotnet tool install --global dotnet-ef

# Navigate to Infrastructure project
cd Infrastructure

# Create database and apply migrations
dotnet ef database update --startup-project ../Backend.Function/Backend_Function.csproj

# Verify
dotnet ef migrations list
```

## Step 3: Run Azure Functions Locally

### Terminal 1 - Start Azure Functions
```bash
cd /Users/Suni/Documents/src/Backend.Function

# Start the Functions host
func start

# Expected output:
# Azure Functions Core Tools ... started
# Http Functions:
#   DeleteSchedule: [DELETE] http://localhost:7071/api/schedules/{id}
#   CreateSchedule: [POST] http://localhost:7071/api/schedules
#   ...
# Worker process started and initialized.
```

**Note the port (7071) - used later for MAUI app configuration**

### Test API Endpoints

#### Terminal 2 - Test create schedule
```bash
# Create a schedule
curl -X POST http://localhost:7071/api/schedules \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Doctor Appointment",
    "date": "2026-02-25",
    "category": "Health",
    "description": "Annual checkup",
    "time": "14:30:00",
    "location": "City Medical Center"
  }'

# Expected response:
# {"id":"<guid>"}
```

#### Test get schedules by month
```bash
curl http://localhost:7071/api/schedules?year=2026&month=2

# Expected response (JSON array of schedules)
```

#### Test get schedule by ID
```bash
# Replace <SCHEDULE_ID> with actual ID from create response
curl http://localhost:7071/api/schedules/<SCHEDULE_ID>

# Expected response:
# {"id":"...","title":"...","date":"...","category":"..."}
```

#### Test update schedule
```bash
curl -X PUT http://localhost:7071/api/schedules/<SCHEDULE_ID> \
  -H "Content-Type: application/json" \
  -d '{
    "id": "<SCHEDULE_ID>",
    "title": "Doctor Appointment - Updated",
    "date": "2026-02-26",
    "category": "Health",
    "time": "15:00:00"
  }'
```

#### Test delete schedule
```bash
curl -X DELETE http://localhost:7071/api/schedules/<SCHEDULE_ID>
```

## Step 4: Run MAUI App Locally

### Option A: Android Emulator
```bash
cd /Users/Suni/Documents/src

# Start Android emulator first (via Android Studio or command line)
emulator -avd Pixel_5_API_34 &

# Build and run
dotnet build ReminderSaaS.Maui/ReminderSaaS.Maui.csproj -f net10.0-android

# Run on emulator
dotnet run -f net10.0-android --project ReminderSaaS.Maui/ReminderSaaS.Maui.csproj

# Note: Android emulator uses 10.0.2.2 to access localhost:7071
# MauiProgram.cs already handles this
```

### Option B: iOS Simulator (macOS only)
```bash
cd /Users/Suni/Documents/src

# Build for iOS simulator
dotnet build ReminderSaaS.Maui/ReminderSaaS.Maui.csproj -f net10.0-ios

# Run on simulator
dotnet run -f net10.0-ios --project ReminderSaaS.Maui/ReminderSaaS.Maui.csproj

# Or directly launch simulator:
open -a Simulator
dotnet run -f net10.0-ios --project ReminderSaaS.Maui/ReminderSaaS.Maui.csproj
```

### Option C: macCatalyst (macOS native)
```bash
cd /Users/Suni/Documents/src

# Build for macOS
dotnet build ReminderSaaS.Maui/ReminderSaaS.Maui.csproj -f net10.0-maccatalyst

# Run
dotnet run -f net10.0-maccatalyst --project ReminderSaaS.Maui/ReminderSaaS.Maui.csproj
```

## Complete Local Testing Workflow

### Terminal 1: Start Database (if using Docker)
```bash
docker run -e "ACCEPT_EULA=Y" \
  -e "SA_PASSWORD=YourPassword123!" \
  -p 1433:1433 \
  --name mssql-dev \
  -d \
  mcr.microsoft.com/mssql/server:2022-latest

# Verify it's running
docker logs mssql-dev | grep "SQL Server is now ready"
```

### Terminal 2: Start Azure Functions
```bash
cd /Users/Suni/Documents/src/Backend.Function
func start
```

### Terminal 3: Start MAUI App
```bash
cd /Users/Suni/Documents/src
dotnet run -f net10.0-android --project ReminderSaaS.Maui/ReminderSaaS.Maui.csproj
```

### Test in App
1. Open Calendar tab
2. Create schedule: Tap "Add Schedule" button
3. Fill in details:
   - Title: "Test Event"
   - Date: Pick a date
   - Category: Health/Tax/School/Personal
   - Time: Optional
   - Location: Optional
4. Tap "Save"
5. Verify schedule appears on calendar
6. Tap to view details
7. Test Edit/Delete

## Troubleshooting

### "Connection refused" errors
```bash
# Check if Azure Functions is running
curl http://localhost:7071/

# Check if SQL Server is running
docker ps | grep mssql
sqlcmd -S localhost,1433 -U sa -P "YourPassword123!" -Q "SELECT 1"
```

### "Cannot access 10.0.2.2" (Android emulator)
```bash
# This is normal - MauiProgram.cs handles this automatically
# Verify in Android emulator:
# Settings → Wi-Fi → tap network → show advanced options
# Confirm you can ping 10.0.2.2 (gateway IP)
```

### Database migration errors
```bash
# Reset database
docker exec mssql-dev /opt/mssql-tools/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P "YourPassword123!" \
  -Q "DROP DATABASE ReminderDb;"

# Reapply migrations
dotnet ef database update --startup-project Backend.Function/Backend_Function.csproj
```

### MAUI app won't connect to functions
1. Verify base URL in `MauiProgram.cs` is correct
2. Check firewall allows localhost:7071
3. For Android: ensure emulator can reach 10.0.2.2:7071
4. Test with curl first (see Terminal 2 tests above)

## Debugging Tips

### Enable Logging
Edit `Backend.Function/Program.cs`:
```csharp
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);
```

### View Database
Use SQL Server Management Studio or VS Code extension:
```bash
# VS Code extension
# Install: ms-mssql.mssql

# Query in VS Code:
# SELECT * FROM Schedules WHERE [Date] = CAST('2026-02-25' AS DATE);
```

### Inspect MAUI Logs
```bash
# iOS/macOS
log stream --predicate 'process == "ReminderSaaS.Maui"'

# Android
adb logcat | grep ReminderSaaS.Maui
```

### Performance Testing
Use Azure Functions CLI built-in tools:
```bash
func azure functionapp publish <FunctionAppName> --build remote
```

## Next Steps for Phase 1

1. ✅ Compile and build locally
2. ✅ Test database migrations
3. ✅ Test Azure Functions locally
4. ✅ Test MAUI app on emulator/simulator
5. ✅ Verify create/read/update/delete operations
6. 📝 Add unit tests
7. 📝 Add integration tests
8. 📝 Deploy to Azure

## Quick Reference Commands

```bash
# Database
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourPassword123!" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest

# Functions
cd Backend.Function && func start

# MAUI Android
dotnet run -f net10.0-android --project ReminderSaaS.Maui/ReminderSaaS.Maui.csproj

# MAUI iOS
dotnet run -f net10.0-ios --project ReminderSaaS.Maui/ReminderSaaS.Maui.csproj

# Test API
curl -X POST http://localhost:7071/api/schedules \
  -H "Content-Type: application/json" \
  -d '{"title":"Test","date":"2026-02-25","category":"Personal"}'
```

---

**Status**: Local testing guide for Phase 1
**Date**: February 17, 2026
