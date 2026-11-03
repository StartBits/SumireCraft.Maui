namespace SumireCraft.Maui.Exceptions;

/// <summary>
/// Turns what the last-resort hooks hand out into an <see cref="ExceptionReport"/>.
/// </summary>
/// <remarks>
/// Kept apart from <see cref="SafetyNet"/> on purpose. Subscribing to
/// <see cref="AppDomain.UnhandledException"/> cannot be exercised by a test -- raising it would
/// take the test host down with it -- so everything that involves a decision lives here instead,
/// where it is an ordinary function with ordinary tests.
/// </remarks>
internal static class UnhandledExceptionTranslator
{
    /// <summary>Names the hook in <see cref="ExceptionReport.Source"/>.</summary>
    internal const string AppDomainSource = "AppDomain.UnhandledException";

    /// <inheritdoc cref="AppDomainSource"/>
    internal const string UnobservedTaskSource = "TaskScheduler.UnobservedTaskException";

    /// <inheritdoc cref="AppDomainSource"/>
    internal const string PlatformApplicationSource = "Application.UnhandledException";

    /// <summary>
    /// Translates what <see cref="AppDomain.UnhandledException"/> reported.
    /// </summary>
    internal static ExceptionReport FromAppDomain(object? exceptionObject) =>
        new(AsException(exceptionObject), ExceptionOrigin.Unhandled, AppDomainSource);

    /// <summary>
    /// Translates what <see cref="TaskScheduler.UnobservedTaskException"/> reported.
    /// </summary>
    internal static ExceptionReport FromUnobservedTask(AggregateException? exception) =>
        new(Unwrap(exception), ExceptionOrigin.Unhandled, UnobservedTaskSource);

    /// <summary>
    /// Translates what a platform's own application-level hook reported.
    /// </summary>
    internal static ExceptionReport FromPlatformApplication(Exception? exception) =>
        new(exception ?? NothingToDescribe(), ExceptionOrigin.Unhandled, PlatformApplicationSource);

    /// <summary>
    /// Makes sense of <see cref="UnhandledExceptionEventArgs.ExceptionObject"/>, which is typed as
    /// <see cref="object"/> because a language other than C# is free to throw anything at all.
    /// </summary>
    private static Exception AsException(object? thrown) => thrown switch
    {
        Exception exception => exception,
        null => NothingToDescribe(),
        _ => new InvalidOperationException($"An object that is not an exception was thrown: {thrown}"),
    };

    /// <summary>
    /// Reduces the aggregate a faulted task carries to the failure it actually describes.
    /// </summary>
    /// <remarks>
    /// One failure should read as one failure. The aggregate is how the task infrastructure carries
    /// exceptions around, not information about what went wrong, and a log full of
    /// <see cref="AggregateException"/> wrappers hides the line that matters. An aggregate holding
    /// more than one failure is kept whole, because then the wrapper is the information.
    /// </remarks>
    private static Exception Unwrap(AggregateException? aggregate)
    {
        if (aggregate is null)
        {
            return NothingToDescribe();
        }

        var flattened = aggregate.Flatten();

        return flattened.InnerExceptions.Count == 1 ? flattened.InnerExceptions[0] : flattened;
    }

    /// <summary>
    /// Stands in when a hook reports a failure but hands over nothing that describes it, so that a
    /// report is still produced rather than the event being dropped.
    /// </summary>
    private static InvalidOperationException NothingToDescribe() =>
        new("An exception was reported without anything to describe it.");
}
