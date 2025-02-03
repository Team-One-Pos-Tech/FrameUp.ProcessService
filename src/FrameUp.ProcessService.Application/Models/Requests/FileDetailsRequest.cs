namespace FrameUp.ProcessService.Application.Models.Requests;

public record FileDetailsRequest
{
    public required byte[] ContentStream { get; init; }
    public required string Name { get; set; }
    public required string ContentType { get; set; }
    public long Size { get; set; }
}