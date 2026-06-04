using Cassandra.Mapping;
using fk_news_detector.Models;

namespace fk_news_detector.Infrastructure;

public static class CassandraSessionFactory
{
    private static bool _mappingConfigured;

    public static Cassandra.ISession CreateSession(IConfiguration configuration)
    {
        var contactPoint = configuration["Cassandra:ContactPoint"] ?? "127.0.0.1";
        var port = int.Parse(configuration["Cassandra:Port"] ?? "9042");
        var keyspace = configuration["Cassandra:Keyspace"] ?? "fake_news_ks";

        var cluster = Cassandra.Cluster.Builder()
            .AddContactPoint(contactPoint)
            .WithPort(port)
            .Build();

        var session = cluster.Connect(keyspace);

        if (!_mappingConfigured)
        {
            ConfigureMappings();
            _mappingConfigured = true;
        }

        return session;
    }

    private static void ConfigureMappings()
    {
        MappingConfiguration.Global.Define(

            new Map<Article>()
                .TableName("articles")
                .PartitionKey(a => a.ArticleId)
                .Column(a => a.ArticleId, cm => cm.WithName("article_id"))
                .Column(a => a.Title, cm => cm.WithName("title"))
                .Column(a => a.Content, cm => cm.WithName("content"))
                .Column(a => a.SourceUrl, cm => cm.WithName("source_url"))
                .Column(a => a.SubmittedAt, cm => cm.WithName("submitted_at"))
                .Column(a => a.Verdict, cm => cm.WithName("verdict"))
                .Column(a => a.ConfidenceScore, cm => cm.WithName("confidence_score")),

            new Map<ArticleByDate>()
                .TableName("articles_by_date")
                .PartitionKey(a => a.Bucket)
                .Column(a => a.Bucket, cm => cm.WithName("bucket"))
                .Column(a => a.SubmittedAt, cm => cm.WithName("submitted_at"))
                .Column(a => a.ArticleId, cm => cm.WithName("article_id"))
                .Column(a => a.Title, cm => cm.WithName("title"))
                .Column(a => a.SourceUrl, cm => cm.WithName("source_url"))
                .Column(a => a.Verdict, cm => cm.WithName("verdict"))
                .Column(a => a.ConfidenceScore, cm => cm.WithName("confidence_score")),

            new Map<DetectionResultEntity>()
                .TableName("detection_results")
                .PartitionKey(d => d.ArticleId)
                .Column(d => d.ArticleId, cm => cm.WithName("article_id"))
                .Column(d => d.AnalyzedAt, cm => cm.WithName("analyzed_at"))
                .Column(d => d.ResultId, cm => cm.WithName("result_id"))
                .Column(d => d.ModelName, cm => cm.WithName("model_name"))
                .Column(d => d.Verdict, cm => cm.WithName("verdict"))
                .Column(d => d.Confidence, cm => cm.WithName("confidence")),

            new Map<BlacklistedDomain>()
                .TableName("blacklisted_domains")
                .PartitionKey(b => b.Domain)
                .Column(b => b.Domain, cm => cm.WithName("domain"))
                .Column(b => b.Label, cm => cm.WithName("label"))
                .Column(b => b.Source, cm => cm.WithName("source"))
                .Column(b => b.LastUpdate, cm => cm.WithName("last_update"))
                .Column(b => b.Type, cm => cm.WithName("type"))
                .Column(b => b.HarmScore, cm => cm.WithName("harm_score"))
                .Column(b => b.Accuracy, cm => cm.WithName("accuracy"))
                .Column(b => b.Transparency, cm => cm.WithName("transparency"))
        );
    }
}