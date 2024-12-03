using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.IO.Compression;

public static class ZipArchiveExtensions
{
    /// <summary>
    /// Creates a new text file in <paramref name="zip"/> and writes the <paramref name="lines"/> into it.
    /// </summary>
    public static async Task CreateTextEntryAsync(this ZipArchive zip, string entryName, IEnumerable<string> lines)
    {
        await using var writer = new StreamWriter(zip.CreateEntry(entryName).Open());

        foreach (var line in lines)
        {
            await writer.WriteLineAsync(line);
        }
    }

    /// <summary>
    /// Creates a new text file in <paramref name="zip"/> and writes the <paramref name="text"/> into it.
    /// </summary>
    public static Task CreateTextEntryAsync(this ZipArchive zip, string entryName, string text) =>
        zip.CreateTextEntryAsync(entryName, [text]);

    /// <summary>
    /// Creates a new binary file in <paramref name="zip"/> and writes the <paramref name="data"/> into it.
    /// </summary>
    public static async Task CreateBinaryEntryAsync(this ZipArchive zip, string entryName, byte[] data)
    {
        await using var stream = zip.CreateEntry(entryName).Open();
        await stream.WriteAsync(data);
    }
}
