using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FrameUp.ProcessService.Application.Models.Requests;
using FrameUp.ProcessService.Application.Models.Response;

namespace FrameUp.ProcessService.Application.Contracts;

public interface IFileBucketRepository
{
    Task UploadAsync(UploadFileRequest request);
    
    Task<DownloadFileResponse> DownloadFilesByOrderAndNameAsync(Guid orderId, IEnumerable<string> objectNames);
    Task<DownloadFileResponse> DownloadAllFilesByOrderIdAsync(Guid orderId);
}