using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using FrameUp.ProcessService.Application.Contracts;
using FrameUp.ProcessService.Application.Models.Events;
using FrameUp.ProcessService.Application.Models.Requests;
using FrameUp.ProcessService.Domain.Enums;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FrameUp.ProcessService.Application.EventConsumers;

public class VideoReadyToProcessConsumer : IConsumer<ReadyToProcessVideo>
{
    private static readonly IDictionary<ResolutionTypes, Size> Resolutions = new Dictionary<ResolutionTypes, Size>
    {
        { ResolutionTypes.QuadHD, new Size(2560, 1440) },
        { ResolutionTypes.FullHD, new Size(1920, 1080) },
        { ResolutionTypes.HD, new Size(1280, 720) },
        { ResolutionTypes.SD, new Size(640, 480) },
    };

    private readonly ILogger<VideoReadyToProcessConsumer> _logger;
    
    private readonly IFileBucketRepository _fileBucketRepository;
    private readonly IThumbnailService _thumbnailService;
    
    public async Task Consume(ConsumeContext<ReadyToProcessVideo> context)
    {
        var streamsToProcess = await ListVideoStreamsToProcess(context.Message.OrderId);
        var zipFiles = await ProcessStreamsIntoZipStreams(context.Message.Parameters, streamsToProcess, context.CancellationToken);
        await UploadZipStreams(context.Message.OrderId, zipFiles);
    }

    private async Task UploadZipStreams(Guid orderId, IDictionary<string, byte[]> zipFiles)
    {
        ConsumeContext<ReadyToProcessVideo> context;
        var filesToUpload = zipFiles
            .Select(zipFile => new FileDetailsRequest
            {
                Name = zipFile.Key, ContentStream = zipFile.Value, 
                ContentType = MediaTypeNames.Application.Zip,
                Size = zipFile.Value.Length,
            });

        var uploadRequest = new UploadFileRequest
        {
            OrderId = orderId,
            FileDetails = filesToUpload
        };

        await _fileBucketRepository.UploadAsync(uploadRequest);
    }

    private async Task<IDictionary<string, byte[]>> ProcessStreamsIntoZipStreams(ProcessVideoParameters parameters, Dictionary<string, byte[]> streamsToProcess, CancellationToken cancellationToken)
    {
        var processRequest = new ProcessVideosRequest
        {
            CaptureInterval = TimeSpan.FromSeconds((long)parameters.CaptureInterval!),
            ExportResolution = Resolutions[parameters.ExportResolution],
            Videos = streamsToProcess
        };

        var zipFiles = await _thumbnailService.ProcessThumbnailsToAZipStreamAsync(processRequest, cancellationToken);
        return zipFiles;
    }

    private async Task<Dictionary<string, byte[]>> ListVideoStreamsToProcess(Guid orderid)
    {
        try
        {
            _logger.LogInformation("Preparing to Download video files from Bucket. Order Id [{orderId}]", orderid);
            var filesToProcess = await _fileBucketRepository.DownloadAsync(new DownloadFileRequest
            {
                OrderId = orderid
            });

            var videos = filesToProcess
                .FileDetails
                .ToDictionary(fileDetail => fileDetail.Name, fileDetail => fileDetail.ContentStream);
            
            _logger.LogInformation("A total of [{quantity}] videos were downloaded to order id [{orderId}]", videos.Count, orderid);
            
            return videos;
        }
        catch (Exception exception)
        {
            _logger.LogError("An exception happens when downloading video files from Bucket for order Id [{orderId}]: [{exception}]", orderid, exception.InnerException);
            throw;
        }
    }
}