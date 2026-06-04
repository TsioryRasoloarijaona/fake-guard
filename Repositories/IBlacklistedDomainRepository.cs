using fk_news_detector.Models;

namespace fk_news_detector.Repositories;

public interface IBlacklistedDomainRepository : IRepository<BlacklistedDomain>
{
    Task<bool> ExistsAsync(string domain);
    Task<BlacklistedDomain?> GetByDomainAsync(string domain);
}