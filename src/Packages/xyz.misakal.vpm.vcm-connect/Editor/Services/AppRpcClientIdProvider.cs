using System;
using System.IO;
using System.Threading.Tasks;
using VRChatContentManagerConnect.Editor.Services.Rpc;

namespace VRChatContentManagerConnect.Editor.Services;

internal sealed class AppRpcClientIdProvider : IRpcClientIdProvider {
    public string GetClientId() {
        var clientIdFile = Path.Combine(AppStorageService.GetStoragePath(), "client-id");

        if (!File.Exists(clientIdFile)) {
            var clientId = Guid.NewGuid().ToString("D");
            File.WriteAllText(clientIdFile, clientId);

            return clientId;
        }

        return File.ReadAllText(clientIdFile);
    }
}