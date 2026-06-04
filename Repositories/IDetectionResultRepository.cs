using fk_news_detector.Models;

namespace fk_news_detector.Repositories;

public interface IDetectionResultRepository : IRepository<DetectionResultEntity>
{
    Task<DetectionResultEntity?> GetLatestByArticleIdAsync(Guid articleId);
    Task<IEnumerable<DetectionResultEntity>> GetAllByArticleIdAsync(Guid articleId);
}