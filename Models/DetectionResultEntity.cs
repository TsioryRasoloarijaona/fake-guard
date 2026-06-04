using Cassandra.Mapping.Attributes;

namespace fk_news_detector.Models;

[Table("detection_results")]
public class DetectionResultEntity
{
    [PartitionKey]
    [Column("article_id")]
    public Guid ArticleId { get; set; }

    [ClusteringKey(0)]
    [Column("analyzed_at")]
    public DateTimeOffset AnalyzedAt { get; set; }

    [Column("result_id")]
    public Guid ResultId { get; set; }

    [Column("model_name")]
    public string ModelName { get; set; } = string.Empty;

    [Column("verdict")]
    public string Verdict { get; set; } = string.Empty;

    [Column("confidence")]
    public double Confidence { get; set; }
}