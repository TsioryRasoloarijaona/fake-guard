using fk_news_detector.Repositories;

namespace fk_news_detector.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    public IArticleRepository Articles { get; }
    public IDetectionResultRepository DetectionResults { get; }
    public IBlacklistedDomainRepository BlacklistedDomains { get; }

    public UnitOfWork(Cassandra.ISession session)
    {
        Articles          = new ArticleRepository(session);
        DetectionResults  = new DetectionResultRepository(session);
        BlacklistedDomains = new BlacklistedDomainRepository(session);
    }

    public Task CommitAsync()
    {
        // Cassandra auto-commit sur chaque statement
        // Cette méthode existe pour la cohérence de l'interface
        return Task.CompletedTask;
    }
}