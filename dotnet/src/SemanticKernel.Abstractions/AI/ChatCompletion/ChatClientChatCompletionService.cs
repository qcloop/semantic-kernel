﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Services;

namespace Microsoft.SemanticKernel.ChatCompletion;

/// <summary>Provides an implementation of <see cref="IChatCompletionService"/> around an <see cref="IChatClient"/>.</summary>
internal sealed class ChatClientChatCompletionService : IChatCompletionService
{
    /// <summary>The wrapped <see cref="IChatClient"/>.</summary>
    private readonly IChatClient _chatClient;

    /// <summary>Initializes the <see cref="ChatClientChatCompletionService"/> for <paramref name="chatClient"/>.</summary>
    internal ChatClientChatCompletionService(IChatClient chatClient, IServiceProvider? serviceProvider)
    {
        Verify.NotNull(chatClient);

        // Store the client.
        this._chatClient = chatClient;

        // Initialize the attributes.
        var attrs = new Dictionary<string, object?>();
        this.Attributes = new ReadOnlyDictionary<string, object?>(attrs);

        var metadata = chatClient.GetService<ChatClientMetadata>();
        if (metadata?.ProviderUri is not null)
        {
            attrs[AIServiceExtensions.EndpointKey] = metadata.ProviderUri.ToString();
        }
        if (metadata?.ModelId is not null)
        {
            attrs[AIServiceExtensions.ModelIdKey] = metadata.ModelId;
        }
    }

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, object?> Attributes { get; }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
        ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
    {
        Verify.NotNull(chatHistory);

        var messageList = chatHistory.ToChatMessageList();
        var currentSize = messageList.Count;

        var completion = await this._chatClient.GetResponseAsync(
            messageList,
            executionSettings.ToChatOptions(kernel),
            cancellationToken).ConfigureAwait(false);

        chatHistory.AddRange(
            messageList
                .Skip(currentSize)
                .Select(m => m.ToChatMessageContent()));

        return completion.Choices.Select(m => m.ToChatMessageContent(completion)).ToList();
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
        ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        Verify.NotNull(chatHistory);

        await foreach (var update in this._chatClient.GetStreamingResponseAsync(
            chatHistory.ToChatMessageList(),
            executionSettings.ToChatOptions(kernel),
            cancellationToken).ConfigureAwait(false))
        {
            yield return update.ToStreamingChatMessageContent();
        }
    }
}
