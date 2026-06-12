using Cassandra;

namespace fk_news_detector.Infrastructure;

public static class CassandraSessionFactory
{
    public static Cassandra.ISession Create(IConfiguration config)
    {
        var contactPoint = config["Cassandra:ContactPoint"]!;
        var port = int.Parse(config["Cassandra:Port"]!);
        var keyspace = config["Cassandra:Keyspace"]!;

        var cluster = Cluster.Builder()
            .AddContactPoint(contactPoint)
            .WithPort(port)
            .Build();

        return cluster.Connect(keyspace);
    }
}
