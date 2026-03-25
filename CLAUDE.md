# Claude Code Rehberi

Bu dosya Claude Code tarafından her oturumda otomatik okunur.
Projeye dair tüm kararlar, kurallar ve mimari burada tanımlanmıştır.
Çelişen bir talimat gelse dahi bu dosyadaki kurallardan sapılmaz.

---

## Proje Yapısı

```
AcilEvrak/
├── CLAUDE.md
├── docker-compose.yml
├── .env.development
├── .env.staging
├── .env.production
├── .dockerignore
├── AcilEvrak.slnx
├── Dockerfile
├── .gitignore
├── src/
│   ├── AcilEvrak.Domain/
│   ├── AcilEvrak.Application/
│   ├── AcilEvrak.Infrastructure/
│   └── AcilEvrak.WebAPI/
├── tests/
│   └── IntegrationTests/
└── infra/
    ├── postgres/init/01_init.sql
    ├── logstash/pipeline/logstash.conf
    ├── logstash/config/logstash.yml
    ├── otel/otel-collector-config.yml
    └── grafana/provisioning/datasources/datasources.yml
```

---

## Proje Özeti

**.NET 10** — Clean Architecture + DDD + CQRS.
Multi-tenant yapı zorunludur, her sorguda `tenant_id` koşulu bulunur.
Ortamlar: Development, Staging, Production.

---

## Mimari

### Katman Bağımlılık Kuralı (içten dışa):
```
Domain → (sıfır bağımlılık, hiçbir projeye referans vermez)
Application → Domain
Infrastructure → Domain, Application
WebAPI → Domain, Application, Infrastructure
```

**Bağımlılık yönü asla tersine çevrilmez.** Domain hiçbir şeye bağımlı değildir.

### Proje Detayları:

```
src/
├── AcilEvrak.Domain/              ← Class Library (sıfır bağımlılık)
│   ├── Common/
│   │   ├── BaseEntity.cs
│   │   ├── AuditableEntity.cs
│   │   ├── TenantEntity.cs
│   │   └── IAggregateRoot.cs
│   ├── ValueObjects/
│   │   ├── Email.cs
│   │   └── PasswordHash.cs
│   ├── Events/
│   │   ├── IDomainEvent.cs
│   │   └── {Feature}Events/
│   ├── Exceptions/
│   │   ├── DomainException.cs
│   │   ├── NotFoundException.cs
│   │   ├── ConflictException.cs
│   │   ├── ValidationException.cs
│   │   └── UnauthorizedException.cs
│   ├── Interfaces/
│   │   ├── IUnitOfWork.cs
│   │   ├── IDbConnectionFactory.cs
│   │   ├── ITenantContext.cs
│   │   ├── IPasswordHasher.cs
│   │   └── IJwtTokenService.cs
│   └── Entities/
│       ├── Tenant.cs
│       ├── User.cs
│       └── Session.cs
│
├── AcilEvrak.Application/        ← Class Library (sadece Domain'e referans)
│   ├── Features/
│   │   ├── Auth/
│   │   │   ├── Commands/
│   │   │   └── Queries/
│   │   └── Users/
│   │       ├── Commands/
│   │       └── Queries/
│   ├── Models/
│   │   ├── Result.cs
│   │   └── PagedResponse.cs
│   └── Interfaces/
│       ├── IUserRepository.cs
│       ├── ISessionRepository.cs
│       └── IOutboxRepository.cs
│
├── AcilEvrak.Infrastructure/     ← Class Library (Domain + Application'a referans)
│   ├── Database/
│   │   ├── NpgsqlConnectionFactory.cs
│   │   ├── UnitOfWork.cs
│   │   └── TenantContext.cs
│   ├── Repositories/
│   │   ├── UserRepository.cs
│   │   └── SessionRepository.cs
│   ├── Migrations/
│   ├── Outbox/
│   │   ├── OutboxMessage.cs
│   │   ├── OutboxRepository.cs
│   │   └── OutboxBackgroundService.cs
│   ├── Messaging/
│   │   ├── RabbitMqPublisher.cs
│   │   └── RabbitMqSetup.cs
│   ├── Auth/
│   │   ├── JwtTokenService.cs
│   │   └── PasswordHasher.cs
│   └── Cache/
│       ├── ICacheService.cs
│       └── InMemoryCacheService.cs
│
└── AcilEvrak.WebAPI/             ← Web API (hepsine referans)
    ├── Controllers/
    │   ├── AuthController.cs
    │   └── UsersController.cs
    ├── Middleware/
    │   ├── CorrelationIdMiddleware.cs
    │   ├── TenantMiddleware.cs
    │   ├── RequiresTenantAttribute.cs
    │   ├── IdempotencyMiddleware.cs
    │   └── ExceptionHandlingMiddleware.cs
    ├── Models/
    │   └── ApiResponse.cs
    ├── Extensions/
    │   └── ServiceCollectionExtensions.cs
    └── Program.cs

tests/
└── IntegrationTests/
    ├── Features/
    │   ├── Auth/
    │   └── Users/
    ├── Fixtures/            → PostgreSqlFixture, WebAppFactory
    └── Builders/            → test veri üreticileri
```

