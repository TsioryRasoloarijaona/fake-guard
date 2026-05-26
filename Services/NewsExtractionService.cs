using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using fk_news_detector.Models;
using HtmlAgilityPack;

namespace fk_news_detector.Services;

public class NewsExtractionService : INewsExtractionService
{
    private static readonly HttpClient _http = new()
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    static NewsExtractionService()
    {
        _http.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");
        _http.DefaultRequestHeaders.Add("Accept",
            "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        _http.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
    }

    public async Task<ExtractedArticle> ExtractAsync(string url)
    {
        try
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                return Fail(url, "Invalid URL — must start with http:// or https://");
            }

            var response = await _http.GetAsync(uri);
            response.EnsureSuccessStatusCode();

            var bytes = await response.Content.ReadAsByteArrayAsync();
            var encoding = TryGetEncoding(response.Content.Headers.ContentType?.CharSet) ?? Encoding.UTF8;
            var html = encoding.GetString(bytes);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            StripNoise(doc);

            var title = ExtractTitle(doc);
            var content = ExtractContent(doc);
            var author = ExtractAuthor(doc);
            var date = ExtractDate(doc);

            var success = !string.IsNullOrWhiteSpace(content) && content.Length >= 100;

            return new ExtractedArticle
            {
                Title = title,
                Content = content,
                SourceUrl = url,
                Author = author,
                PublishedDate = date,
                Success = success,
                ErrorMessage = success ? null : "Could not extract enough article content from this URL."
            };
        }
        catch (TaskCanceledException)
        {
            return Fail(url, "Request timed out — the site took too long to respond.");
        }
        catch (HttpRequestException ex)
        {
            return Fail(url, $"HTTP error fetching the URL: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Fail(url, $"Unexpected error: {ex.Message}");
        }
    }

    private static void StripNoise(HtmlDocument doc)
    {
        var noiseTags = new[] { "script", "style", "nav", "header", "footer", "aside", "form", "noscript", "iframe", "button" };
        foreach (var tag in noiseTags)
        {
            foreach (var node in doc.DocumentNode.SelectNodes($"//{tag}") ?? Enumerable.Empty<HtmlNode>())
                node.Remove();
        }

        var noiseKeywords = new[] { "ad", "advertisement", "related", "comment", "social", "share", "newsletter", "sidebar", "widget", "breadcrumb", "promo", "popup" };
        var toRemove = doc.DocumentNode
            .SelectNodes("//*[@class or @id]")?
            .Where(n =>
            {
                var cls = (n.GetAttributeValue("class", "") + " " + n.GetAttributeValue("id", "")).ToLowerInvariant();
                return noiseKeywords.Any(kw => cls.Contains(kw));
            })
            .ToList() ?? [];

        foreach (var node in toRemove)
            node.Remove();
    }

    private static string ExtractTitle(HtmlDocument doc)
    {
        var ogTitle = doc.DocumentNode.SelectSingleNode("//meta[@property='og:title']")?.GetAttributeValue("content", "");
        if (!string.IsNullOrWhiteSpace(ogTitle)) return WebUtility.HtmlDecode(ogTitle.Trim());

        var twitterTitle = doc.DocumentNode.SelectSingleNode("//meta[@name='twitter:title']")?.GetAttributeValue("content", "");
        if (!string.IsNullOrWhiteSpace(twitterTitle)) return WebUtility.HtmlDecode(twitterTitle.Trim());

        var articleH1 = doc.DocumentNode.SelectSingleNode("//article//h1 | //main//h1")?.InnerText;
        if (!string.IsNullOrWhiteSpace(articleH1)) return WebUtility.HtmlDecode(articleH1.Trim());

        var pageTitle = doc.DocumentNode.SelectSingleNode("//title")?.InnerText;
        if (!string.IsNullOrWhiteSpace(pageTitle))
        {
            var cleaned = Regex.Replace(pageTitle, @"[\|\-—].*$", "").Trim();
            return WebUtility.HtmlDecode(cleaned);
        }

        return string.Empty;
    }

