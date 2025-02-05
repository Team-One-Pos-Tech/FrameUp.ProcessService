using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Minio;

namespace FrameUp.ProcessService.Behavior.Tests.Fixtures;

public abstract class MinIOFixture
{
    private IContainer _minioContainer;
    protected IMinioClient MinioClient;
    
    [SetUp]
    protected async Task BaseSetUp()
    {
        const string userName = "test-user";
        const string password = "#$Sup3rP4ss123";
        const int minioServerPublicPort = 9000;
        
        _minioContainer = new ContainerBuilder()
            .WithImage("quay.io/minio/minio")
            .WithPortBinding(minioServerPublicPort, true)
            .WithCommand("server", "/data")
            .WithEnvironment("MINIO_ROOT_USER", userName)
            .WithEnvironment("MINIO_ROOT_PASSWORD", password)
            .WithCleanUp(true)
            .WithWaitStrategy(Wait
                .ForUnixContainer()
                .UntilPortIsAvailable(minioServerPublicPort))
            .Build();
        
        await _minioContainer.StartAsync();

        var minioServerEndpoint = $"{_minioContainer.Hostname}:{_minioContainer.GetMappedPublicPort(minioServerPublicPort)}";
        MinioClient = new MinioClient()
            .WithEndpoint(minioServerEndpoint)
            .WithCredentials(userName, password)
            .Build();
    }

    [TearDown]
    protected async Task BaseTearDown()
    {
        await _minioContainer.StopAsync();
        await _minioContainer.DisposeAsync();
        
        MinioClient?.Dispose();
    }
}