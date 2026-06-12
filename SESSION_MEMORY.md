# Session Memory
_Last updated: 2026-06-05T00:00:00Z_
_Project: fk-news-detector (FakeGuard)_

---

## 🎯 Current Objective
Build a full end-to-end ASP.NET Core MVC 8 app that receives news articles (URL or text), stores them in Cassandra, runs ML detection, and shows a verdict page.

---

## ✅ What Worked
- Cassandra session factory (`CassandraSessionFactory.cs`) connects via `IConfiguration` keys
- Repository pattern implemented: `ArticleRepository` and `ArticleLookupRepository` using `IMapper` from CassandraCSharpDriver
- `UnitOfWork` wraps both repositories, registered as scoped in DI
- `ArticleService.GetOrAnalyzeAsync` — full dedup + save flow:
  - URL submissions: fingerprint = lowercased URL
  - Manual text: fingerprint = SHA-256 of content
  - Checks `article_lookup` table first; returns cached result if found
  - If new: calls ML, writes `articles` + `article_lookup` to Cassandra
- `DetectionService` calls Flask ML endpoint, applies 0.75 confidence threshold → FAKE / REAL / UNCERTAIN
- `ArticleController.Submit` now calls `ArticleService.GetOrAnalyzeAsync` then redirects to `Result/Details/{id}`
- `ResultController.Details` loads article from Cassandra by `Guid id`, maps to `ResultDisplayVM`
- `ResultDisplayVM` computes `BadgeClass`, `VerdictIcon`, `ConfidencePct` as computed properties
- `Details.cshtml` fully bound to `ResultDisplayVM` — no hardcoded values
- Build compiles clean (0 errors, 0 warnings)

---

## ❌ What Failed
_None._

---

## 🐛 Bugs & Errors Log

| # | Error / Bug | Status | Fix applied |
|---|-------------|--------|-------------|
| 1 | `CS8601` nullable warning on `SourceUrl` assignment in `ArticleController` | Fixed | Added `?? string.Empty` coalesce |

---

## 🔑 Key Points to Remember
- **Stack**: ASP.NET Core 8 (not .NET Framework 4.8 as CLAUDE.md says — project was migrated), Cassandra via `CassandraCSharpDriver`, Unity DI replaced by built-in `Microsoft.Extensions.DI`
- **No Entity Framework** — all DB access via `IMapper` from the Cassandra driver
- **`ISession` is singleton** — registered in `Program.cs` via `AddSingleton`
- **`IUnitOfWork` is scoped** — one per HTTP request
- **Cassandra tables already exist**: `articles` (partition key: `article_id` UUID) and `article_lookup` (partition key: `fingerprint` text) — created manually in the `fake_news_ks` keyspace
- **ML endpoint**: `http://localhost:8000/predict` — expects `{ site_web, title, article }`, returns `{ prediction, label, is_fake, confidence }`
- **Confidence threshold**: 0.75 — below this → UNCERTAIN regardless of label
- **`ArticleController` does NOT inject `IDetectionService` directly** — it only uses `IArticleService` and `INewsExtractionService`
- **Fingerprint dedup** prevents re-running ML on the same article twice
- `ExtractedArticle.SourceUrl` is non-nullable `string` (not `string?`)

---

## 📁 Important Files & Paths
- `Infrastructure/CassandraSessionFactory.cs` — creates and returns `ISession`
- `Models/Article.cs` — Cassandra `articles` table mapping
- `Models/ArticleLookup.cs` — Cassandra `article_lookup` table mapping
- `Models/ViewModels/ResultDisplayVM.cs` — **new** — result page VM with computed badge/icon properties
- `Repositories/ArticleRepository.cs` — `SaveAsync`, `GetByIdAsync`
- `Repositories/ArticleLookupRepository.cs` — `SaveAsync`, `GetByFingerprintAsync`
- `UnitOfWork/UnitOfWork.cs` — exposes `Articles` and `ArticleLookups` repos
- `Services/ArticleService.cs` — dedup + ML + Cassandra write logic
- `Services/DetectionService.cs` — HTTP call to Flask, verdict threshold logic
- `Controllers/ArticleController.cs` — Submit (URL/text), FetchFromUrl (AJAX)
- `Controllers/ResultController.cs` — `Details(Guid id)` loads from Cassandra
- `Views/Result/Details.cshtml` — verdict page, fully bound to `ResultDisplayVM`
- `appsettings.json` — `Cassandra:ContactPoint`, `Cassandra:Port`, `Cassandra:Keyspace`, `ML:Endpoint`
- `Program.cs` — DI registrations

---

## 🔧 Environment & Config
- .NET 8, `dotnet build` passes clean
- Cassandra expected at `127.0.0.1:9042`, keyspace `fake_news_ks`
- ML Flask service expected at `http://localhost:8000/predict`
- Cassandra tables already created in `fake_news_ks` keyspace — no schema work needed

---

## ⏭️ Next Step
> Run the app (`dotnet run`) with Cassandra and the Flask ML service both up, then do a full end-to-end test: submit a URL → verify dedup on second submit → check the result page renders the real verdict.

### Backlog
- [ ] Add loading spinner / "Analysing…" state to the Submit button (UX)
- [ ] `ResultController` returns `NotFound()` — add a proper 404 view
- [ ] `Article/Index` (history page) — list past analyses from Cassandra
- [ ] Handle ML service being unreachable gracefully (show error on Submit page, don't crash)
- [ ] Add `IDetectionResultRepository` for audit trail (separate `detection_results` table)