    private static string ExtractContent(HtmlDocument doc)
    {
        // Strategy 1: <article>
        var article = doc.DocumentNode.SelectSingleNode("//article");
        if (article != null)
        {
            var text = CleanText(article.InnerText);
            if (text.Length >= 100) return text;
        }

        // Strategy 2: known class/id patterns + itemprop
        var patterns = new[] { "article-body", "article-content", "story-body", "post-content", "entry-content" };
        foreach (var pattern in patterns)
        {
            var node = doc.DocumentNode.SelectSingleNode(
                $"//*[contains(@class,'{pattern}') or contains(@id,'{pattern}')]");
            if (node != null)
            {
                var text = CleanText(node.InnerText);
                if (text.Length >= 100) return text;
            }
        }

        var itemprop = doc.DocumentNode.SelectSingleNode("//*[@itemprop='articleBody']");
        if (itemprop != null)
        {
            var text = CleanText(itemprop.InnerText);
            if (text.Length >= 100) return text;
        }

        // Strategy 3: div with most paragraph text
        var bestDiv = doc.DocumentNode
            .SelectNodes("//div[.//p]")?
            .Select(div => new { div, len = string.Concat(div.SelectNodes(".//p")?.Select(p => p.InnerText) ?? []).Length })
            .OrderByDescending(x => x.len)
            .FirstOrDefault();

        if (bestDiv != null)
        {
            var text = CleanText(bestDiv.div.InnerText);
            if (text.Length >= 100) return text;
        }

        // Strategy 4: concatenate all <p> elements as a last resort
        var paragraphs = doc.DocumentNode.SelectNodes("//p");
        if (paragraphs != null)
        {
            var text = CleanText(string.Join("\n\n", paragraphs.Select(p => p.InnerText.Trim()).Where(t => t.Length > 0)));
            if (text.Length > 0) return text;
        }

        return string.Empty;
    }

    private static string? ExtractAuthor(HtmlDocument doc)
    {
        var meta = doc.DocumentNode.SelectSingleNode("//meta[@name='author']")?.GetAttributeValue("content", "");
        if (!string.IsNullOrWhiteSpace(meta)) return WebUtility.HtmlDecode(meta.Trim());

        var byline = doc.DocumentNode.SelectSingleNode(
            "//*[contains(@class,'author') or contains(@class,'byline')]");
        if (byline != null)
        {
            var text = CleanText(byline.InnerText);
            if (!string.IsNullOrWhiteSpace(text)) return text;
        }

        return null;
    }

    private static DateTime? ExtractDate(HtmlDocument doc)
    {
        var sources = new[]
        {
            doc.DocumentNode.SelectSingleNode("//meta[@property='article:published_time']")?.GetAttributeValue("content", ""),
            doc.DocumentNode.SelectSingleNode("//meta[@name='pubdate']")?.GetAttributeValue("content", ""),
            doc.DocumentNode.SelectSingleNode("//meta[@name='date']")?.GetAttributeValue("content", ""),
            doc.DocumentNode.SelectSingleNode("//time[@datetime]")?.GetAttributeValue("datetime", ""),
        };

        foreach (var src in sources)
        {
            if (!string.IsNullOrWhiteSpace(src) && DateTime.TryParse(src, out var dt))
                return dt;
        }

        return null;
    }

    private static string CleanText(string raw)
    {
        var decoded = WebUtility.HtmlDecode(raw);
        decoded = Regex.Replace(decoded, @"[ \t]+", " ");
        decoded = Regex.Replace(decoded, @"\n{3,}", "\n\n");
        return decoded.Trim();
    }

    private static Encoding? TryGetEncoding(string? charSet)
    {
        if (string.IsNullOrWhiteSpace(charSet)) return null;
        try { return Encoding.GetEncoding(charSet); }
        catch { return null; }
    }

    private static ExtractedArticle Fail(string url, string message) =>
        new() { SourceUrl = url, Success = false, ErrorMessage = message };
}
