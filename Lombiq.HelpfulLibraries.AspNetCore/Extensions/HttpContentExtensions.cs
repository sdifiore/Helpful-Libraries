using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;

namespace System.Net.Http;

public static class HttpContentExtensions
{
    /// <summary>
    /// Attaches a new file field to this web request content.
    /// </summary>
    /// <param name="form">The form content of the request.</param>
    /// <param name="name">The name of the field.</param>
    /// <param name="fileName">The name and extension of the file being uploaded.</param>
    /// <param name="mediaType">The file's MIME type (use <see cref="MediaTypeNames"/>).</param>
    /// <param name="content">The content of the file.</param>
    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "The parent form should be disposed instead.")]
    public static void AddFile(
        this MultipartFormDataContent form,
        string name,
        string fileName,
        string mediaType,
        byte[] content)
    {
        var xml = new ByteArrayContent(content);
        xml.Headers.ContentType = MediaTypeHeaderValue.Parse(mediaType);
        form.Add(xml, name, fileName);
    }

    /// <inheritdoc cref="AddFile(System.Net.Http.MultipartFormDataContent,string,string,string,byte[])"/>
    /// <param name="content">The content of the file. It will be encoded as UTF-8.</param>
    public static void AddFile(
        this MultipartFormDataContent form,
        string name,
        string fileName,
        string mediaType,
        string content) =>
        form.AddFile(name, fileName, mediaType, Encoding.UTF8.GetBytes(content));
}
