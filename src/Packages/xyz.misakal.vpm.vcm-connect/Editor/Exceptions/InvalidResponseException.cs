using System;

namespace VRChatContentPublisherConnect.Editor.Exceptions;

internal sealed class InvalidResponseException : Exception {
    public InvalidResponseException() : base("The response from the server was invalid.") { }
}