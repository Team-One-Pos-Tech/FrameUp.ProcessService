using System.Collections.Generic;
using FrameUp.ProcessService.Application.Models.Requests;

namespace FrameUp.ProcessService.Application.Models.Response;

public record DownloadFileResponse
{
    public IList<FileDetailsRequest> FileDetails { get; set; } = [];
}