using Cassandra.Mapping.Attributes;

namespace fk_news_detector.Models;

[Table("articles")]
public class Article
{
    [PartitionKey]
    [Column("article_id")]
    public Guid ArticleId { get; set; }

    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("content")]
    public string Content { get; set; } = string.Empty;

    [Column("source_url")]
    public string SourceUrl { get; set; } = string.Empty;

    [Column("submitted_at")]
    public DateTimeOffset SubmittedAt { get; set; }

    [Column("verdict")]
    public string Verdict { get; set; } = string.Empty;

    [Column("confidence_score")]
    public double ConfidenceScore { get; set; }
}