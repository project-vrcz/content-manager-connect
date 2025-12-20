using System;

namespace VRChatContentManagerConnect.Editor.Exceptions.PreUploadCheck;

public sealed class NoSessionToRestoreException : Exception {
    public NoSessionToRestoreException() : base("RPC Client is not connected. And no session to restore.") { }
}