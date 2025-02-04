using System;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using FrameUp.ProcessService.Application.Contracts;
using FrameUp.ProcessService.Application.EventConsumers;
using FrameUp.ProcessService.Application.Models.Events;
using FrameUp.ProcessService.Application.Models.Requests;
using FrameUp.ProcessService.Application.Services;
using FrameUp.ProcessService.Behavior.Tests.Fixtures;
using FrameUp.ProcessService.Domain.Enums;
using FrameUp.ProcessService.Infra.Repositories;
using MassTransit;
using MassTransit.Context;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Reqnroll;

namespace FrameUp.ProcessService.Behavior.Tests.StepDefinitions;

[Binding]
public class VideoReadyToProcessConsumerStepDefinition : MinIOFixture
{
    private IFileBucketRepository _bucketRepository;
    private VideoReadyToProcessConsumer _videoReadyToProcessConsumer;

    [BeforeScenario]
    public async Task Setup()
    {
        await BaseSetUp();

        var loggerFactory = new NullLoggerFactory();
        _bucketRepository = new MinioBucketRepository(MinioClient, loggerFactory.CreateLogger<MinioBucketRepository>());

        var zipService = new ZipFileService();
        var thumbnailService = new ThumbnailService(zipService, loggerFactory.CreateLogger<ThumbnailService>());
        _videoReadyToProcessConsumer = new VideoReadyToProcessConsumer(
            loggerFactory.CreateLogger<VideoReadyToProcessConsumer>(), _bucketRepository, thumbnailService);
    }

    [Given("a valid bucket for order id '(.*)' with a video stored")]
    public async Task GivenAValidBucketForOrderIdWithAVideoStored(Guid orderId)
    {
        var fileInfo = new FileInfo("./Assets/Video.mp4");
        await using var stream = fileInfo.OpenRead();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);

        var uploadRequest = new UploadFileRequest
        {
            OrderId = orderId,
            FileDetails =
            [
                new FileDetailsRequest
                {
                    Name = fileInfo.Name,
                    Size = fileInfo.Length,
                    ContentType = "video/mp4",
                    ContentStream = memoryStream.ToArray()
                }
            ]
        };

        await _bucketRepository.UploadAsync(uploadRequest);
    }

    [When(@"a ReadyToProcessVideo event is risen for order id '(.*)'")]
    public async Task WhenAReadyToProcessVideoEventIsRisenForOrderId(Guid orderId)
    {
        var readyToProcessVideoEvent = new ReadyToProcessVideo(orderId, new ProcessVideoParameters
        {
            CaptureInterval = 10,
            ExportResolution = ResolutionTypes.FullHD
        });

        var context = Mock.Of<ConsumeContext<ReadyToProcessVideo>>(_ => _.Message == readyToProcessVideoEvent);
        await _videoReadyToProcessConsumer.Consume(context);
    }

    [Then(@"it should process the video successfully")]
    public void ThenItShouldProcessTheVideoSuccessfully()
    {
    }
}