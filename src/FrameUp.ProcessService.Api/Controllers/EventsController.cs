using FrameUp.ProcessService.Application.Models.Events;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace FrameUp.ProcessService.Api.Controllers;

[Route("[controller]")]
[ApiController]
public class EventsController(IPublishEndpoint publishEndpoint) : ControllerBase
{
    [HttpPost("Process/{orderId}")]
    public async Task Post(Guid orderId, ProcessVideoParameters parameters)
    {
        await publishEndpoint.Publish(new ReadyToProcessVideo(orderId, parameters));
    }
}
