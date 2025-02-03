using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using Minio;
using Testcontainers.Minio;

namespace FrameUp.ProcessService.Behavior.Tests.Fixtures;

public abstract class MinIOFixture
{
    protected IMinioClient MinioClient;
    private MinioContainer _minioContainer;
    
    [SetUp]
    protected async Task BaseSetUp()
    {
        const string userName = "test-user";
        const string password = "#$Sup3rP4ss123";
        const int minioPublicPort = 9000;
        
        _minioContainer = new MinioBuilder()
            .WithImage("quay.io/minio/minio")
            .WithUsername(userName)
            .WithPassword(password)
            .WithCleanUp(true)
            .WithWaitStrategy(Wait
                .ForUnixContainer()
                .UntilPortIsAvailable(minioPublicPort))
            .Build();

        await _minioContainer.StartAsync();
        MinioClient = new MinioClient()
            .WithEndpoint(_minioContainer.GetConnectionString())
            .WithCredentials(_minioContainer.GetAccessKey(), _minioContainer.GetSecretKey())
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