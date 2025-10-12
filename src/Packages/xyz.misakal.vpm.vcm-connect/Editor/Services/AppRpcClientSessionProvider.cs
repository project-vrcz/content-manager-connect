using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using VRChatContentManagerConnect.Editor.Models;
using VRChatContentManagerConnect.Editor.Services.Rpc;

namespace VRChatContentManagerConnect.Editor.Services;

internal sealed class AppRpcClientSessionProvider : IRpcClientSessionProvider {
    public async ValueTask<RpcClientSession?> GetSessionsAsync() {
        var path = GetSessionStoragePath();

        if (!File.Exists(path))
            return null;

        var raw = await File.ReadAllTextAsync(path);

        try {
            return JsonSerializer.Deserialize<RpcClientSession>(raw);
        }
        catch {
            return null;
        }
    }

    public async Task SetSessionAsync(RpcClientSession session) {
        var path = GetSessionStoragePath();
        var raw = JsonSerializer.Serialize(session);

        await File.WriteAllTextAsync(path, raw);
    }

    public Task RemoveSessionAsync() {
        File.Delete(GetSessionStoragePath());
        return Task.CompletedTask;
    }

    private string GetSessionStoragePath() {
        return Path.Combine(AppStorageService.GetStoragePath(), "rpc-client-session.json");
    }
}