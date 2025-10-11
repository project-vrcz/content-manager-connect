using System.Threading.Tasks;

namespace VRChatContentManagerConnect.Editor.Services;

internal interface IRpcClientIdProvider {
    string GetClientId();
}