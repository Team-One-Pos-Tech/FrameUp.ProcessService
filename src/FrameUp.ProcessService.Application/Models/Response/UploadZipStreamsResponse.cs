namespace FrameUp.ProcessService.Application.Models.Response;

public class UploadZipStreamsResponse
{
    public UploadedStreamResponse[] Items { get; set; } = [];
}

public record UploadedStreamResponse(string FileName, string Uri);