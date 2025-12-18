using System;

namespace VRChatContentManagerConnect.Editor.Exceptions;

internal sealed class InvalidResponseException : Exception {
    public InvalidResponseException() : base("The response from the server was invalid.") { }
}