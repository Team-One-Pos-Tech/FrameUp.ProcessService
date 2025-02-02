using System;
using System.Collections.Generic;

namespace FrameUp.ProcessService.Application.Models.Requests;

public record DownloadFileRequest
{
    public Guid OrderId { get; set; }

    public string BucketName { get; set; }
    
    // Todo: Check this at the future!
    public IEnumerable<string> FileNames { get; set; } = ArraySegment<string>.Empty;
}