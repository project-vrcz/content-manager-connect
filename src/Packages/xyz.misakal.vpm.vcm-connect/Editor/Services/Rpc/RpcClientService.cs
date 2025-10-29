using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using UnityEngine;
using VRChatContentManagerConnect.Editor.Models;
using VRChatContentManagerConnect.Editor.Models.RpcApi.Request;
using VRChatContentManagerConnect.Editor.Models.RpcApi.Request.Task;
using VRChatContentManagerConnect.Editor.Models.RpcApi.Response;
using Random = System.Random;

namespace VRChatContentManagerConnect.Editor.Services.Rpc;

internal sealed class RpcClientService {
    public event EventHandler<RpcClientState>? StateChanged;
    public event EventHandler<string>? IdentityPromptChanged;

    public RpcClientState State { get; private set; } = RpcClientState.Disconnected;

    private readonly string _clientId;

    private readonly IRpcClientSessionProvider _sessionProvider;
    private string? _token;

    private Uri? _baseUrl;
    private string? _identityPrompt;

    private readonly HttpClient _httpClient;

    private readonly JsonSerializerOptions _serializerOptions = new() {
        RespectNullableAnnotations = true
    };

    public RpcClientService(IRpcClientIdProvider clientIdProvider, IRpcClientSessionProvider sessionProvider) {
        _sessionProvider = sessionProvider;
        _clientId = clientIdProvider.GetClientId();
        _httpClient = new HttpClient();

        _httpClient.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue(new ProductHeaderValue("VRChatContentManager.ConnectEditorApp", "snapshot")));
    }

    public async Task RestoreSessionAsync() {
        var session = await _sessionProvider.GetSessionsAsync();

        if (session is null) {
            throw new InvalidOperationException("No session to restore.");
        }

        var hostUri = new Uri(session.Host);
        var request = new HttpRequestMessage(HttpMethod.Get, new Uri(hostUri, "/v1/auth/metadata"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.Token);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var metadata = await response.Content.ReadFromJsonAsync<AuthMetadataResponse>();
        if (metadata is null)
            throw new Exception("Invalid response from server.");

        _httpClient.BaseAddress = hostUri;
        _token = session.Token;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        _baseUrl = hostUri;

        ChangeState(RpcClientState.Connected);
    }

    public string GetClientId() {
        return _clientId;
    }

    public string? GetIdentityPrompt() {
        return _identityPrompt;
    }

    public async ValueTask<string> RequestChallengeAsync(string baseUrl) {
        await DisconnectAsync();

        var baseUri = new Uri(baseUrl);

        var identityPrompt = SetRandomIdentityPrompt();

        var requestUri = new Uri(baseUri, "/v1/auth/request-challenge");
        var requestBody = new RequestChallengeRequest(_clientId, identityPrompt);
        var response = await _httpClient.PostAsJsonAsync(requestUri, requestBody, _serializerOptions);

        if (response.StatusCode != HttpStatusCode.NoContent) {
            throw new Exception("Unexpected response status code: " + response.StatusCode);
        }

        _baseUrl = baseUri;
        _httpClient.BaseAddress = baseUri;
        ChangeState(RpcClientState.AwaitingChallenge);

        return identityPrompt;
    }

    public async Task CompleteChallengeAsync(string code) {
        if (State != RpcClientState.AwaitingChallenge)
            throw new InvalidOperationException("Client is not awaiting a challenge.");
        if (_baseUrl is null)
            throw new InvalidOperationException("Base URL is not set. Call RequestChallengeAsync first.");
        if (_identityPrompt is null)
            throw new InvalidOperationException("IdentityPrompt is not set. Call RequestChallengeAsync first.");

        var requestBody = new ChallengeRequest(_clientId, code, _identityPrompt);
        var response = await _httpClient.PostAsJsonAsync("/v1/auth/challenge", requestBody, _serializerOptions);

        if (!response.IsSuccessStatusCode) {
            throw new Exception("Challenge failed with status code: " + response.StatusCode);
        }

        var responseBody = await response.Content.ReadFromJsonAsync<ChallengeResponse>(_serializerOptions);
        if (responseBody is null) {
            throw new Exception("Invalid response from server.");
        }

        _token = responseBody.Token;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

        try {
            await GetAuthMetadataAsyncCore();
            await _sessionProvider.SetSessionAsync(new RpcClientSession(_baseUrl.ToString(), _token));
        }
        catch (Exception ex) {
            Debug.LogException(ex);
            Debug.LogError("Failed to validate token, disconnecting.");

            await DisconnectAsync();
            return;
        }

        ChangeState(RpcClientState.Connected);
    }

    private async ValueTask<AuthMetadataResponse> GetAuthMetadataAsyncCore() {
        var response = await _httpClient.GetFromJsonAsync<AuthMetadataResponse>("/v1/auth/metadata");
        if (response is null) {
            throw new Exception("Invalid response from server.");
        }

        return response;
    }

    internal async ValueTask<string> UploadFileAsync(string filePath, string fileName) {
        await using var fileStream = File.OpenRead(filePath);

        using var content = new MultipartFormDataContent();

        using var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        content.Add(fileContent, "file", fileName);

        var response = await SendAsync(new HttpRequestMessage(HttpMethod.Post, "/v1/files") {
            Content = content
        });

        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadFromJsonAsync<UploadFileResponse>(_serializerOptions);
        if (responseBody is null) {
            throw new Exception("Invalid response from server.");
        }

        return responseBody.FileId;
    }

    internal async ValueTask CreateWorldPublishTaskAsync(
        string worldId, string bundleFileId, string worldName, string platform, string unityVersion,
        string? worldSignature) {
        var requestBody =
            new CreateWorldPublishTaskRequest(worldId, bundleFileId, worldName, platform, unityVersion, worldSignature);

        var response = await SendAsync(new HttpRequestMessage(HttpMethod.Post, "/v1/tasks/world") {
            Content = JsonContent.Create(requestBody)
        });
        response.EnsureSuccessStatusCode();
    }

    internal async ValueTask CreateAvatarPublishTaskAsync(
        string avatarId, string bundleFileId, string avatarName, string platform, string unityVersion) {
        var requestBody =
            new CreateAvatarPublishTaskRequest(avatarId, bundleFileId, avatarName, platform, unityVersion);

        var response = await SendAsync(new HttpRequestMessage(HttpMethod.Post, "/v1/tasks/avatar") {
            Content = JsonContent.Create(requestBody)
        });

        response.EnsureSuccessStatusCode();
    }

    internal async ValueTask<HttpResponseMessage> SendAsync(HttpRequestMessage request) {
        if (State != RpcClientState.Connected)
            throw new InvalidOperationException("Client is not connected.");
        if (_baseUrl is null)
            throw new InvalidOperationException("Base URL is not set. Call RequestChallengeAsync first.");
        if (_token is null)
            throw new InvalidOperationException("Not authenticated. Call CompleteChallengeAsync first.");

        var response = await _httpClient.SendAsync(request);
        if (response.StatusCode == HttpStatusCode.Unauthorized) {
            Debug.LogWarning("Unauthorized response, disconnecting.");
            await DisconnectAsync();

            throw new InvalidOperationException("Unauthorized response.");
        }

        return response;
    }

    public async Task DisconnectAsync() {
        ChangeState(RpcClientState.Disconnected);
        _token = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;
        _baseUrl = null;
        _httpClient.BaseAddress = null;

        await _sessionProvider.RemoveSessionAsync();

        await Task.CompletedTask;
    }

    private void ChangeState(RpcClientState newState) {
        if (State == newState) return;

        State = newState;
        StateChanged?.Invoke(this, newState);
    }

    private string SetRandomIdentityPrompt() {
        var words = new[] {
            "Red", "Blue", "Green", "Yellow", "Purple", "Orange", "Black", "White", "Gray", "Silver",
            "Gold", "Bronze", "Copper", "Iron", "Steel", "Wooden", "Plastic", "Glass", "Crystal", "Diamond"
        };

        var random = new Random();
        var word1 = words[random.Next(words.Length)];
        var word2 = words[random.Next(words.Length)];

        _identityPrompt = $"{word1} {word2}";
        IdentityPromptChanged?.Invoke(this, _identityPrompt);
        return _identityPrompt;
    }
}

internal enum RpcClientState {
    Disconnected,
    AwaitingChallenge,
    Connected
}