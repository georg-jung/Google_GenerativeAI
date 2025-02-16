﻿using GenerativeAI.Clients;
using GenerativeAI.Core;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;

namespace GenerativeAI;

/// <summary>
/// The GoogleAI class serves as the main entry point for interacting with Google's AI models.
/// It provides methods to create generative and embedding models, as well as manage and retrieve model details.
/// This class integrates Google's AI platform capabilities by utilizing appropriate models
/// and configurations to handle generative AI tasks, embeddings, and other related operations.
/// </summary>
public class GoogleAi : GenAI,IGenerativeAI
{
    /// <summary>
    /// Implements a client adapter for Google's AI platform to facilitate integration with its generative AI services.
    /// Provides configuration and methods for managing credentials, setting API versions, creating task URLs,
    /// and authorizing requests for secure access to Google's AI APIs.
    /// </summary>
    public GoogleAi(string? apiKey =null, string? accessToken = null, HttpClient? client = null, ILogger? logger = null) :
        this(new GoogleAIPlatformAdapter(apiKey, accessToken:accessToken), client, logger)
    {
    }

    /// <summary>
    /// Represents a specific implementation of the GenAI base class designed for Google's AI platform.
    /// Offers functionality to interact with Google's generative AI and machine learning models.
    /// Provides support for initialization with various parameters such as API keys, access tokens, HTTP client, and logging.
    /// </summary>
    public GoogleAi(IPlatformAdapter adapter, HttpClient? client = null, ILogger? logger = null) : base(adapter, client,
        logger)
    {
    }

    /// <summary>
    /// Instantiates a new generative model using the specified parameters.
    /// This method initializes and configures the generative model for AI-driven content generation,
    /// providing additional customization options through configuration and safety settings.
    /// </summary>
    /// <param name="modelName">The name of the generative model to create.</param>
    /// <param name="config">An optional configuration for generation settings, which may include parameters such as temperature, max tokens, or top-p values.</param>
    /// <param name="safetyRatings">An optional collection of safety settings that define acceptable content safety parameters and thresholds.</param>
    /// <param name="systemInstruction">An optional system instruction to guide the generative model's initialization or behavior.</param>
    /// <returns>Returns an instance of a configured <see cref="GenerativeModel"/>.</returns>
    public override GenerativeModel CreateGenerativeModel(string modelName, GenerationConfig? config = null,
        ICollection<SafetySetting>? safetyRatings = null, string? systemInstruction = null)
    {
        return new GeminiModel(this.Platform, modelName, config, safetyRatings, systemInstruction, this.HttpClient,
            this.Logger);
    }

    /// <summary>
    /// Creates a new instance of the GeminiModel class, configured with the specified model name, generation settings,
    /// safety settings, system instructions, and optional HTTP client and logger for enhanced AI functionality.
    /// GeminiModel provides an interface to interact with specific generative AI models on the Google AI platform,
    /// offering tailored content generation capabilities.
    /// </summary>
    /// <param name="modelName">The name of the Gemini model to be created. It identifies the specific type of AI model to interact with.</param>
    /// <param name="config">Optional configuration details for generation, including parameters such as temperature, max tokens, and other settings.</param>
    /// <param name="safetyRatings">An optional collection of safety settings to restrict the model's output based on defined criteria.</param>
    /// <param name="systemInstruction">Optional system-level instruction or guidelines provided to influence the model's behavior.</param>
    /// <returns>A new instance of the GeminiModel class, configured with the provided inputs for generative AI tasks.</returns>
    public GeminiModel CreateGeminiModel(string modelName, GenerationConfig? config = null,
        ICollection<SafetySetting>? safetyRatings = null, string? systemInstruction = null)
    {
        return new GeminiModel(this.Platform, modelName, config, safetyRatings, systemInstruction, this.HttpClient,
            this.Logger);
    }
}