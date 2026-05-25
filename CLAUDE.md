# CLAUDE.md — ASP.NET MVC Backend Architecture
## Fake News Detection Project

---

## What This Project Is

An ASP.NET MVC 5 web application (.NET Framework 4.8) that receives news articles,
sends them to a Python/Flask AI microservice for classification, stores everything
in Apache Cassandra, and returns a verdict (FAKE / REAL / UNCERTAIN) to the user.

---

## Stack

| Layer | Technology |
|---|---|
| Web framework | ASP.NET MVC 5 (.NET Framework 4.8) |
| Database | Apache Cassandra via CassandraCSharpDriver |
| ORM approach | Code First + Data Annotations only |
| Architecture | MVC + Repository Pattern + Unit of Work |
| AI backend | Python Flask microservice (external HTTP call) |
| Scraping | SmartReader + HtmlAgilityPack |
| DI container | Unity (Unity.Mvc5) |

---

## Absolute Rules

These rules are non-negotiable. Violating any of them breaks the architecture.

- **No Entity Framework, no DbContext** — the database driver is CassandraCSharpDriver, the session object is `ISession`
- **No Fluent API** — entity mapping uses Data Annotation attributes only
- **No Cassandra queries in Controllers** — all data access goes through Repositories via UnitOfWork
- **No business logic in Controllers** — Controllers receive input, delegate to Services, return a result
- **No synchronous database calls** — every Cassandra operation must be async/await
- **No JOINs** — Cassandra does not support joins; data must be denormalized at design time
- **One ISession singleton** — the Cassandra session is thread-safe and must be shared, not recreated per request
- **UnitOfWork is scoped per HTTP request** — one instance per request, registered as PerRequestLifetimeManager in Unity

---

## Project Structure

```
FakeNewsDetection/
├── App_Start/
│   ├── RouteConfig.cs
│   └── UnityConfig.cs
├── Controllers/
│   ├── HomeController.cs
│   ├── ArticleController.cs
│   └── ResultController.cs
├── Models/
│   ├── Article.cs
│   ├── DetectionResult.cs
│   └── ViewModels/
│       ├── ArticleSubmitVM.cs
│       └── ResultDisplayVM.cs
├── Repositories/
│   ├── IRepository.cs
│   ├── IArticleRepository.cs
│   ├── ArticleRepository.cs
│   ├── IDetectionResultRepository.cs
│   └── DetectionResultRepository.cs
├── UnitOfWork/
│   ├── IUnitOfWork.cs
│   └── UnitOfWork.cs
├── Services/
│   ├── IDetectionService.cs
│   ├── DetectionService.cs
│   ├── IArticleScraperService.cs
│   └── ArticleScraperService.cs
├── Infrastructure/
│   └── CassandraSessionFactory.cs
├── Views/
│   ├── Shared/
│   ├── Home/
│   ├── Article/
│   └── Result/
└── Web.config
```

---

## Layer Responsibilities

### Models/
- One file per entity: `Article.cs`, `DetectionResult.cs`
- Attributes come from two namespaces: `System.ComponentModel.DataAnnotations` for validation, `Cassandra.Mapping.Attributes` for table/column mapping
- Every entity must declare its Cassandra partition key and clustering key via attributes
- No methods, no logic — pure data containers
- ViewModels live in `Models/ViewModels/` and carry all input validation annotations; they are never persisted directly

### Infrastructure/
- `CassandraSessionFactory.cs` is the only place that creates the Cassandra cluster and session
- It also bootstraps `MappingConfiguration.Global` so the driver knows how to map entity classes to tables
- The session it returns is registered as a singleton in Unity

### Repositories/
- `IRepository.cs` defines the generic CRUD interface (GetById, GetAll, Add, Update, Delete) — all async
- Each entity gets its own interface extending the generic one, adding entity-specific query methods
- Concrete repository classes receive `ISession` via constructor injection and use the `IMapper` abstraction from the driver
- Repositories contain CQL queries — nowhere else in the project should CQL appear

### UnitOfWork/
- `IUnitOfWork` exposes one property per repository and a `CommitAsync()` method
- `UnitOfWork` owns the repositories and the session reference
- Controllers inject only `IUnitOfWork` — they never reference individual repository types directly
- `CommitAsync()` exists for interface consistency; Cassandra auto-commits individual statements

