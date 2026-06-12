using fk_news_detector.Repositories;

namespace fk_news_detector.UnitOfWork;

public interface IUnitOfWork
{
    IArticleRepository Articles { get; }
    IArticleLookupRepository ArticleLookups { get; }
    Task CommitAsync();
}
