using fk_news_detector.Models;

namespace fk_news_detector.Repositories;

public interface IArticleRepository : IRepository<Article>
{
    Task<IEnumerable<Article>> GetByBucketAsync(string bucket);
    Task AddToHistoryAsync(ArticleByDate articleByDate);
}