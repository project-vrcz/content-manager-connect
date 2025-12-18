using System;

namespace VRChatContentManagerConnect.Editor.Exceptions;

internal sealed class InvalidStatusCodeException : Exception {
    public InvalidStatusCodeException() : base("Response returned an invalid status code.") { }
}