@echo off
set /p migration="Enter migration name: "
dotnet ef migrations add --context SqlDbContext --project SqlStorageCoreTests --startup-project SqlStorageCoreTests --verbose %migration%
pause