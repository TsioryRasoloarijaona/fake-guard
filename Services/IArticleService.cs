using fk_news_detector.Models;

namespace fk_news_detector.Services;

public interface IArticleService
{
    Task<Article> GetOrAnalyzeAsync(ExtractedArticle extracted);
}
