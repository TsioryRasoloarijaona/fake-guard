using Cassandra.Mapping;
using fk_news_detector.Models;

namespace fk_news_detector.Repositories;

public class ArticleLookupRepository : IArticleLookupRepository
{
    private readonly IMapper _mapper;

    public ArticleLookupRepository(Cassandra.ISession session)
    {
        _mapper = new Mapper(session);
    }

    public Task SaveAsync(ArticleLookup lookup)
        => _mapper.InsertAsync(lookup);

    public async Task<ArticleLookup?> GetByFingerprintAsync(string fingerprint)
        => await _mapper.SingleOrDefaultAsync<ArticleLookup>("WHERE fingerprint = ?", fingerprint);
}
