using Cassandra.Mapping.Attributes;

namespace fk_news_detector.Models;

[Table("blacklisted_domains")]
public class BlacklistedDomain
{
    [PartitionKey]
    [Column("domain")]
    public string Domain { get; set; } = string.Empty;

    [Column("label")]
    public string Label { get; set; } = string.Empty;

    [Column("source")]
    public string Source { get; set; } = string.Empty;

    [Column("last_update")]
    public string LastUpdate { get; set; } = string.Empty;

    [Column("type")]
    public string Type { get; set; } = string.Empty;

    [Column("harm_score")]
    public int HarmScore { get; set; }

    [Column("accuracy")]
    public int Accuracy { get; set; }

    [Column("transparency")]
    public int Transparency { get; set; }
}