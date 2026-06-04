using Cassandra.Mapping.Attributes;

namespace fk_news_detector.Models;

[Table("articles_by_date")]
public class ArticleByDate
{
    [PartitionKey]
    [Column("bucket")]
    public string Bucket { get; set; } = string.Empty;

    [ClusteringKey(0)]
    [Column("submitted_at")]
    public DateTimeOffset SubmittedAt { get; set; }

    [ClusteringKey(1)]
    [Column("article_id")]
    public Guid ArticleId { get; set; }

    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("source_url")]
    public string SourceUrl { get; set; } = string.Empty;

    [Column("verdict")]
    public string Verdict { get; set; } = string.Empty;

    [Column("confidence_score")]
    public double ConfidenceScore { get; set; }
}