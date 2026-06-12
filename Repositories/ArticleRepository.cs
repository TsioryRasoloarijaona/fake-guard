using Cassandra.Mapping;
using fk_news_detector.Models;

namespace fk_news_detector.Repositories;

public class ArticleRepository : IArticleRepository
{
    private readonly IMapper _mapper;

    public ArticleRepository(Cassandra.ISession session)
    {
        _mapper = new Mapper(session);
    }

    public Task SaveAsync(Article article)
        => _mapper.InsertAsync(article);

    public async Task<Article?> GetByIdAsync(Guid articleId)
        => await _mapper.SingleOrDefaultAsync<Article>("WHERE article_id = ?", articleId);

    public async Task<IEnumerable<Article>> GetAllAsync()
    {
        var result = await _mapper.FetchAsync<Article>();
        return result.ToList();
    }
}
