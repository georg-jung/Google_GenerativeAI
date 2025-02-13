﻿using System.Text;
using GenerativeAI.Core;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;

namespace GenerativeAI;

public class ChatSession : GenerativeModel
{
    #region Properties
    private List<Content> _history = new List<Content>();

    /// <summary>
    /// Gets the content of the last request made in the current chat session,
    /// allowing access to the most recently sent message or query.
    /// This property is updated each time a request is successfully made within the session.
    /// </summary>
    /// <remarks>
    /// The value of this property is `null` if no request has been made or if the session
    /// was reset by assigning a new history to the <see cref="History"/> property.
    /// </remarks>
    public Content? LastRequestContent { get; private set; }

    /// <summary>
    /// Gets the content of the last response received in the current chat session,
    /// providing access to the most recently generated reply or output from the model.
    /// This property is updated each time a response is successfully received within the session.
    /// </summary>
    /// <remarks>
    /// The value of this property is `null` if no response has been received or if the session
    /// was reset by assigning a new history to the <see cref="History"/> property.
    /// </remarks>
    public Content? LastResponseContent { get; private set; }

    /// <summary>
    /// Gets or sets the history of the current chat session, representing a collection of
    /// user and model interactions recorded throughout the session.
    /// Updates to this property will reset both <see cref="LastRequestContent"/> and <see cref="LastResponseContent"/>.
    /// </summary>
    /// <remarks>
    /// Assigning a new value to this property effectively resets the session, clearing any previously tracked
    /// request or response content. The history contains instances of <see cref="Content"/> that represent
    /// messages exchanged between the user and the model.
    /// </remarks>
    public List<Content> History
    {
        get => this._history;
        set
        {
            this._history = value;
            this.LastRequestContent = null;
            this.LastResponseContent = null;
        }
    }

    #endregion
    #region Constructors

    public ChatSession(List<Content>? history, IPlatformAdapter platform, string model, GenerationConfig config = null,
        ICollection<SafetySetting>? safetySettings = null, string? systemInstruction = null,
        HttpClient? httpClient = null, ILogger? logger = null) : base(platform, model, config, safetySettings,
        systemInstruction, httpClient, logger)
    {
        History = history ?? new();
    }

    public ChatSession(List<Content>? history,string apiKey, ModelParams modelParams, HttpClient? client = null, ILogger? logger = null) :
        base(apiKey, modelParams, client, logger)
    {
        History = history ?? new();
    }

    public ChatSession(List<Content>? history, string apiKey, string model, GenerationConfig config = null,
        ICollection<SafetySetting>? safetySettings = null, string? systemInstruction = null,
        HttpClient? httpClient = null, ILogger? logger = null) : base(apiKey, model, config, safetySettings,
        systemInstruction, httpClient, logger)
    {
        History = history ?? new();
    }

    #endregion
    
    /// <inheritdoc />
    protected override void PrepareRequest(GenerateContentRequest request)
    {
        request.Contents.InsertRange(0, History);
        base.PrepareRequest(request);
    }


    /// <inheritdoc />
    public override async Task<GenerateContentResponse> GenerateContentAsync(GenerateContentRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await base.GenerateContentAsync(request, cancellationToken);

        UpdateHistory(request, response);
        return response;
    }
    /// <inheritdoc />
    public override async IAsyncEnumerable<GenerateContentResponse> StreamContentAsync(GenerateContentRequest request, CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder();
        await foreach (var response in base.StreamContentAsync(request,cancellationToken))
        {
            if (cancellationToken.IsCancellationRequested)
                yield break;
            
            sb.Append(response.Text());
            yield return response;
        }

        var lastRequestContent = request.Contents.Last();
        var lastResponseContent = RequestExtensions.FormatGenerateContentInput(sb.ToString(), Roles.Model);
        
        UpdateHistory(lastRequestContent, lastResponseContent);
    }

    private void UpdateHistory(GenerateContentRequest request, GenerateContentResponse response)
    {
        if (response.Candidates is { Length: > 0 } && response.Candidates[0].Content != null)
        {
            var lastRequestContent = request.Contents.Last();
            var lastResponseContent = response.Candidates?[0].Content;
            if (lastResponseContent != null)
            {
                UpdateHistory(lastRequestContent, lastResponseContent);
            }
        }
       
    }
    private void UpdateHistory(Content lastRequestContent, Content lastResponseContent)
    {
        lastRequestContent.Role ??= Roles.User;
        lastResponseContent.Role ??= Roles.Model;
        History.Add(lastRequestContent);
        History.Add(lastResponseContent);
        this.LastRequestContent = lastRequestContent;
        this.LastResponseContent = lastResponseContent;
    }
}