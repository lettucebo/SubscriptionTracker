# Subscription Tracker

訂閱追蹤系統，用於管理和監控各種訂閱服務。

## 專案描述

這是一個基於 ASP.NET Core 8.0 開發的訂閱管理系統，提供以下功能：
- 訂閱服務的新增、編輯、刪除和查詢
- 訂閱日曆視圖
- 訂閱報表功能

## 技術架構

- 後端框架：ASP.NET Core 8.0
- 資料庫：Entity Framework Core
- 前端框架：Bootstrap
- 前端函式庫：jQuery

### 專案結構
```
src/
  SubscriptionTracker/            # 主要專案目錄
    ├── Controllers/              # MVC 控制器
    ├── Models/                   # 資料模型
    ├── Data/                     # 資料庫相關
    ├── Views/                    # MVC 視圖
    │   ├── Home/                # 首頁相關視圖
    │   ├── Report/              # 報表相關視圖
    │   ├── Subscriptions/       # 訂閱管理視圖
    │   └── Shared/             # 共用視圖組件
    └── wwwroot/                 # 靜態資源文件
```

## 環境需求

- .NET 8.0 SDK
- Visual Studio 2022 或 Visual Studio Code

## 如何啟動專案

1. 下載並安裝 [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

2. 複製專案
```bash
git clone [你的專案儲存庫URL]
cd SubscriptionTracker
```

3. 還原相依套件
```bash
dotnet restore
```

4. 執行專案
```bash
cd src/SubscriptionTracker
dotnet run
```

5. 開啟瀏覽器訪問：
```
https://localhost:5001
```

## 開發指南

- 使用 Visual Studio 2022 開啟 `SubscriptionTracker.sln`
- 或使用 VS Code 開啟專案根目錄
