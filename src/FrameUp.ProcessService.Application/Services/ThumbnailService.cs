using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FFMpegCore;
using FFMpegCore.Pipes;
using FrameUp.ProcessService.Application.Contracts;
using FrameUp.ProcessService.Application.Models.Requests;
using System.Drawing;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace FrameUp.ProcessService.Application.Services;

public class ThumbnailService : IThumbnailService
{
    private readonly ILogger<ThumbnailService> _logger;
    private readonly IZipFileService _zipFileService;

    public ThumbnailService(IZipFileService zipFileService, ILogger<ThumbnailService> logger)
    {
        _zipFileService = zipFileService;
        _logger = logger;
    }

    public async Task<IDictionary<string, byte[]>> ProcessThumbnailsToAZipStreamAsync(ProcessVideosRequest request, CancellationToken cancellationToken)
    {
        var outputFiles = new Dictionary<string, byte[]>();
        
        var outputImageSize = request.ExportResolution;
        var captureInterval = request.CaptureInterval;
        
        // Unfortunately, could not make FFProbe work with a memory stream!
        // So, I have to write the video file to work on it!
        using var workbench = new WorkBench();
        foreach (var video in request.Videos)
        {
            var videoPath = Path.Combine(workbench.Location, video.Key);
            await File.WriteAllBytesAsync(videoPath, video.Value, cancellationToken);
            
            var snapshots = TakeSnapshotBatchAsync(videoPath, captureInterval, outputImageSize);
            var zipStream = await _zipFileService.ZipFileAsync(snapshots, CancellationToken.None);
            
            outputFiles.Add($"packages/{DateTime.UtcNow:u}_snapshots.zip", zipStream);
        }
        
        return outputFiles;
    }

    private IDictionary<string, byte[]> TakeSnapshotBatchAsync(string inputFile, TimeSpan captureInterval,
        Size resolution)
    {
        var result = new Dictionary<string, byte[]>();
        var videoDetails = FFProbe.Analyse(inputFile);
        var duration = videoDetails.Duration;

        for (var currentTime = TimeSpan.Zero; currentTime < duration; currentTime += captureInterval)
        {
            var snapshot = Snapshot(inputFile, resolution, currentTime, videoDetails);
            if (snapshot.Length > 0)
                result.Add($"frame_at_{currentTime.TotalSeconds}.jpg", snapshot);
        }

        return result;
    }

    private byte[] Snapshot(string input, Size? size, TimeSpan? captureTime, IMediaAnalysis sourceDetails)
    {
        const string videoFormat = "rawvideo";

        var (arguments, outputOptions) = SnapshotArgumentBuilder.BuildSnapshotArguments(
            input, 
            sourceDetails, 
            size, 
            captureTime);
        
        try
        {
            _logger.LogInformation("Processing snapshot for [{fileName}] at the [{captureTime}] seconds!", input, captureTime);
            using var outputStream = new MemoryStream();

            arguments
                .OutputToPipe(new StreamPipeSink(outputStream), options => outputOptions(options
                    .ForceFormat(videoFormat)))
                .ProcessSynchronously();

            return outputStream.ToArray();
        }
        catch (Exception exception)
        {
            _logger.LogError("An error happens when processing a snapshot for snapshot for [{fileName}] at the [{captureTime}] seconds: [{Error}]", 
                input, 
                captureTime, 
                exception.Message);
            
            return [];
        }
    }
}


public class WorkBench : IDisposable
{
    private const string FrameUpTempSpace = "frame_up";
    public string Location { get; }

    public WorkBench()
    {
        var tempPath = Path.GetTempPath();
        Location = Path.Combine(tempPath, FrameUpTempSpace, Guid.NewGuid().ToString());

        Directory.CreateDirectory(Location);
    }

    public void Dispose() => Directory.Delete(Location, true);
}