using System.Threading.Tasks;
using VRChatContentManagerConnect.Editor.Models;

namespace VRChatContentManagerConnect.Editor.Services;

internal interface IRpcClientSessionProvider {
    ValueTask<RpcClientSession?> GetSessionsAsync();
    Task SetSessionAsync(RpcClientSession session);
    Task RemoveSessionAsync();
}