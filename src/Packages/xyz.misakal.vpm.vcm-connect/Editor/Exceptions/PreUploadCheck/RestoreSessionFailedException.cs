using System;

namespace VRChatContentPublisherConnect.Editor.Exceptions.PreUploadCheck;

public sealed class RestoreSessionFailedException : Exception {
    public RestoreSessionFailedException(Exception innerException) : base(
        "RPC Client is not connected and failed to restore session.", innerException
    ) { }
}