### Mimari kurallar:
- Feature klasörleri birbirinin koduna dokunamaz
- Feature'lar arası iletişim → RabbitMQ (async) veya HTTP (sadece third-party)
- Domain katmanı hiçbir harici pakete bağımlı değildir (saf C#)
- Application katmanı yalnızca MediatR'a bağımlıdır
- Infrastructure katmanı Dapper, Npgsql, FluentMigrator vb. paketleri içerir
- WebAPI katmanı Controller, Middleware ve DI composition root'unu barındırır

---

## Build & Test Komutları

```bash
# Build (.NET 10 .slnx formatını otomatik algılar)
dotnet build AcilEvrak.slnx

# Testler (Docker gerektirir — Testcontainers gerçek PostgreSQL başlatır)
dotnet test AcilEvrak.slnx

# Tek test
dotnet test tests/IntegrationTests/ --filter "FullyQualifiedName~MethodName_StateUnderTest_ExpectedBehavior"

# Local çalıştır
dotnet run --project src/AcilEvrak.WebAPI/

# Tüm servisleri başlat (development)
docker compose --env-file .env.development up -d

# Tüm servisleri başlat (staging)
docker compose --env-file .env.staging up -d

# Tüm servisleri başlat (production)
docker compose --env-file .env.production up -d
```

---

## Teknik Zorunluluklar

- .NET 10 LTS kullanılır
- `<Nullable>enable</Nullable>` aktif olur
- Tüm `DateTime` → `DateTime.UtcNow`, asla `DateTime.Now` kullanılmaz
- Tüm `Id` → `long`, DB tarafından `BIGSERIAL` üretilir
- Tüm `Uuid` → `Guid`, .NET tarafında `Guid.CreateVersion7()` (UUID v7) ile üretilir, DB'ye INSERT sırasında gönderilir. `Guid.NewGuid()` (v4) kullanılmaz
- Dışarıya (API response, URL) her zaman `Uuid` açılır, `Id` asla expose edilmez
- Insert sonrası `RETURNING id, uuid` ile aynı transaction içinde alınır
- Tüm `async` metodlar `CancellationToken` parametresi alır
- **[DEĞİŞTİRİLEMEZ KURAL]** Her dosyada yalnızca bir tür (class, record, interface, enum, struct) bulunur. Aynı dosyaya ikinci bir tür eklenmez, istisnası yoktur
- Namespace'ler klasör yapısını birebir yansıtır
- `public` constructor yerine `static factory metod` tercih edilir
- Mümkün olan her yerde `sealed` kullanılır
- XML dokümantasyon yazılmaz
- AutoMapper kullanılmaz → manuel `ToDto()`, `ToEntity()` extension metodları yazılır

---

## DDD Kuralları

### Value Object:
- Eşitlik identity'ye değil değere göre belirlenir
- Immutable olmalıdır (tüm property'ler `init` veya ctor'da set)
- Kendi validasyonunu taşır → geçersiz değerle oluşturulamaz
- `static factory metod` ile oluşturulur

### Entity:
- Her Entity private setter kullanır
- Durumu yalnızca kendi metodlarıyla değiştirir
- İş kuralları (invariant) Entity içinde korunur

### Aggregate Root:
- `IAggregateRoot` marker interface'ini uygular
- Domain Event'leri yalnızca Aggregate Root raise eder
- Dışarıdan erişim yalnızca Aggregate Root üzerinden olur
- Her Aggregate Root için bir Repository vardır

### Domain Event:
- Entity state değiştiğinde `RaiseDomainEvent()` ile raise edilir
- UoW commit sırasında Outbox'a yazılır (aynı transaction)
- Handler'lar Domain Event'i manuel oluşturmaz, Entity raise eder

### Domain Service:
- Birden fazla Aggregate'i ilgilendiren iş kuralları Domain Service'te yaşar
- `IPasswordHasher`, `IJwtTokenService` gibi interface'ler Domain'de tanımlanır
- Implementasyonları Infrastructure'da yaşar

---

## Kullanılabilecek Paketler

### AcilEvrak.Domain:
- Hiçbir harici paket kullanılmaz (saf C#)

### AcilEvrak.Application:
- `MediatR` v12.4.1

### AcilEvrak.Infrastructure:
- `Dapper` v2.1.35
- `Npgsql` v9.0.3
- `FluentMigrator` v5.2.0
- `FluentMigrator.Runner` v5.2.0
- `FluentMigrator.Runner.Postgres` v5.2.0
- `RabbitMQ.Client` v6.8.1
- `Microsoft.Extensions.Configuration.Abstractions` v10.0.0
- `Microsoft.Extensions.Hosting.Abstractions` v10.0.0
- `Microsoft.IdentityModel.Tokens` v8.3.0
- `System.IdentityModel.Tokens.Jwt` v8.3.0

### AcilEvrak.WebAPI:
- `Microsoft.AspNetCore.Authentication.JwtBearer` v10.0.0
- `OpenTelemetry.Extensions.Hosting` v1.9.0
- `OpenTelemetry.Instrumentation.AspNetCore` v1.9.0
- `OpenTelemetry.Exporter.OpenTelemetryProtocol` v1.9.0

### Sadece test projelerinde:
- `FluentAssertions` v6.12.2
- `Testcontainers.PostgreSql` v3.10.0
- `Microsoft.AspNetCore.Mvc.Testing` v10.0.0

### Kesinlikle yasak:
- `Entity Framework Core` (hiçbir amaçla)
- `AutoMapper`
- `FluentValidation`
- `Serilog`
- `Polly`
- `MassTransit`
- `NSubstitute`
- Yukarıda listelenmeyen diğer tüm üçüncü parti paketler

---

## Entity Kuralları

```csharp
// Tüm entity'lerin tabanı — Domain Event desteği ile
public abstract class BaseEntity
{
    public long Id { get; private set; }
    public Guid Uuid { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public long Version { get; private set; }

    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}

// Audit bilgisi gereken entity'ler
public abstract class AuditableEntity : BaseEntity
{
    public long CreatedBy { get; private set; }
    public long? UpdatedBy { get; private set; }
    public long? DeletedBy { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    // IsDeleted boolean YAZILMAZ → WHERE deleted_at IS NULL kullanılır
}

// Tenant izolasyonu gereken entity'ler
public abstract class TenantEntity : AuditableEntity
{
    public long TenantId { get; private set; }
}

// Aggregate Root marker
public interface IAggregateRoot { }
```

---

## CQRS Kuralları

- Command → veri değiştirir, `Result` döner
- Query → sadece okur, `Result<T>` döner, veri değiştirmez
- Her Handler tek bir iş yapar
- Handler'lar UoW üzerinden repository'e erişir, direkt DB erişimi yasak
- Command Handler'lar Domain Entity metodlarını çağırır, iş mantığını kendileri yazmaz

```
Application/Features/Users/
├── Commands/CreateUser/
│   ├── CreateUserCommand.cs
│   ├── CreateUserCommandHandler.cs
│   └── CreateUserCommandValidator.cs
└── Queries/GetUsers/
    ├── GetUsersQuery.cs
    ├── GetUsersQueryHandler.cs
    └── GetUsersResponse.cs
```

---

## Veritabanı Kuralları

- Tüm DB işlemleri UoW içinde transaction ile çalışır
- Tablo adları `snake_case` ve çoğul: `users`, `refresh_tokens`
- Kolon adları `snake_case`: `created_at`, `tenant_id`
- Soft delete: `WHERE deleted_at IS NULL`, boolean kullanılmaz
- Silinmiş kayıtlar için açıkça `includeDeleted: true` geçilir

### Her tablo şu kolonları içerir:
```sql
id          BIGSERIAL PRIMARY KEY,
uuid        UUID NOT NULL,
created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
updated_at  TIMESTAMPTZ,
version     BIGINT NOT NULL DEFAULT 0
```

### Auditable tablolar ekler:
```sql
created_by  BIGINT NOT NULL,
updated_by  BIGINT,
deleted_by  BIGINT,
deleted_at  TIMESTAMPTZ
```

### Tenant tabloları ekler:
```sql
tenant_id   BIGINT NOT NULL
```

### Her uuid için unique index:
```sql
CREATE UNIQUE INDEX idx_{tablo}_uuid ON {tablo}(uuid);
```

### Migration isimlendirme:
```
M20240101000001_CreateTenantsTable
```
Migration'lar `AcilEvrak.Infrastructure/Migrations/` klasöründe yaşar.

---

## Tenant Kuralları

- Tenant zorunluluğu endpoint bazlı opt-in şeklinde çalışır (Laravel route middleware mantığı)
- `[RequiresTenant]` attribute'ü ile işaretlenen controller/action'larda `X-Tenant-Id` header'ı zorunludur
- `[RequiresTenant]` olmayan endpoint'lerde header opsiyoneldir; gönderilirse set edilir, gönderilmezse istek reddedilmez
- `TenantMiddleware` header'ı okur ve `ITenantContext`'e yazar; zorunluluk `[RequiresTenant]` metadata'sına bağlıdır
- Tenant-scoped entity'ler `TenantEntity`'den türer ve `tenant_id` kolonunu içerir
- Tenant-scoped sorgular `WHERE tenant_id = @TenantId` koşulunu içerir
- **Users ve Sessions tabloları tenant-scoped DEĞİLDİR** (global entity)
- Tenant izolasyonu gereken tablolarda ihlal kabul edilmez

---

## API Kuralları

- Tüm endpoint'ler versiyonlanır: `/api/v1/...`
- URL'lerde her zaman `uuid`: `/api/v1/users/{uuid}`
- POST/PUT/PATCH → `X-Idempotency-Key` header zorunlu
- `X-Correlation-Id` → yoksa middleware üretir
- `X-Tenant-Id` → yalnızca `[RequiresTenant]` ile işaretlenen endpoint'lerde zorunludur
- JWT → `Authorization: Bearer {token}`, max 15 dakika
- Refresh Token → max 7 gün, kullanılınca rotate edilir

### Response formatı:
```json
// Başarılı
{ "success": true, "data": {}, "error": null, "correlationId": "uuid", "timestamp": "utc" }

// Hatalı
{ "success": false, "data": null, "error": { "code": "USER_NOT_FOUND", "message": "...", "type": "NotFound", "details": {} }, "correlationId": "uuid", "timestamp": "utc" }
```

---

## Session Kuralları

```sql
CREATE TABLE sessions (
    id                  BIGSERIAL PRIMARY KEY,
    uuid                UUID NOT NULL,
    user_id             BIGINT NOT NULL REFERENCES users(id),
    device_name         VARCHAR(255),
    ip_address          VARCHAR(45),
    user_agent          TEXT,
    refresh_token_hash  VARCHAR(512) NOT NULL,  -- SHA256, ham token asla saklanmaz
    last_used_at        TIMESTAMPTZ,
    expires_at          TIMESTAMPTZ NOT NULL,
    revoked_at          TIMESTAMPTZ,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMPTZ,
    version             BIGINT NOT NULL DEFAULT 0
);
```

- Login → session oluştur, JWT payload'ına `session_uuid` ekle
- Her istekte `session_uuid` DB'de kontrol edilir (`revoked_at IS NULL AND expires_at > NOW()`)
- Refresh → eski session revoke, yeni session aç (aynı transaction)
- Kullanıcı oturum listesini görür ve istediği oturumu kapatabilir

---

## Outbox Pattern Kuralları

- Domain event → Outbox tablosuna ana işlemle aynı transaction'da yazılır
- RabbitMQ'ya direkt publish kesinlikle yasak
- `BackgroundService` Outbox'ı okur, RabbitMQ'ya publish eder
- Başarısız publish'lerde `retry_count` artar, max 3'te `error` kolonu doldurulur
- Entity'nin `DomainEvents` listesi UoW commit sırasında Outbox'a aktarılır

---

## RabbitMQ Kuralları

- Exchange tipi: `Topic`
- Her event için Dead Letter Queue (DLQ) tanımlanır
- `IConnection` singleton, `IChannel` scoped

### İsimlendirme:
```
Exchange : {proje}.exchange              → acilEvrak.exchange
Queue    : {proje}.{event}.queue         → acilEvrak.user-created.queue
DLQ      : {proje}.{event}.dlq
Routing  : {feature}.{entity}.{eylem}   → users.user.created
```

### Event yapısı:
```json
{
  "eventId": "uuid",
  "eventType": "users.user.created",
  "occurredAt": "utc-datetime",
  "tenantId": "uuid",
  "correlationId": "uuid",
  "payload": {}
}
```

---

## Test Kuralları

- Yalnızca integration test yazılır, unit test yazılmaz
- Testcontainers ile gerçek PostgreSQL kullanılır, in-memory DB yasak
- Her endpoint için en az bir integration test yazılır
- Test adı formatı: `MethodName_StateUnderTest_ExpectedBehavior`
- Her test bağımsızdır, test sonrası DB temizlenir

---

## Endpoint Dokümantasyonu

Her feature kendi `ENDPOINTS.md` dosyasını WebAPI/Controllers yanında içerir:

```markdown
## [METHOD] /api/v1/[path]
**Açıklama:** Ne iş yapar
**Yetki:** Public / JWT / Admin
**Idempotency Key:** Zorunlu / Opsiyonel / Yok
**Headers:** X-Correlation-Id (opsiyonel), X-Tenant-Id (yalnızca [RequiresTenant] endpoint'lerinde zorunlu)
**Request:** Alan açıklamaları
**Response:** Başarılı dönüş
**Hatalar:** HTTP kodu ve açıklaması
```

---

## Gözlemlenebilirlik

```
Uygulama → OpenTelemetry Collector → Elasticsearch (log) + Grafana (metrik)
```

- Trace'ler `X-Correlation-Id` ile ilişkilendirilir
- Her log `tenantId`, `correlationId`, `userId` içerir (varsa)
- Log seviyeleri: `Debug`, `Information`, `Warning`, `Error`

---

## Önemli Kararlar

| Karar | Gerekçe |
|---|---|
| Clean Architecture | Katmanlar arası net bağımlılık yönü, test edilebilirlik |
| DDD taktik kalıpları | İş kuralları Domain'de korunur, anemic model önlenir |
| Dapper (EF Core yok) | Tam SQL kontrolü, şeffaf sorgular |
| FluentMigrator | Up/Down desteği, EF Core gerektirmez |
| RabbitMQ Topic Exchange | Esnek routing, DLQ, ileride yeni consumer kolaylığı |
| BIGSERIAL + UUID | Id internal join için, UUID dışarıya güvenli identifier |
| Outbox Pattern | Event kayıp riskini sıfırlar, Entity'den raise edilir |
| Session tablosu | JWT stateless ama oturum iptali DB ile sağlanır |
| DeletedAt (bool değil) | Silinme tarihi + soft delete tek alanda |
| Sadece integration test | Handler'lar gerçek DB ile zaten test edilir |
