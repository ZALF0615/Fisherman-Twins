# Fisherman-Twins

Unity ver: 2021.3.2f1

----------------------------------------------------------------------------

Script Directory

├── MainGameScene  
│   ├── GameController.cs (메인 게임 로직 제어)  
│   ├── PlayerController.cs (플레이어 동작 제어)  
│   ├── RiverGenerator.cs (강 및 물고기 생성)  
│   ├── FishScript.cs (물고기 동작 제어)  
│   ├── ObstacleScript.cs (장애물 특성 정의)  
│   ├── Bullet.cs (총알 동작 제어)  
│   ├── UIScript.cs (게임 UI 관리 및 업데이트)  
│   └── SimpleFollow.cs (특정 대상을 따라가는 카메라 동작 제어)  
│  
├── AdventureMode  
│   ├── MessageWindow.cs (메시지 창 동작 제어)  
│   └── AdventureModeManager.cs (어드벤처 모드 스테이지 관리)  
│  
├── SceneManagement  
│   ├── GameManager.cs (현재 씬 및 스테이지 인덱스 관리 등 게임 씬 관리)  
│   ├── TitleScript.cs (타이틀 씬 동작 제어)  
│   ├── MainLobbyScript.cs (메인 로비 씬 동작 제어)  
│   └── AdventureLobbyScript.cs (어드벤처 모드 로비 씬 동작 제어)  
│  
└── Utils  
    └── Constants.cs (게임 상수 정의 및 스프레드시트 데이터 로드)