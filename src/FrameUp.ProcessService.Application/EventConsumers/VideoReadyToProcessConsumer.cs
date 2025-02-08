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
using FrameUp.ProcessService.Application.Models.Response;
using FrameUp.ProcessService.Domain.Enums;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FrameUp.ProcessService.Application.EventConsumers;

public class VideoReadyToProcessConsumer(
    ILogger<VideoReadyToProcessConsumer> logger,
    IFileBucketRepository fileBucketRepository,
    IThumbnailService thumbnailService,
    IPublishEndpoint publishEndpoint) : IConsumer<ReadyToProcessVideo>
{
    private static readonly IDictionary<ResolutionTypes, Size> Resolutions = new Dictionary<ResolutionTypes, Size>
    {
        { ResolutionTypes.QuadHD, new Size(2560, 1440) },
        { ResolutionTypes.FullHD, new Size(1920, 1080) },
        { ResolutionTypes.HD, new Size(1280, 720) },
        { ResolutionTypes.SD, new Size(640, 480) },
    };

    public async Task Consume(ConsumeContext<ReadyToProcessVideo> context)
    {
        if (context.Message.Parameters.CaptureInterval <= 0)
        {
            logger.LogInformation("Capture interval for order to process [{orderId}] is invalid!", context.Message.OrderId);
            return;
        }

        var streamsToProcess = await ListVideoStreamsToProcess(context.Message.OrderId);
        if (streamsToProcess.Count <= 0)
        {
            logger.LogInformation("There are no video streams to process for order with id [{orderId}]", context.Message.OrderId);
            return;
        }

        await NotifyProcessingOrderAsync(context.Message.OrderId, ProcessingStatus.Processing);

        try
        {
            var zipFiles = await ProcessStreamsIntoZipStreams(context.Message.Parameters, streamsToProcess, context.CancellationToken);
            var uploadedStreamResponse = await UploadZipStreams(context.Message.OrderId, zipFiles);

            await NotifyProcessingOrderAsync(context.Message.OrderId, ProcessingStatus.Concluded, uploadedStreamResponse);

            logger.LogInformation("Order [{orderId}] has been processed successfully", context.Message.OrderId);
        }
        catch (Exception exception)
        {
            logger.LogError("An error occurred when processingg the order [{orderid}]: [{message}]", context.Message.OrderId, exception.Message);
            await NotifyProcessingOrderAsync(context.Message.OrderId, ProcessingStatus.Failed);
        }
    }

    private async Task NotifyProcessingOrderAsync(
        Guid orderId, 
        ProcessingStatus status, 
        UploadZipStreamsResponse? uploadedStreamResponse = null)
    {
        var uploadedPackagesResponseItems = uploadedStreamResponse != null ?
                uploadedStreamResponse
                .Items
                .Select(item => new UploadedPackageItemResponse(item.FileName, item.Uri)).ToArray() : [];

        var processingEvent = new UpdateOrderStatusEvent(orderId, status, uploadedPackagesResponseItems);

        await publishEndpoint.Publish(processingEvent);
    }

    private async Task<UploadZipStreamsResponse> UploadZipStreams(Guid orderId, IDictionary<string, byte[]> zipFiles)
    {
        var filesToUpload = zipFiles
            .Select(zipFile => new FileDetailsRequest
            {
                Name = zipFile.Key,
                ContentStream = zipFile.Value,
                ContentType = MediaTypeNames.Application.Zip,
                Size = zipFile.Value.Length,
            });

        var uploadRequest = new UploadFileRequest
        {
            OrderId = orderId,
            FileDetails = filesToUpload
        };

        var response = await fileBucketRepository.UploadAsync(uploadRequest);

        return new UploadZipStreamsResponse
        {
            Items = response.Select(fileDetail => new UploadedStreamResponse(fileDetail.Key, fileDetail.Value)).ToArray()
        };
    }

    private async Task<IDictionary<string, byte[]>> ProcessStreamsIntoZipStreams(ProcessVideoParameters parameters, Dictionary<string, byte[]> streamsToProcess, CancellationToken cancellationToken)
    {
        var processRequest = new ProcessVideosRequest
        {
            CaptureInterval = TimeSpan.FromSeconds((long)parameters.CaptureInterval!),
            ExportResolution = Resolutions[parameters.ExportResolution],
            Videos = streamsToProcess
        };

        var zipFiles = await thumbnailService.ProcessThumbnailsToAZipStreamAsync(processRequest, cancellationToken);
        return zipFiles;
    }

    private async Task<Dictionary<string, byte[]>> ListVideoStreamsToProcess(Guid orderid)
    {
        try
        {
            logger.LogInformation("Preparing to Download video files from Bucket. Order Id [{orderId}]", orderid);
            var filesToProcess = await fileBucketRepository.DownloadAllFilesByOrderIdAsync(orderid);

            var videos = filesToProcess
                .FileDetails
                .ToDictionary(fileDetail => fileDetail.Name, fileDetail => fileDetail.ContentStream);

            logger.LogInformation("A total of [{quantity}] videos were downloaded to order id [{orderId}]", videos.Count, orderid);

            return videos;
        }
        catch (Exception exception)
        {
            logger.LogError("An exception happens when downloading video files from Bucket for order Id [{orderId}]: [{exception}]", orderid, exception.InnerException);
            throw;
        }
    }
}