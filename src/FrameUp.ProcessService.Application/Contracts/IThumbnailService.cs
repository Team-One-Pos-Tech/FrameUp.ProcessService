using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FrameUp.ProcessService.Application.Models.Requests;

namespace FrameUp.ProcessService.Application.Contracts;

public interface IThumbnailService
{
    Task<IDictionary<string, byte[]>> ProcessThumbnailsToAZipStreamAsync(ProcessVideosRequest request, CancellationToken cancellationToken);
}