namespace VRChatContentPublisherConnect.Editor.Services.Rpc;

internal interface IRpcClientIdProvider {
    string GetClientId();
    string GetClientName();
    void SetClientName(string name);
}