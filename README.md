![.NET](https://img.shields.io/badge/.NET-8-blue)
![React](https://img.shields.io/badge/React-19-blue)
![SSMS](https://img.shields.io/badge/Microsoft_SQL_Server-CC2927)


# Approval System

一套以企業內部簽核流程為核心的全端系統。

---

# DEMO 網址

https://approvalsystem.sytes.net/

# Local 測試
以下為本專案的本機啟動方式。

請先安裝以下工具：
- Docker
- Docker Compose
- Git

```bash
git clone https://github.com/dongwenj/ApprovalSystem.git
```

```bash
cd ApprovalSystem
```

於專案根目錄執行：
```bash
docker compose up --build
```

| Frontend | http://localhost |

---

# 專案功能

- 權限區分
- 申請單 CRUD 管理
- Email寄送 (Hangfire 背景任務)
- 即時 Log 監控（SignalR）
- CI/CD 自動部署 (Github Action)
<img width="817" height="962" alt="image" src="https://github.com/user-attachments/assets/6f9c3580-db7c-4013-8ba6-e2dca3208720" />

---
# 架構圖

<img width="516" height="707" alt="TEST drawio" src="https://github.com/user-attachments/assets/9aa41a78-4d4d-43ca-b7cf-4f37b481ca09" />

---
# 技術棧

## Backend

- ASP.NET Core
- Entity Framework Core
- SignalR
- Hangfire
- UnitTest

## Frontend

- React

## DevOps

- Docker
- Nginx
- AWS EC2
- AWS RDS
