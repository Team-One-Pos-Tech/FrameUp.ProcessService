using System;

namespace FrameUp.ProcessService.Application.Contracts;

public interface IDateTimeFactory
{
    DateTime GetCurrentUtcDateTime();
}