using fk_news_detector.Repositories;

namespace fk_news_detector.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    public IArticleRepository Articles { get; }
    public IArticleLookupRepository ArticleLookups { get; }

    public UnitOfWork(Cassandra.ISession session)
    {
        Articles = new ArticleRepository(session);
        ArticleLookups = new ArticleLookupRepository(session);
    }

    public Task CommitAsync() => Task.CompletedTask;
}
