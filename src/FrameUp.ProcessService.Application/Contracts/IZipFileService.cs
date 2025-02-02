using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FrameUp.ProcessService.Application.Contracts;

public interface IZipFileService
{
    Task<byte[]> ZipFileAsync(IDictionary<string, byte[]> streamDictionary, CancellationToken cancellationToken = default);
}