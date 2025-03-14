﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;

namespace ChatWithAgent.ApiService;

/// <summary>
/// Data model for storing a section of text with an embedding and an optional reference link.
/// </summary>
/// <typeparam name="TKey">The type of the data model key.</typeparam>
internal sealed class TextSnippet<TKey>
{
    [VectorStoreRecordKey]
    public required TKey Key { get; set; }

    [VectorStoreRecordData]
    [TextSearchResultValue]
    public string? Text { get; set; }

    [VectorStoreRecordData]
    [TextSearchResultName]
    public string? ReferenceDescription { get; set; }

    [VectorStoreRecordVector(1536)]
    public ReadOnlyMemory<float> TextEmbedding { get; set; }
}
