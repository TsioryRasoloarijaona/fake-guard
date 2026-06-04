using Cassandra.Mapping;
using fk_news_detector.Models;

namespace fk_news_detector.Repositories;

public class ArticleRepository : IArticleRepository
{
    private readonly IMapper _mapper;

    public ArticleRepository(Cassandra.ISession session)
        => _mapper = new Mapper(session);

    public async Task<Article?> GetByIdAsync(Guid id)
    {
        try
        {
            return await _mapper.SingleOrDefaultAsync<Article>(
                "WHERE article_id = ?", id);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error fetching article {id}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<Article>> GetAllAsync()
    {
        try
        {
            return await _mapper.FetchAsync<Article>();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error fetching all articles: {ex.Message}", ex);
        }
    }

    public async Task AddAsync(Article entity)
    {
        try
        {
            await _mapper.InsertAsync(entity);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error inserting article: {ex.Message}", ex);
        }
    }

    public async Task UpdateAsync(Article entity)
    {
        try
        {
            await _mapper.UpdateAsync(entity);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error updating article: {ex.Message}", ex);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        try
        {
            await _mapper.DeleteAsync<Article>("WHERE article_id = ?", id);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error deleting article {id}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<Article>> GetByBucketAsync(string bucket)
    {
        try
        {
            return await _mapper.FetchAsync<ArticleByDate>(
                "WHERE bucket = ?", bucket)
                .ContinueWith(t => t.Result.Select(a => new Article
                {
                    ArticleId       = a.ArticleId,
                    Title           = a.Title,
                    SourceUrl       = a.SourceUrl,
                    SubmittedAt     = a.SubmittedAt,
                    Verdict         = a.Verdict,
                    ConfidenceScore = a.ConfidenceScore
                }));
        }
        catch (Exception ex)
        {
            throw new Exception($"Error fetching articles by bucket: {ex.Message}", ex);
        }
    }

    public async Task AddToHistoryAsync(ArticleByDate articleByDate)
    {
        try
        {
            await _mapper.InsertAsync(articleByDate);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error inserting to history: {ex.Message}", ex);
        }
    }
}