using System;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using FrameUp.ProcessService.Application.Contracts;
using FrameUp.ProcessService.Application.Models.Requests;
using FrameUp.ProcessService.Behavior.Tests.Fixtures;
using FrameUp.ProcessService.Infra.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Reqnroll;

namespace FrameUp.ProcessService.Behavior.Tests.StepDefinitions;

[Binding]
public class VideoReadyToProcessConsumerStepDefinition : MinIOFixture
{
    private IFileBucketRepository _bucketRepository;
    
    [BeforeScenario]
    public async Task Setup()
    {
       await  BaseSetUp();
       
       var loggerFactory = new NullLoggerFactory();
       _bucketRepository = new MinioBucketRepository(MinioClient, loggerFactory.CreateLogger<MinioBucketRepository>());
       
    }

    [Given("a valid bucket for order id '(.*)' with a video stored")]
    public async Task GivenAValidBucketForOrderIdWithAVideoStored(Guid orderId)
    {
        var fileInfo = new FileInfo("./Assets/Marvel_DOTNET_CSHARP.mp4");
        await using var stream = fileInfo.OpenRead();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        
        var uploadRequest = new UploadFileRequest
        {
            OrderId = orderId,
            FileDetails = new []
            {
                new FileDetailsRequest
                {
                    Name = fileInfo.Name,
                    Size = fileInfo.Length,
                    ContentType = "video/mp4",
                    ContentStream = memoryStream.ToArray()
                }
            }
        };

        await _bucketRepository.UploadAsync(uploadRequest);
        
        Console.WriteLine("Waiting!");
    }
}