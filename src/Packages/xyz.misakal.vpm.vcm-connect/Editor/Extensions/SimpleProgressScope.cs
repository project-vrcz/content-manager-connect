using System;
using UnityEditor;

namespace VRChatContentPublisherConnect.Editor.Extensions;

internal sealed class SimpleProgressScope : IDisposable {
    private readonly int _progressId;
    private readonly bool _showDialog;

    private readonly string _title;

    private bool _isDisposed;

    public SimpleProgressScope(string title, string? description = null, bool showDialog = false) {
        _title = title;
        _showDialog = showDialog;
        _progressId =
            Progress.Start(title, description, Progress.Options.Indefinite | Progress.Options.Managed);

        Progress.SetTimeDisplayMode(_progressId, Progress.TimeDisplayMode.NoTimeShown);
        Progress.SetPriority(_progressId, Progress.Priority.Idle);
    }

    public void Report(string description) {
        Progress.SetDescription(_progressId, description);

        if (_showDialog) {
            MainThreadDispatcher.Dispatch(() =>
                EditorUtility.DisplayProgressBar(_title, description, 0));
        }
    }

    public void Dispose() {
        if (_isDisposed)
            return;

        if (_showDialog)
            MainThreadDispatcher.Dispatch(EditorUtility.ClearProgressBar);

        Progress.Remove(_progressId);
        _isDisposed = true;
    }
}