# VIEWS.md — ASP.NET MVC Views Architecture
## Fake News Detection Project

---

## What This File Covers

All rules and conventions for generating Razor views (.cshtml) in this project.
No business logic lives in views. No inline styles. No hardcoded routes.
Every view is strongly typed. Every form is protected.

---

## Stack

| Concern | Technology |
|---|---|
| Template engine | Razor (.cshtml) |
| CSS framework | Bootstrap 4 (CDN, no build step) |
| JavaScript | Vanilla JS only — no extra jQuery plugins |
| Icons / glyphs | Unicode emoji only — no icon font dependency |
| Validation | ASP.NET Data Annotations + jQuery Unobtrusive Validation |

---

## Absolute Rules

- Every view declares `@model` with its exact strongly-typed ViewModel or Model
- Every view sets `ViewBag.Title` — used by `_Layout.cshtml` for the `<title>` tag
- Every form includes `@Html.AntiForgeryToken()` — no exceptions
- Every input field has a paired `@Html.ValidationMessageFor()` call below it
- Every form view includes `@Html.ValidationSummary()` at the top of the form
- All navigation links use `@Url.Action()` — never hardcoded href paths
- All scripts go inside `@section Scripts { }` — never inline outside the section
- No inline `<style>` blocks anywhere — use Bootstrap utility classes or `site.css`
- Verdict color (FAKE / REAL / UNCERTAIN) is always computed via a local variable mapped to a Bootstrap contextual class (`danger` / `success` / `warning`) — never hardcoded per-verdict

---

## Layout Architecture

### _ViewStart.cshtml
- Sets `_Layout.cshtml` as the default layout for all views
- No other logic

### Views/Shared/_Layout.cshtml
- Single master layout for the entire application
- Contains: `<head>` with Bootstrap CDN, `<nav>` with site name and two nav links, `<main>` with `@RenderBody()`, `<footer>` with copyright, jQuery and Bootstrap JS CDN at bottom, `@RenderSection("Scripts", required: false)` before closing body
- Nav links: "Analyze Article" → `Article/Submit` and "History" → `Article/Index`
- Site name: **FakeGuard**
- Color scheme: dark navbar (`#1a1a2e`), accent color red (`#e94560`)

### Views/Shared/_ValidationScripts.cshtml
- Partial view that renders the jQuery validation bundle
- Referenced in every view that contains a form via `@Html.Partial("_ValidationScripts")`

---

## Views Inventory

### Home/Index.cshtml

**Purpose:** Landing page. No model required.

**Layout:**
- Centered hero section with headline and subheadline
- Two equal-width cards side by side:
  - Card 1 — "Paste Text" → links to `Article/Submit`
  - Card 2 — "Drop a URL" → links to `Article/Submit` with a query param indicating URL mode
- Link to history page below the cards

**No form. No model. No validation.**

---

### Article/Submit.cshtml

**Purpose:** Article submission form — the main user-facing input page.

**Model:** `ArticleSubmitVM`

**Layout:**
- Validation summary at the top
- Two-tab interface (Bootstrap tabs):
  - Tab 1 "Paste Text" — fields: Title (text input), Content (textarea, 10 rows), Source URL (text input, optional)
  - Tab 2 "From URL" — fields: Article URL (text input), info alert explaining auto-scrape behavior
- Submit button at the bottom spanning full width
- On submit: button text changes to a loading indicator and is disabled (JS, no server roundtrip)

**Validation display:** Each field has `ValidationMessageFor` immediately below it. Summary at top of form.

**Scripts section:** jQuery validation partial + loading state JS.

---

### Result/Details.cshtml

**Purpose:** Display the verdict for a single analyzed article.

**Model:** `ResultDisplayVM`

**Layout (top to bottom):**

1. **Verdict card** (Bootstrap card, border color = `badgeClass`)
   - Large emoji icon (❌ FAKE / ✅ REAL / ⚠️ UNCERTAIN)
   - Large badge with verdict text, color = `badgeClass`
   - Bootstrap progress bar showing confidence percentage, color = `badgeClass`
   - Small text showing model name

2. **Article details card**
   - Article title
   - Source URL as a clickable external link (if present)
   - Submission timestamp (formatted: dd MMM yyyy, HH:mm UTC)

3. **Explanation card** (light background)
   - One paragraph of plain-language explanation of what the verdict means
   - Text color matches verdict: red for FAKE, green for REAL, yellow for UNCERTAIN

4. **Navigation row**
   - Left: "← Analyze Another" button → `Article/Submit`
   - Right: "View History" button → `Article/Index`

**Local variable convention:**
```
badgeClass  =  "danger"  | "success"  | "warning"
verdictIcon =  "❌"      | "✅"       | "⚠️"
confidencePct = (int)(Model.ConfidenceScore * 100)
```
These three variables are declared at the top of the view's `@{ }` block and used throughout — verdict appearance is never repeated inline.

---

### Article/Index.cshtml

**Purpose:** List of all recently analyzed articles (history page).

**Model:** `IEnumerable<Article>`

**Layout:**
- Page title + "New Analysis" button (top right, links to `Article/Submit`)
- Empty state: info alert with a link to submit if the list is empty
- Non-empty state: Bootstrap list-group, one item per article
  - Each item is a clickable link to `Result/Details` for that article
  - Left side: article title, source URL (small muted text), submission date
  - Right side: verdict badge (color = `badgeClass`) + confidence percentage
- Left border accent on each list item using the project's red accent color

**No form. No validation. Read-only page.**

---

## ViewModel → View Mapping

| View | ViewModel / Model |
|---|---|
| Home/Index | None |
| Article/Submit | ArticleSubmitVM |
| Result/Details | ResultDisplayVM |
| Article/Index | IEnumerable\<Article\> |

---

## Bootstrap Classes Used (Reference)

| Element | Class |
|---|---|
| Verdict badge | `badge badge-{badgeClass} badge-pill` |
| Confidence bar | `progress-bar bg-{badgeClass}` |
| Form validation error | `text-danger small` |
| Validation summary | `alert alert-danger` |
| Info alert (URL tab) | `alert alert-info` |
| Submit button | `btn btn-danger btn-lg btn-block` |
| Nav links | `btn btn-outline-secondary` / `btn btn-outline-dark` |
| List group history | `list-group list-group-item list-group-item-action` |
| Cards | `card shadow` / `card shadow-sm` |

---

## View Checklist

Before considering any view complete, verify:

- [ ] `@model` directive is present and correct
- [ ] `ViewBag.Title` is set
- [ ] `@Html.AntiForgeryToken()` present on every form
- [ ] `@Html.ValidationSummary()` present at top of every form
- [ ] Every input has `@Html.ValidationMessageFor()` below it
- [ ] All links use `@Url.Action()` — no hardcoded paths
- [ ] Verdict color uses `badgeClass` variable — not hardcoded per-verdict
- [ ] Scripts are inside `@section Scripts { }` — not inline
- [ ] No `<style>` blocks in the view body
- [ ] Loading state JS disables submit button on form submit
