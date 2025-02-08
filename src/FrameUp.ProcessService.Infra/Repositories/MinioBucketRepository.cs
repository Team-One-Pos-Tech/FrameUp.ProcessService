using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FrameUp.ProcessService.Application.Contracts;
using FrameUp.ProcessService.Application.Models.Requests;
using FrameUp.ProcessService.Application.Models.Response;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;
using Minio.DataModel.Tags;

namespace FrameUp.ProcessService.Infra.Repositories;

public class MinioBucketRepository : IFileBucketRepository
{
    private const string BucketName = "frameup.videos";

    private readonly ILogger<MinioBucketRepository> _logger;
    private readonly IMinioClient _minioClient;

    public MinioBucketRepository(IMinioClient minioClient, ILogger<MinioBucketRepository> logger)
    {
        _minioClient = minioClient;
        _logger = logger;
    }

    public async Task<Dictionary<string, string>> UploadAsync(UploadFileRequest request)
    {
        await CreateBucketIfNotExistsAsync();

        var tagging = CreateTagging(request.OrderId.ToString());

        var uploadTasks = request
            .FileDetails
            .Select(file => UploadFile(request.OrderId, file, tagging));

        var response = new Dictionary<string, string>();

        try
        {
            var itens = await Task.WhenAll(uploadTasks);
            
            itens.ToDictionary(item => item.fileName, item => item.uri)
                .ToList()
                .ForEach(item => response.Add(item.Key, item.Value));

            return response;
        }
        catch (Exception e)
        {
            throw new Exception("Error uploading files", e);
        }
    }

    public async Task<DownloadFileResponse> DownloadFilesByOrderAndNameAsync(Guid orderId, IEnumerable<string> objectNames)
    {
        var requestStreams = new Dictionary<string, MemoryStream>();
        var objectStats = new List<ObjectStat>();
        
        foreach (var fileName in objectNames)
        {
            requestStreams[fileName] = new MemoryStream();

            var getObjectRequest = new GetObjectArgs()
                .WithBucket(BucketName)
                .WithObject($"{orderId}/{fileName}")
                .WithCallbackStream(stream => stream.CopyTo(requestStreams[fileName]));
            
            var stats = await _minioClient.GetObjectAsync(getObjectRequest);
            objectStats.Add(stats);
        }
        
        var response = new DownloadFileResponse();

        foreach (var element in objectStats)
        {
            var nameWithoutPrefix = element.ObjectName.Remove(0, element.ObjectName.IndexOf('/') + 1);
            if (string.IsNullOrEmpty(nameWithoutPrefix))
                nameWithoutPrefix = Guid.NewGuid().ToString();
            
            response.FileDetails.Add(new FileDetailsRequest
            {
                ContentStream = requestStreams[nameWithoutPrefix].ToArray(),
                ContentType = element.ContentType,
                Name = nameWithoutPrefix,
                Size = element.Size,
            });
        }
        
        return response;
    }

    public async Task<DownloadFileResponse> DownloadAllFilesByOrderIdAsync(Guid orderId)
    {
        var objectListAsyncEnumerable = ListAllObjectsAtBucketWithPrefix(orderId.ToString());
        return await DownloadObjectList(objectListAsyncEnumerable);
    }

    private async Task<DownloadFileResponse> DownloadObjectList(IAsyncEnumerable<Item> objectListAsyncEnumerable)
    {
        var requestStreams = new Dictionary<string, MemoryStream>();
        var objectStats = new List<ObjectStat>();

        await foreach (var element in objectListAsyncEnumerable)
        {
            if (element.IsDir)
                continue;

            if(element.Key.EndsWith(".zip"))
                continue;
            
            _logger.LogInformation("Preparing to download file {fileName} from bucket {bucket}", element.Key, BucketName);
            
            requestStreams[element.Key] = new MemoryStream();

            var getObjectRequest = new GetObjectArgs()
                .WithBucket(BucketName)
                .WithObject(element.Key)
                .WithCallbackStream(stream => stream.CopyTo(requestStreams[element.Key]));

            var stats = await _minioClient.GetObjectAsync(getObjectRequest);
            objectStats.Add(stats);
        }
        
        var response = new DownloadFileResponse();

        foreach (var element in objectStats)
        {
            var nameWithoutPrefix = element.ObjectName.Remove(0, element.ObjectName.IndexOf('/') + 1);
            if (string.IsNullOrEmpty(nameWithoutPrefix))
                nameWithoutPrefix = Guid.NewGuid().ToString();
            
            response.FileDetails.Add(new FileDetailsRequest
            {
                ContentStream = requestStreams[element.ObjectName].ToArray(),
                ContentType = element.ContentType,
                Name = nameWithoutPrefix,
                Size = element.Size,
            });
        }
        
        return response;
    }

    private IAsyncEnumerable<Item> ListAllObjectsAtBucketWithPrefix(string orderBucketPrefix)
    {
        var bucketList = new ListObjectsArgs()
            .WithBucket(BucketName)
            .WithPrefix(orderBucketPrefix)
            .WithRecursive(true);

        return _minioClient.ListObjectsEnumAsync(bucketList);
    }

    private async Task<(string fileName, string uri)> UploadFile(Guid orderId, FileDetailsRequest file, Tagging tagging)
    {
        _logger.LogInformation("Uploading file {fileName} for Order id [{orderId}]", file.Name, orderId);

        var objectName = $"{orderId}/{file.Name}";

        var args = new PutObjectArgs()
            .WithBucket(BucketName)
            .WithTagging(tagging)
            .WithObject(objectName)
            .WithStreamData(new MemoryStream(file.ContentStream))
            .WithObjectSize(file.ContentStream.Length)
            .WithContentType(file.ContentType);

        await _minioClient.PutObjectAsync(args);

        var uri = await _minioClient.PresignedGetObjectAsync(
            new PresignedGetObjectArgs()
            .WithBucket(BucketName)
            .WithObject(objectName)
            .WithExpiry(604800) // Set expiry to 7 days (604800s)
            );

        return new(file.Name, uri);
    }

    private static Tagging CreateTagging(string orderId)
    {
        return new Tagging(new Dictionary<string, string>()
            {
                {
                    "orderId", orderId
                }
            },
            false);
    }

    private async Task CreateBucketIfNotExistsAsync()
    {
        if (await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(BucketName)))
            return;

        await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(BucketName));
    }
}