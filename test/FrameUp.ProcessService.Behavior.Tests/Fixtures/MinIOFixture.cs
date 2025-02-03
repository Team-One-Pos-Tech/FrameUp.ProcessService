using System.Threading.Tasks;
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
        
        _minioContainer = new MinioBuilder()
            .WithUsername(userName)
            .WithPassword(password)
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