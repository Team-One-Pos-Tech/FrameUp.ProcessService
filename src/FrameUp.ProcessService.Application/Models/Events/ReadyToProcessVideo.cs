using System;
using FrameUp.ProcessService.Domain.Enums;
using MassTransit;

namespace FrameUp.ProcessService.Application.Models.Events;

[MessageUrn("frameup-order-service")]
[EntityName("ready-to-process-video")]
public record ReadyToProcessVideo(Guid OrderId, ProcessVideoParameters Parameters);

public class ProcessVideoParameters
{
    public ResolutionTypes ExportResolution { get; set; }

    public int? CaptureInterval { get; internal set; }
}