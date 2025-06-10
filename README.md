# Avalonia_redis

這是一個使用 Avalonia UI 框架開發的桌面應用程式，主要功能是從本機攝影機擷取即時影像，透過 Redis 進行發布。

## ✨ 功能

*   **即時影像擷取**: 使用 Emgu.CV 和 DirectShowLib 從攝影機讀取影像畫面。
*   **訊息發布/訂閱**: 透過 Redis 的 Pub/Sub 機制，將影像資料即時發布到指定的頻道。
*   **高效能序列化**: 使用 Google Protocol Buffers (Protobuf) 對影像訊息進行序列化與反序列化，確保高效的資料傳輸。
*   **響應式 UI**: 採用 Avalonia  建構跨平台的響應式使用者介面。

## 🛠️ 使用技術

*   **UI 框架**: Avalonia
*   **影像處理**: Emgu.CV, DirectShowLib
*   **通訊**: Redis
*   **資料序列化**: Google.Protobuf
*   **日誌**: Serilog

## 📂 專案結構

```
Avalonia_redis/
├── Assets/              # 存放應用程式的靜態資源 (如圖片、字型)
├── service/
│   ├── CameraManager.cs      # 管理攝影機擷取與影像處理
│   ├── redismanager.cs       # 負責連接 Redis 並發布訊息
│   └── uimanager.cs          # UI 相關邏輯管理
├── util/                     # 共用工具函式
│   ├── FrameToByte.cs        # 影像幀轉換工具
│   └── VideoCaptureApiHelper.cs # 攝影機 API 
├── ViewModels/               # MVVM 模式的 ViewModel
├── Views/                    # MVVM 模式的 View
├── config/                   # 應用程式設定
├── message.proto             # Protobuf 的訊息定義檔
├── App.axaml              # 應用程式的進入點 XAML
├── App.axaml.cs           # 應用程式的 C# 邏輯
└── Avalonia_redis.csproj     # 專案檔與相依性設定
```

## 🚀 運作流程

1.  **啟動攝影機**: `CameraManager` 初始化並開啟指定的攝影機。
2.  **擷取畫面**: `CameraManager` 持續從攝影機擷取影像畫面。
3.  **序列化**: 每個影像畫面被封裝成 Protobuf 定義的 `VideoMsg` 格式。
4.  **發布訊息**: `redismanager` 將序列化後的 `VideoMsg` 位元組發布到設定好的 Redis 頻道。
5.  **日誌紀錄**: 使用 Serilog 記錄應用程式運行狀態與錯誤訊息。

## ⚙️ 安裝與設定

1.  **config**
    設定檔案位置位於`C:\deploy_item\config.json`

2.  **還原相依套件**
    在專案根目錄執行 `dotnet restore`。

3.  **設定 Redis 連線**
    應用程式的 Redis 連線資訊可能儲存在 `config` 目錄下的設定檔中。請根據您的環境修改 Redis 伺服器位址、連接埠和密碼。
    *(注意: 程式碼中 `redismanager.cs` 顯示它從 `AppConfig` 讀取設定)*
4. **Msmf模式下啟動緩慢問題**
    在環境變數中新增`OPENCV_VIDEOIO_MSMF_ENABLE_HW_TRANSFORMS`並設值為`0`,停用在 MediaFoundation 處理圖片中使用硬體加速轉換 （DXVA）

## ▶️ 如何執行

1.  完成上述安裝與設定。
2.  建置專案後即可啟動
