using fk_news_detector.Models;

namespace fk_news_detector.Repositories;

public interface IArticleLookupRepository
{
    Task SaveAsync(ArticleLookup lookup);
    Task<ArticleLookup?> GetByFingerprintAsync(string fingerprint);
}
