# TuranLogix API

Бэкенд для транспортно-экспедиторской компании TuranLogix (turanlogix.kz).

## Стек
- ASP.NET Core 8 Web API
- PostgreSQL 16 + EF Core 8
- Clean Architecture: Domain / Application / Infrastructure / Api
- MediatR + FluentValidation + AutoMapper
- JWT Bearer Auth, BCrypt
- Claude API (AI чат-бот)
- Telegram Bot, Email (MailKit)
- NCALayer (ЭЦП РК) — stub, TODO prod
- Mapbox (геокодинг, опционально)
- Azure Blob Storage (документы)
- Serilog → Seq

## Быстрый старт

### Docker Compose
```bash
cp .env.example .env
# Заполнить .env реальными ключами
docker compose up -d
# API: http://localhost:8080
# Swagger: http://localhost:8080/swagger
# Seq: http://localhost:5341
```

### Локально
```bash
dotnet restore
dotnet build
cd src/TuranLogix.Api
dotnet run
```

### Миграции
```bash
cd src/TuranLogix.Api
dotnet ef migrations add InitialCreate --project ../TuranLogix.Infrastructure
dotnet ef database update
```

## API Endpoints

| Метод | URL | Описание | Auth |
|-------|-----|----------|------|
| POST | /api/auth/register | Регистрация | — |
| POST | /api/auth/login | Вход | — |
| GET | /api/profile | Мой профиль | Client+ |
| PUT | /api/profile | Обновить профиль | Client+ |
| GET | /api/orders/my | Мои заявки | Client |
| GET | /api/orders/{id} | Детали заявки | Client+ |
| POST | /api/orders | Создать заявку | Client |
| PATCH | /api/orders/{id}/status | Изменить статус | Manager+ |
| GET | /api/orders/{id}/documents | Документы заявки | Client+ |
| POST | /api/orders/{id}/documents/upload | Загрузить документ | Client+ |
| POST | /api/orders/{id}/documents/{docId}/sign | Подписать ЭЦП | Client+ |
| POST | /api/chat | AI чат-бот | — |
| POST | /api/telegram/webhook | Telegram Bot | — |
| GET | /api/admin/orders | Все заявки | Manager+ |

## Telegram Bot webhook
```bash
curl -X POST "https://api.telegram.org/bot{TOKEN}/setWebhook?url=https://api.turanlogix.kz/api/telegram/webhook"
```

## Роли
- **Client** — обычный клиент
- **Manager** — менеджер компании
- **Admin** — администратор

## GitHub Secrets для CI/CD
`DOCKER_USERNAME`, `DOCKER_PASSWORD`, `SERVER_HOST`, `SERVER_USER`, `SERVER_SSH_KEY`
