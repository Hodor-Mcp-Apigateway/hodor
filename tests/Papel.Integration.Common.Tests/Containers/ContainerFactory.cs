namespace Papel.Integration.Common.Tests.Containers;

public static class ContainerFactory
{
    private const string Database = "template_db";
    private const string PostgresUsername = "postgres";
    private const string PostgresPassword = "postgres";

    public static IContainer Create<T>() where T : IContainer
    {
        var type = typeof(T);
        return type switch
        {
            not null when type.IsAssignableFrom(typeof(PostgreSqlContainer)) => CreatePostgreSql(),
            not null when type.IsAssignableFrom(typeof(KafkaContainer)) => CreateKafka(),
            not null when type.IsAssignableFrom(typeof(RedisContainer)) => CreateRedis(),
            _ => throw new NotSupportedException($"Couldn't create a container of {nameof(T)}")
        };
    }

    private static PostgreSqlContainer CreatePostgreSql() =>
        new PostgreSqlBuilder()
            .WithUsername(PostgresUsername)
            .WithPassword(PostgresPassword)
            .WithDatabase(Database)
            .WithImage("postgres:16-alpine")
            .WithPortBinding(5432, true)
            .WithCleanUp(true)
            .Build();

    private static KafkaContainer CreateKafka() =>
        new KafkaBuilder()
            .WithImage("confluentinc/cp-kafka:7.5.3")
            .WithCleanUp(true)
            .Build();

    private static RedisContainer CreateRedis() =>
        new RedisBuilder()
            .WithImage("redis:7.4-alpine")
            .WithCleanUp(true)
            .Build();
}
