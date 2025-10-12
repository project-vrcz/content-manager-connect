using System.Threading.Tasks;
using VRChatContentManagerConnect.Editor.Models;

namespace VRChatContentManagerConnect.Editor.Services.Rpc;

internal interface IRpcClientSessionProvider {
    ValueTask<RpcClientSession?> GetSessionsAsync();
    Task SetSessionAsync(RpcClientSession session);
    Task RemoveSessionAsync();
}