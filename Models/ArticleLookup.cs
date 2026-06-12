using Cassandra.Mapping.Attributes;

namespace fk_news_detector.Models;

[Table("article_lookup")]
public class ArticleLookup
{
    [PartitionKey]
    [Column("fingerprint")]
    public string Fingerprint { get; set; } = string.Empty;

    [Column("article_id")]
    public Guid ArticleId { get; set; }
}
