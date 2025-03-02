namespace FrameUp.ProcessService.Domain.Enums;

public enum ProcessingStatus
{
    Refused,
    Received,
    Uploading,
    Processing,
    Concluded,
    Canceled,
    Failed
}