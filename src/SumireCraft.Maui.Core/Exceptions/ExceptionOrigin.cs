namespace SumireCraft.Maui.Exceptions;

/// <summary>
/// Says what the application was doing when an exception surfaced.
/// </summary>
/// <remarks>
/// A handler usually cares more about this than about the exception type. Telling the user that
/// something failed is right when they just asked for it and wrong when nothing was on screen but
/// their own content, and no exception type can make that distinction.
/// </remarks>
public enum ExceptionOrigin
{
    /// <summary>
    /// The user asked for something and it failed: a command, a button, a gesture. Somebody is
    /// waiting in front of the screen for an answer.
    /// </summary>
    Command,

    /// <summary>
    /// Work the application started on its own, with nobody waiting for it.
    /// </summary>
    Background,

    /// <summary>
    /// An exception that nothing caught, picked up by a last-resort handler. The application may
    /// already be on its way down.
    /// </summary>
    Unhandled,
}
