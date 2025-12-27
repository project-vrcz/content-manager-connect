using System.Threading.Tasks;
using VRChatContentPublisherConnect.Editor.Models;

namespace VRChatContentPublisherConnect.Editor.Services.Rpc;

internal interface IRpcClientSessionProvider {
    ValueTask<RpcClientSession?> GetSessionsAsync();
    Task SetSessionAsync(RpcClientSession session);
    Task RemoveSessionAsync();
}