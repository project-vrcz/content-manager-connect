using System;
using System.IO;
using VRChatContentPublisherConnect.Editor.Services.Rpc;
using VRChatContentPublisherConnect.Editor.Utils;

namespace VRChatContentPublisherConnect.Editor.Services;

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

    public string GetClientName() {
        var clientNameFile = GetClientNameFilePath();

        if (!File.Exists(clientNameFile)) {
            var clientName = RandomWordsUtils.GetRandomWords();
            File.WriteAllText(clientNameFile, clientName);

            return clientName;
        }

        return File.ReadAllText(clientNameFile);
    }

    public void SetClientName(string name) {
        File.WriteAllText(GetClientNameFilePath(), name);
    }

    private string GetClientNameFilePath() {
        return Path.Combine(AppStorageService.GetStoragePath(), "client-name");
    }
}