using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FrameUp.ProcessService.Application.Contracts;
using FrameUp.ProcessService.Application.Models.Requests;
using FrameUp.ProcessService.Application.Models.Response;
using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;
using Minio.DataModel.Tags;

namespace FrameUp.ProcessService.Infra.Repositories;

public class MinioBucketRepository : IFileBucketRepository
{
    private const string BucketName = "frameup.videos";

    private readonly IMinioClient _minioClient;

    public MinioBucketRepository(IMinioClient minioClient)
    {
        _minioClient = minioClient;
    }

    public async Task UploadAsync(UploadFileRequest request)
    {
        await CreateBucketIfNotExistsAsync();

        var tagging = CreateTagging(request.OrderId.ToString());

        var uploadTasks = request
            .FileDetails
            .Select(file => UploadFile(request.OrderId, file, tagging));

        try
        {
            await Task.WhenAll(uploadTasks);
        }
        catch (Exception e)
        {
            throw new Exception("Error uploading files", e);
        }
    }

    public async Task<DownloadFileResponse> DownloadAsync(DownloadFileRequest request)
    {
        // Todo: Review and test it! It is too blotted right now!
        var bucketList = new ListObjectsArgs().WithBucket(BucketName);
        var objects = _minioClient.ListObjectsEnumAsync(bucketList);

        var requestTasks = new List<Task<ObjectStat>>();
        var requestStreams = new Dictionary<string, MemoryStream>();
        
        await foreach (var element in objects)
        {
            if(element.IsDir)
                continue;
            
            requestStreams[element.Key] = new MemoryStream();
            
            var getObjectRequest = new GetObjectArgs()
                .WithBucket(BucketName)
                .WithFile(element.Key)
                .WithCallbackStream(stream => stream.CopyTo(requestStreams[element.Key]));
                
            requestTasks.Add(_minioClient.GetObjectAsync(getObjectRequest));
        }
        
        var elements = await Task.WhenAll(requestTasks);
        var response = new DownloadFileResponse();
            
        foreach (var element in elements)
        {
            await requestStreams[element.ObjectName].FlushAsync();
            
            response.FileDetails.Add(new FileDetailsRequest
            {
                ContentStream = requestStreams[element.ObjectName].ToArray(),
                ContentType = element.ContentType,
                Name = element.ObjectName,
                Size = element.Size,
            });
        }
        
        return response;
    }

    private async Task UploadFile(Guid orderId, FileDetailsRequest file, Tagging tagging)
    {
        var objectName = $"{orderId}/{file.Name}";

        var args = new PutObjectArgs()
            .WithBucket(BucketName)
            .WithTagging(tagging)
            .WithObject(objectName)
            .WithStreamData(new MemoryStream(file.ContentStream))
            .WithObjectSize(file.ContentStream.Length)
            .WithContentType(file.ContentType);

        await _minioClient.PutObjectAsync(args);
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