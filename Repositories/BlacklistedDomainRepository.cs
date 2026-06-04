using Cassandra.Mapping;
using fk_news_detector.Models;

namespace fk_news_detector.Repositories;

public class BlacklistedDomainRepository : IBlacklistedDomainRepository
{
    private readonly IMapper _mapper;

    public BlacklistedDomainRepository(Cassandra.ISession session)
        => _mapper = new Mapper(session);

   
    public Task<BlacklistedDomain?> GetByIdAsync(Guid id)
        => throw new NotSupportedException(
            "BlacklistedDomain uses domain (string) as key. Use GetByDomainAsync instead.");

    
    public async Task<IEnumerable<BlacklistedDomain>> GetAllAsync()
    {
        try
        {
            return await _mapper.FetchAsync<BlacklistedDomain>();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error fetching all domains: {ex.Message}", ex);
        }
    }

    // Insère un domaine blacklisté
    public async Task AddAsync(BlacklistedDomain entity)
    {
        try
        {
            await _mapper.InsertAsync(entity);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error inserting domain: {ex.Message}", ex);
        }
    }

    public async Task UpdateAsync(BlacklistedDomain entity)
    {
        try
        {
            await _mapper.UpdateAsync(entity);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error updating domain: {ex.Message}", ex);
        }
    }

    // Non utilisé — suppression par domain (string), pas par UUID
    public Task DeleteAsync(Guid id)
        => throw new NotSupportedException(
            "Use DeleteAsync(string domain) instead.");

  
    public async Task<bool> ExistsAsync(string domain)
    {
        try
        {
            var result = await _mapper.SingleOrDefaultAsync<BlacklistedDomain>(
                "WHERE domain = ?", domain);
            return result != null;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error checking domain: {ex.Message}", ex);
        }
    }

    // Récupère les détails complets d'un domaine
    // Utile pour afficher harm_score, label, source au lead
    public async Task<BlacklistedDomain?> GetByDomainAsync(string domain)
    {
        try
        {
            return await _mapper.SingleOrDefaultAsync<BlacklistedDomain>(
                "WHERE domain = ?", domain);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error fetching domain: {ex.Message}", ex);
        }
    }
}