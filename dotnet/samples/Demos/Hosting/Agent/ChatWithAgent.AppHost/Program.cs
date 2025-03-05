﻿// Copyright (c) Microsoft. All rights reserved.

using ChatWithAgent.AppHost.Extensions;
using ChatWithAgent.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

// Load host configuration.
var hostConfig = new HostConfig(builder.Configuration);

// Deploy and provision AI Service.
var aiService = AddAIServices(builder, hostConfig);

// Deploy and provision Api Service with dependencies.
var apiService = builder.AddProject<Projects.ChatWithAgent_ApiService>("apiservice")
    .WithReference(aiService)
    .WithEnvironment(hostConfig); // Add some host configuration as environment variables so that the Api Service can access them

// Deploy and provision Web Frontend
builder.AddProject<Projects.ChatWithAgent_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();

static IResourceBuilder<IResourceWithConnectionString> AddAIServices(IDistributedApplicationBuilder builder, HostConfig config)
{
    switch (config.AIChatService)
    {
        case AzureOpenAIChatConfig.ConfigSectionName:
        {
            return builder.AddAzureOpenAI(config);
        }

        case OpenAIChatConfig.ConfigSectionName:
        {
            // Use an existing Azure OpenAI service via connection string
            return builder.AddConnectionString(OpenAIChatConfig.ConnectionStringName);
        }

        default:
            throw new NotSupportedException($"AI service '{config.AIChatService}' is not supported.");
    }
}
