using fk_news_detector.Models;

namespace fk_news_detector.Repositories;

public interface IArticleRepository
{
    Task SaveAsync(Article article);
    Task<Article?> GetByIdAsync(Guid articleId);
    Task<IEnumerable<Article>> GetAllAsync();
}
