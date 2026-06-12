using System.Security.Cryptography;
using System.Text;
using fk_news_detector.Models;
using fk_news_detector.UnitOfWork;

namespace fk_news_detector.Services;

public class ArticleService : IArticleService
{
    private readonly IUnitOfWork _uow;
    private readonly IDetectionService _detection;

    public ArticleService(IUnitOfWork uow, IDetectionService detection)
    {
        _uow = uow;
        _detection = detection;
    }

    public async Task<Article> GetOrAnalyzeAsync(ExtractedArticle extracted)
    {
        var fingerprint = ComputeFingerprint(extracted);

        // Check if same article was already analyzed
        var lookup = await _uow.ArticleLookups.GetByFingerprintAsync(fingerprint);
        if (lookup is not null)
        {
            var existing = await _uow.Articles.GetByIdAsync(lookup.ArticleId);
            if (existing is not null)
                return existing;
        }

        // New article — run ML detection
        var result = await _detection.DetectAsync(
            extracted.Content,
            extracted.Title,
            extracted.SourceUrl);

        var article = new Article
        {
            ArticleId    = Guid.NewGuid(),
            SubmittedAt  = DateTimeOffset.UtcNow,
            Title        = extracted.Title,
            Content      = extracted.Content,
            SourceUrl    = string.IsNullOrWhiteSpace(extracted.SourceUrl) ? null : extracted.SourceUrl,
            Author       = extracted.Author,
            PublishedDate = extracted.PublishedDate.HasValue
                ? new DateTimeOffset(DateTime.SpecifyKind(extracted.PublishedDate.Value, DateTimeKind.Utc))
                : null,
            Verdict      = result.Verdict,
            Confidence   = result.Confidence ?? 0,
            ModelName    = "fakeguard-v1"
        };

        await _uow.Articles.SaveAsync(article);
        await _uow.ArticleLookups.SaveAsync(new ArticleLookup
        {
            Fingerprint = fingerprint,
            ArticleId   = article.ArticleId
        });
        await _uow.CommitAsync();

        return article;
    }
    
    private static string ComputeFingerprint(ExtractedArticle extracted)
    {
        if (!string.IsNullOrWhiteSpace(extracted.SourceUrl))
            return extracted.SourceUrl.Trim().ToLowerInvariant();

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(extracted.Content.Trim()));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
