﻿using Refit;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Lombiq.HelpfulLibraries.Refit.Models;

public class SimpleTextResponse
{
    public string Content { get; }
    public IReadOnlyDictionary<string, string> Headers { get; }
    public bool IsOk { get; }

    public string Location => Headers.TryGetValue(nameof(Location), out var value) ? value : null;

    public SimpleTextResponse(IApiResponse<string> response)
    {
        Content = response.Content;
        Headers = response.Headers.ToDictionary(header => header.Key, header => header.Value.First());
        IsOk = response.StatusCode == HttpStatusCode.OK;
    }

    public static SimpleTextResponse ConvertAndDisposeApiResponse(ApiResponse<string> response)
    {
        if (response == null) return null;

        using (response)
        {
            return new SimpleTextResponse(response);
        }
    }
}