using System;
using FrameUp.ProcessService.Domain.Enums;
using MassTransit;

namespace FrameUp.ProcessService.Application.Models.Events;

[MessageUrn("frameup-order-service")]
[EntityName("update-order-status")]
public record UpdateOrderStatusEvent(Guid OrderId, ProcessingStatus Status);
