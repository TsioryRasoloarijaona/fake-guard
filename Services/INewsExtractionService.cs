using fk_news_detector.Models;

namespace fk_news_detector.Services;

public interface INewsExtractionService
{
    Task<ExtractedArticle> ExtractAsync(string url);
}
