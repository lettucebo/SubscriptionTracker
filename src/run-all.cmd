@echo off
echo Starting ASP.NET Core Back-end...
start dotnet watch --project SubscriptionTracker.Api/SubscriptionTracker.Api.csproj
timeout /t 5 /nobreak >nul
echo Starting Vue.js Front-end...
cd subscription-tracker-client
start npm run serve
