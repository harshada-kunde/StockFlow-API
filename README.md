# 📦 What is StockFlow?

StockFlow is a Inventory Management REST API built with ASP.NET Core 8. It handles the full lifecycle of inventory operations — managing categories, products, and orders — with secure role-based access for Admin and Employee users.

# Key capabilities:

📂 Categories — organise your product catalogue   

🛍️ Products — manage stock with pagination, search, and price filtering

🛒 Orders — place orders, track status, auto-deduct stock, restore on cancellation

🔐 Authentication — register, login, and access protected endpoints with JWT tokens


# 🚀 How to Use

**Option 1 — Use Docker**

git clone https://github.com/harshada-kunde/StockFlow-API.git

cp .env.example .env - Open .env and set SA_PASSWORD and JWT_SECRET

Run - docker-compose up

Open http://localhost:8080/swagger

Docker automatically starts SQL Server, runs migrations, and launches the API. No manual database setup needed.



**Option 2 — Run Locally**

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your-connection-string"

dotnet user-secrets set "JwtSettings:SecretKey" "your-secret-key-min-32-chars"

Create database - dotnet ef database update

Start - dotnet run

Open https://localhost:7051/swagger


Getting Started in Swagger

1. POST /api/auth/register  →  create your account (role: Admin or Employee)
2. POST /api/auth/login     →  get your JWT token
3. Click 🔒 Authorize       →  paste token
4. Start calling endpoints


# 🧱 Technologies          

Language - C#12

Framework - ASP.NET Core 8 Web API

Database - Microsoft SQL Server 2022

ORM - Entity Framework Core 8

Authentication - JWT Bearer Tokens + BCrypt

Containerisation - Docker + Docker Compose

API Docs - Swagger / OpenAPI


# Project structure:

Controllers/          ← HTTP layer

Services/             ← business orchestration

ValidationService/    ← all validation (single source of truth)

Repositories/         ← data access

Entities/             ← database models

DTOs/                 ← request shapes (input only)

Models/               ← ApiResponse<T>, PagedResponse<T>

Middleware/           ← global exception handling

Extensions/           ← service registrations (keeps Program.cs clean)

Enums/                ← UserRole

# A few deliberate design choices:

All validation lives in ValidationService — not in DTOs or controllers — so every response always uses the same ApiResponse<T> format

Product name is immutable after creation — it is the natural unique key

Order items store the price at time of purchase — historical orders are never affected by price changes

Bulk product creation validates all records before saving any — partial saves don't happen
