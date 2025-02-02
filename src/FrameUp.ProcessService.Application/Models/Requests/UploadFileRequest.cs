using System;
using System.Collections.Generic;

namespace FrameUp.ProcessService.Application.Models.Requests;

public record UploadFileRequest
{
    public Guid OrderId { get; set; }
    public IEnumerable<FileDetailsRequest> FileDetails { get; set; } = ArraySegment<FileDetailsRequest>.Empty;
}