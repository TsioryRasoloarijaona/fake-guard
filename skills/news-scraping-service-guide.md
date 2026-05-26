# News URL Scraping Service — Logic Guide

## Context

This guide describes the logic for implementing a URL-based news content
extraction service in an existing ASP.NET MVC 5 project that uses the
Repository + Unit of Work pattern and Apache Cassandra as its database.
You are adding one isolated service. Nothing in the existing pipeline changes.

---

## What This Service Does

The service takes a news article URL as input and returns a structured object
containing the article title, body content, author, and published date.
It is best-effort — it must never throw to the caller, always returning a
result object that indicates either success or a human-readable failure reason.

---

## Package Dependency

You need one NuGet package: **HtmlAgilityPack**. It parses raw HTML into a
traversable tree that you can query with XPath expressions. Nothing else is needed.

---

## Files to Create

- `Models/ExtractedArticle.cs` — the result transfer object
- `Services/INewsExtractionService.cs` — the interface
- `Services/NewsExtractionService.cs` — the implementation

One existing file to modify:

- `App_Start/UnityConfig.cs` — register the new service

---

## ExtractedArticle Model

A plain C# class with no Cassandra annotations and no validation attributes.
It is not persisted — it only carries data between the service and the controller.

Fields it needs: title, content, source URL, author, published date (nullable),
a boolean indicating whether extraction succeeded, and an error message string
for when it does not.

---

## The Service Interface

One async method that takes a URL string and returns an `ExtractedArticle`.

---

## The Service Implementation — Logic Breakdown

### HttpClient Setup

Declare `HttpClient` as a **static field** on the class, not instantiated per
call. Per-call instantiation causes socket exhaustion. Configure it once with
a realistic browser User-Agent header, an Accept header, and an Accept-Language
header. Without these, most news websites return 403 Forbidden. Set a timeout
of around 15 seconds.

---

### Main Method Logic — in order

**Step 1 — Validate the URL**
Before making any network call, check that the URL is well-formed and uses
either http or https. Return immediately with an error message if not.

**Step 2 — Fetch the HTML**
Make an async GET request using the static HttpClient. Read the response bytes
and decode them using the encoding declared in the response Content-Type header,
not always UTF-8. Fall back to UTF-8 if the declared encoding is unrecognised.

**Step 3 — Parse into an HtmlDocument**
Load the raw HTML string into an HtmlAgilityPack HtmlDocument. This turns the
string into a tree you can query.

**Step 4 — Strip noise from the tree**
Before extracting anything, remove elements that are guaranteed to contain no
article text. This includes by tag name: script, style, nav, header, footer,
aside, form, noscript, iframe, button. Also remove any element whose CSS class
contains noise keywords such as: ad, advertisement, related, comment, social,
share, newsletter, sidebar, widget, breadcrumb, promo, popup.
Do this destructively on the document before running any extraction query.

**Step 5 — Extract each field** (described individually below)

**Step 6 — Decide if extraction succeeded**
Mark extraction as succeeded only if the content field is non-empty and at least
100 characters long. Below that threshold the result is not useful for detection.

**Step 7 — Wrap all of the above in a try/catch**
Catch timeout exceptions, HTTP exceptions, and general exceptions separately
so the error message is meaningful to the user. Never let an exception propagate
to the controller.

---

### Title Extraction — Priority Chain

Try each source in order, return the first non-empty result:

1. The `og:title` Open Graph meta tag — editors set this explicitly for sharing,
   so it is clean and does not contain the site name
2. The `twitter:title` meta tag — same idea, different standard
3. The first `<h1>` element found inside an `<article>` or `<main>` container
4. The page `<title>` tag as a last resort — strip any site name suffix that
   appears after a pipe, dash, or em dash character

Always HTML-decode the result before returning it.

---

### Content Extraction — Three Fallback Strategies

Try each strategy in order. Return the result of the first one that produces
at least 100 characters of text, after cleaning.

**Strategy 1 — HTML5 article tag**
Look for a single `<article>` element. When a site uses it correctly it contains
exactly the article body and nothing else. This is the best case.

**Strategy 2 — Known CSS class and attribute patterns**
Different news sites use different class names for their content containers but
there is a common vocabulary. Query for divs whose class or id contains patterns
like: article-body, article-content, story-body, post-content, entry-content.
Also check for elements with the `itemprop="articleBody"` attribute, which is
a schema.org standard some sites follow. Try each pattern and return the first
that yields enough text.

**Strategy 3 — Largest paragraph block heuristic**
When no known pattern matches, find the `<div>` element that contains the most
total text across all its `<p>` descendants. That div is almost always the
article body. This is a fallback for unknown site structures.

---

### Author Extraction

Try in order:
1. The `author` meta tag
2. Any visible element whose CSS class contains "author" or "byline"

Return null if neither yields anything. Author is optional — do not fail
extraction because of a missing author.

---

### Published Date Extraction

Try in order:
1. The `article:published_time` Open Graph meta tag
2. The `pubdate` or `date` meta tags
3. A `<time>` element with a `datetime` attribute

Parse the value as a DateTime. Return null if nothing is found or nothing parses.
Date is optional — do not fail extraction because of a missing date.

---

### Text Cleaning

After extracting any inner text from an HTML node, clean it before storing:
decode HTML entities (e.g. `&amp;`, `&nbsp;`), collapse multiple spaces and
tabs into a single space, and collapse more than two consecutive newlines into
two. Trim leading and trailing whitespace.

---

## Dependency Injection Registration

Register the service as a **singleton** in UnityConfig, not per-request.
It must be singleton because it owns the static HttpClient. A new instance
per request would not cause errors here but it is semantically wrong and
wastes resources.

---

## Controller Integration

Add `INewsExtractionService` as a constructor parameter to `ArticleController`
alongside the existing `IUnitOfWork` and `IDetectionService`. Unity will inject
it automatically once registered.

Add one new action `FetchFromUrl` that accepts a URL via POST, calls the
extraction service, and returns a JSON response. On success the JSON contains
the title, content, author, and published date. On failure it contains a
success flag set to false and the error message. The existing `Submit` action
does not change at all.

---

## Known Limitations to Be Aware Of

- Sites that load article content via JavaScript after page load will return
  an empty skeleton. The extraction will fail the 100-character threshold and
  report failure gracefully. There is no fix for this without a headless browser.
- Sites behind paywalls return only teaser content, which is usually too short
  to pass the threshold.
- Some sites aggressively block automated requests even with correct headers.
  This surfaces as an HTTP error which is caught and returned as an error message.

---

## Implementation Order

1. Install HtmlAgilityPack via NuGet
2. Create the ExtractedArticle model
3. Create the interface
4. Create the service — implement the main method first, then each private
   helper in the order they are called
5. Register in UnityConfig
6. Update the ArticleController constructor and add the FetchFromUrl action
7. Test manually with a known-good news URL such as BBC, Reuters, or AP News
