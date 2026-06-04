using Cassandra.Mapping;
using fk_news_detector.Models;

namespace fk_news_detector.Repositories;

public class DetectionResultRepository : IDetectionResultRepository
{
    private readonly IMapper _mapper;

    public DetectionResultRepository(Cassandra.ISession session)
        => _mapper = new Mapper(session);

    // Récupère UN résultat par article_id
    // (retourne le plus récent grâce au CLUSTERING ORDER BY DESC)
    public async Task<DetectionResultEntity?> GetByIdAsync(Guid id)
    {
        try
        {
            return await _mapper.SingleOrDefaultAsync<DetectionResultEntity>(
                "WHERE article_id = ?", id);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error fetching result {id}: {ex.Message}", ex);
        }
    }

    // Récupère TOUS les résultats (toutes partitions)
    public async Task<IEnumerable<DetectionResultEntity>> GetAllAsync()
    {
        try
        {
            return await _mapper.FetchAsync<DetectionResultEntity>();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error fetching all results: {ex.Message}", ex);
        }
    }

    // Insère un nouveau résultat d'analyse
    // Cassandra ajoute une nouvelle ligne dans la partition article_id
    public async Task AddAsync(DetectionResultEntity entity)
    {
        try
        {
            await _mapper.InsertAsync(entity);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error inserting result: {ex.Message}", ex);
        }
    }

    public async Task UpdateAsync(DetectionResultEntity entity)
    {
        try
        {
            await _mapper.UpdateAsync(entity);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error updating result: {ex.Message}", ex);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        try
        {
            await _mapper.DeleteAsync<DetectionResultEntity>(
                "WHERE article_id = ?", id);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error deleting result {id}: {ex.Message}", ex);
        }
    }

    // ⭐ Méthode clé — utilisée par le lead pour afficher le verdict
    // LIMIT 1 + analyzed_at DESC = toujours le dernier résultat
    public async Task<DetectionResultEntity?> GetLatestByArticleIdAsync(Guid articleId)
    {
        try
        {
            return await _mapper.SingleOrDefaultAsync<DetectionResultEntity>(
                "WHERE article_id = ? LIMIT 1", articleId);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error fetching latest result: {ex.Message}", ex);
        }
    }

    // Historique complet de toutes les analyses d'un article
    public async Task<IEnumerable<DetectionResultEntity>> GetAllByArticleIdAsync(Guid articleId)
    {
        try
        {
            return await _mapper.FetchAsync<DetectionResultEntity>(
                "WHERE article_id = ?", articleId);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error fetching results by article: {ex.Message}", ex);
        }
    }
}