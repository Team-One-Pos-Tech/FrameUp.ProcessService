using System;
using System.Collections.Generic;
using System.Drawing;

namespace FrameUp.ProcessService.Application.Models.Requests;

public record ProcessVideosRequest
{
    public IDictionary<string, byte[]> Videos { get; init; } = new Dictionary<string, byte[]>();
    public required Size ExportResolution { get; init; }
    public required TimeSpan CaptureInterval { get; init; }
}