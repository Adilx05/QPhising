# EF Core Migrations (Official)

`QPhising` için resmi migration yolu EF Core design-time tooling üzerindendir.

## Kurulum ve kullanım

```bash
dotnet add backend/Infrastructure/Infrastructure.csproj package Microsoft.EntityFrameworkCore.Design
```

```bash
export Database__ConnectionString="Host=localhost;Port=5432;Database=qphising;Username=postgres;Password=postgres"
dotnet ef database update \
  --project backend/Infrastructure/Infrastructure.csproj \
  --startup-project backend/API/API.csproj
```

## Notlar

- Design-time DbContext üretimi `QPhisingDesignTimeDbContextFactory` üzerinden yapılır.
- Baseline migration, önceki SQL baseline dosyasını embedded resource olarak uygular.
- Yeni migration'lar bu klasör altında EF Core ile üretilmelidir.
