using System;
using FrameUp.ProcessService.Application.Contracts;

namespace FrameUp.ProcessService.Application.Services;

public class DefaultDateTimeFactory : IDateTimeFactory
{
    public DateTime GetCurrentUtcDateTime() => DateTime.UtcNow;
}