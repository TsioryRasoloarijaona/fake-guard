using fk_news_detector.Repositories;

namespace fk_news_detector.UnitOfWork;

public interface IUnitOfWork
{
    IArticleRepository Articles { get; }
    IDetectionResultRepository DetectionResults { get; }
    IBlacklistedDomainRepository BlacklistedDomains { get; }
    Task CommitAsync();
}