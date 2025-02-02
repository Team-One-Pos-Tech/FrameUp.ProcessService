using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using FrameUp.ProcessService.Application.Contracts;

namespace FrameUp.ProcessService.Application.Services;

public class ZipFileService : IZipFileService
{
    public async Task<byte[]> ZipFileAsync(IDictionary<string, byte[]> streamDictionary,
        CancellationToken cancellationToken = default)
    {
        using var zipStream = new MemoryStream();
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, false)) // Needs to be disposed before return the byte[]
        {
            foreach (var stream in streamDictionary)
            {
                var entry = archive.CreateEntry(stream.Key, CompressionLevel.Optimal);
                await using var entryStream = entry.Open();
                await entryStream.WriteAsync(stream.Value.AsMemory(0, stream.Value.Length), cancellationToken);
            }
        }
        
        return zipStream.ToArray();
    }
}