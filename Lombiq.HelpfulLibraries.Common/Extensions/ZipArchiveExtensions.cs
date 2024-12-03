using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.IO.Compression;

public static class ZipArchiveExtensions
{
    /// <summary>
    /// Creates a new text file in <paramref name="zip"/> and writes the <paramref name="lines"/> into it.
    /// </summary>
    public static async Task CreateTextEntryAsync(ZipArchive zip, string entryName, IEnumerable<string> lines)
    {
        await using var writer = new StreamWriter(zip.CreateEntry(entryName).Open());

        foreach (var line in lines)
        {
            await writer.WriteLineAsync(line);
        }
    }

    /// <summary>
    /// Creates a new binary file in <paramref name="zip"/> and writes the <paramref name="data"/> into it.
    /// </summary>
    public static async Task CreateBinaryEntryAsync(ZipArchive zip, string entryName, byte[] data)
    {
        await using var stream = zip.CreateEntry(entryName).Open();
        await stream.WriteAsync(data);
    }
}
