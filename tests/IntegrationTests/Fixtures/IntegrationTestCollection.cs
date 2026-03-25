namespace IntegrationTests.Fixtures;

[CollectionDefinition("Integration")]
public sealed class IntegrationTestCollection : ICollectionFixture<PostgreSqlFixture>
{
}