### Services/
- `IDetectionService` / `DetectionService` — sends article content to the Python Flask endpoint via HTTP POST, receives `{ label, confidence }` JSON, applies the confidence threshold rule, returns a `DetectionResult` object
- `IArticleScraperService` / `ArticleScraperService` — receives a URL, fetches raw HTML with proper browser headers, uses SmartReader to extract the main article body, uses HtmlAgilityPack to extract Open Graph metadata (title, author, publisher, date), returns a `ScrapedArticle` object
- Services never access Cassandra directly — they only produce or transform data

### Controllers/
- `HomeController` — serves the landing page only
- `ArticleController` — handles article submission (both manual text and URL scraping), delegates to scraper if URL provided, delegates to DetectionService, saves via UnitOfWork, redirects to result
- `ResultController` — reads a single article and its latest detection result from Cassandra via UnitOfWork, maps to ResultDisplayVM, renders the result view
- Every controller action that writes data must have `[ValidateAntiForgeryToken]`
- Controllers never instantiate Services or Repositories — everything comes via constructor injection

---

## Data Flow

```
User input (text or URL)
    ↓
ArticleController validates input (ModelState)
    ↓ if URL → ArticleScraperService fetches and extracts content
    ↓
DetectionService sends content to Flask ML endpoint
    ↓ receives { label, confidence }
    ↓ applies threshold: confidence ≥ 0.75 → use label, else UNCERTAIN
    ↓
UnitOfWork writes Article + DetectionResult to Cassandra
    ↓
Redirect to ResultController
    ↓
ResultController reads from Cassandra → renders verdict view
```

---

## Cassandra Data Design

### Tables

**articles** — primary read/write table
- Partition key: `article_id` (UUID)
- Stores: title, content, source_url, submitted_at, verdict, confidence_score
- Verdict is stored here (denormalized) so the result page needs only one read

**detection_results** — audit trail table
- Partition key: `article_id` (UUID)
- Clustering key: `analyzed_at` (timestamp, DESC)
- Stores: result_id, model_name, verdict, confidence
- Multiple model runs per article are supported; latest result is always first row

### Why Denormalization
Cassandra cannot JOIN tables. The verdict must be stored in `articles` so a result page read is a single partition lookup. `detection_results` exists for audit, history, and model comparison — not for the primary display path.

---

## Verdict Threshold Rule

The confidence score from the ML model drives the three-state verdict:
- Score ≥ 0.75 → use the model's label directly (FAKE or REAL)
- Score < 0.75 → override to UNCERTAIN regardless of label

This rule lives in `DetectionService` only — nowhere else.

---

## Dependency Injection (Unity)

Registration order and lifetime matter:

| Type | Lifetime |
|---|---|
| ISession (Cassandra) | Singleton — one for the whole app |
| IArticleRepository | PerRequest |
| IDetectionResultRepository | PerRequest |
| IUnitOfWork | PerRequest |
| IDetectionService | PerRequest |
| IArticleScraperService | PerRequest |

---

## NuGet Packages Required

| Package | Purpose |
|---|---|
| CassandraCSharpDriver | Cassandra session, mapper, CQL execution |
| Unity | Dependency injection container |
| Unity.Mvc5 | Unity integration with ASP.NET MVC request lifetime |
| Newtonsoft.Json | Deserializing ML service JSON response |
| HtmlAgilityPack | Parsing HTML for Open Graph metadata extraction |
| SmartReader | Mozilla Readability algorithm — extracts article body from any webpage |
| PuppeteerSharp | Headless Chromium fallback for JS-rendered pages (use only if SmartReader returns < 200 chars) |
| Microsoft.AspNet.Mvc | ASP.NET MVC 5 framework |

---

## Web.config Keys

All environment-specific values must come from Web.config appSettings — never hardcoded:

| Key | Example value |
|---|---|
| Cassandra:ContactPoint | 127.0.0.1 |
| Cassandra:Port | 9042 |
| Cassandra:Keyspace | fake_news_ks |
| ML:Endpoint | http://localhost:5000/predict |

---

## Common Mistakes to Avoid

| Mistake | Correct approach |
|---|---|
| Writing CQL inside a Controller action | Move the query to the appropriate Repository method |
| Calling `.Result` or `.Wait()` on async methods | Use `await` all the way up the call chain |
| Recreating ISession per request | Inject the singleton ISession registered in UnityConfig |
| Hardcoding verdict color logic in the controller | Verdict display logic belongs in the View or ViewModel |
| Forgetting `ModelState.Clear()` after scraping | After populating VM from scraper, clear and re-validate before checking ModelState.IsValid |
| Using `HttpClient` without User-Agent header | Always set browser-like headers or sites will block the scraper |
