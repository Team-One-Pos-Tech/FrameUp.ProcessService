using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using FrameUp.ProcessService.Infra.Context;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace FrameUp.ProcessService.Behavior.Tests.Fixtures;

public abstract class PostgreSqlFixture
{
    private const int PostgresPublicPort = 5432;
    private const string PostgresRootPassword = "postgres";

    private PostgreSqlContainer _postgresContainer;

    protected ProcessServiceDbContext ProcessServiceDbContext;
    
    [SetUp]
    protected async Task BaseSetUp()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithDatabase("FrameUpIntegrationTests")
            .WithUsername("postgres")
            .WithPassword(PostgresRootPassword)
            .WithPortBinding(PostgresPublicPort, true)
            .WithCleanUp(true)
            .WithWaitStrategy(Wait
                .ForUnixContainer()
                .UntilPortIsAvailable(PostgresPublicPort))
            .Build();

        await _postgresContainer.StartAsync();
        
        var dbContextOptions = new DbContextOptionsBuilder<ProcessServiceDbContext>()
            .UseNpgsql(_postgresContainer.GetConnectionString())
            .Options;

        ProcessServiceDbContext = new ProcessServiceDbContext(dbContextOptions);
        await ProcessServiceDbContext.Database.EnsureCreatedAsync();
        
    }

    [TearDown]
    protected async Task BaseTearDown()
    {
        await _postgresContainer.StopAsync();
        await _postgresContainer.DisposeAsync();
    }
}